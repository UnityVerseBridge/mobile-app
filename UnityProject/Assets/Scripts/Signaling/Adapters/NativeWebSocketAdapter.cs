using System;
using System.Threading.Tasks; // Task 사용
using UnityEngine; // Debug 사용
using NativeWebSocket; // 실제 NativeWebSocket 라이브러리 사용
using UnityVerseBridge.Core.Signaling;

// 네임스페이스 충돌을 피하기 위해 우리가 정의한 State에 별칭(alias) 부여 (선택적이지만 편리함)
using CoreWebSocketState = UnityVerseBridge.Core.Signaling.WebSocketState;

namespace UnityVerseBridge.MobileApp.Signaling
{
    /// <summary>
    /// 독립 실행형 NativeWebSocket 라이브러리를 사용하여 IWebSocketClient 인터페이스를 구현하는 어댑터 클래스입니다.
    /// Meta SDK가 없는 환경(예: mobile-app)에서 기본 WebSocket 통신을 제공합니다.
    /// </summary>
    public class NativeWebSocketAdapter : IWebSocketClient, IDisposable // IDisposable 추가 고려
    {
        private WebSocket nativeWebSocket; // NativeWebSocket 라이브러리의 실제 객체
        private CoreWebSocketState currentState = CoreWebSocketState.Closed; // 내부 상태 추적

        // --- IWebSocketClient 인터페이스 이벤트 구현 ---
        public event Action OnOpen;
        public event Action<byte[]> OnMessage;
        public event Action<string> OnError;
        public event Action<ushort> OnClose;

        // --- IWebSocketClient 인터페이스 속성 구현 ---
        public CoreWebSocketState State => currentState;

        // --- IWebSocketClient 인터페이스 메서드 구현 ---

        /// <summary>
        /// 지정된 URL로 WebSocket 연결을 시도합니다.
        /// </summary>
        public async Task Connect(string url)
        {
            // 이미 연결 중이거나 열려 있으면 중복 실행 방지
            if (currentState == CoreWebSocketState.Connecting || currentState == CoreWebSocketState.Open)
            {
                Debug.LogWarning($"[NWAdapter] Already connecting or open to {url}.");
                return;
            }

            // 이전 연결이 남아있을 수 있으므로 정리
            await CleanupPreviousConnection();

            currentState = CoreWebSocketState.Connecting;
            Debug.Log($"[NWAdapter] Connecting to {url}...");

            try
            {
                nativeWebSocket = new WebSocket(url);

                // NativeWebSocket 이벤트에 내부 핸들러 연결
                nativeWebSocket.OnOpen += HandleNWOpen;
                nativeWebSocket.OnMessage += HandleNWMessage;
                nativeWebSocket.OnError += HandleNWError;
                nativeWebSocket.OnClose += HandleNWClose;

                // NativeWebSocket의 Connect는 async void일 수 있으므로 await하지 않음.
                // 연결 상태는 OnOpen, OnError, OnClose 이벤트를 통해 업데이트됨.
                #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                nativeWebSocket.Connect();
                #pragma warning restore CS4014
            }
            catch (Exception e)
            {
                HandleNWError($"Exception during WebSocket creation/connection: {e.Message}");
                // Task 반환 인터페이스를 만족시키기 위해 완료된 Task 반환 (실제로는 에러 처리)
                await Task.CompletedTask;
            }
             // Connect 호출 후 즉시 Task 완료 (연결 완료는 이벤트로 확인)
             await Task.CompletedTask;
        }

        /// <summary>
        /// 현재 WebSocket 연결을 닫습니다.
        /// </summary>
        public async Task Close()
        {
            if (nativeWebSocket != null && currentState != CoreWebSocketState.Closing && currentState != CoreWebSocketState.Closed)
            {
                currentState = CoreWebSocketState.Closing;
                Debug.Log("[NWAdapter] Closing WebSocket connection...");
                await nativeWebSocket.Close(); // NativeWebSocket의 Close는 async Task
                // 실제 정리는 OnClose 이벤트 핸들러에서 수행됨
            }
            else
            {
                 // 이미 닫혔거나 없는 경우 즉시 완료
                 await Task.CompletedTask;
            }
        }

        /// <summary>
        /// 바이트 배열 데이터를 전송합니다.
        /// </summary>
        public async Task Send(byte[] bytes)
        {
            if (State == CoreWebSocketState.Open && nativeWebSocket != null)
            {
                // Debug.Log($"[NWAdapter] Sending {bytes.Length} bytes."); // 너무 빈번하면 주석처리
                await nativeWebSocket.Send(bytes);
            }
            else
            {
                Debug.LogWarning("[NWAdapter] Cannot send bytes, WebSocket is not open.");
                await Task.CompletedTask; // 실패 시에도 Task 반환
            }
        }

        /// <summary>
        /// 텍스트 데이터를 전송합니다. (내부적으로 UTF8 바이트 변환 후 Send 호출)
        /// </summary>
        public async Task SendText(string message)
        {
            if (State == CoreWebSocketState.Open && nativeWebSocket != null)
            {
                // Debug.Log($"[NWAdapter] Sending text: {message}"); // 너무 빈번하면 주석처리
                await nativeWebSocket.SendText(message);
            }
            else
            {
                Debug.LogWarning("[NWAdapter] Cannot send text, WebSocket is not open.");
                await Task.CompletedTask; // 실패 시에도 Task 반환
            }
        }

        /// <summary>
        /// NativeWebSocket의 메시지 큐를 처리합니다. 외부(SignalingClient 또는 MonoBehaviour)에서 주기적으로 호출되어야 합니다.
        /// </summary>
        public void DispatchMessageQueue()
        {
            // NativeWebSocket v2.x.x 기준으로는 Update 루프에서 호출 필요
            // v1.x.x 에서는 필요 없을 수 있음 - 사용하는 라이브러리 버전 확인 필요
             #if !UNITY_WEBGL || UNITY_EDITOR // WebGL 제외 또는 Editor에서만 (NativeWebSocket 문서 참조)
                 nativeWebSocket?.DispatchMessageQueue();
             #endif
        }

        // --- NativeWebSocket 이벤트 핸들러 (Private) ---

        private void HandleNWOpen()
        {
            currentState = CoreWebSocketState.Open;
            Debug.Log("[NWAdapter] WebSocket connection opened.");
            OnOpen?.Invoke(); // IWebSocketClient 이벤트 발생
        }

        private void HandleNWMessage(byte[] bytes)
        {
            // Debug.Log($"[NWAdapter] Message received ({bytes.Length} bytes)."); // 너무 빈번하면 주석처리
            OnMessage?.Invoke(bytes); // IWebSocketClient 이벤트 발생
        }

        private void HandleNWError(string errorMsg)
        {
            // 상태 업데이트는 OnClose에서 처리될 수 있으므로 여기서는 에러 로그 및 이벤트 발생만
            Debug.LogError($"[NWAdapter] WebSocket error: {errorMsg}");
            OnError?.Invoke(errorMsg); // IWebSocketClient 이벤트 발생
            // 중요: 에러 발생 후 NativeWebSocket이 자동으로 Close 이벤트를 호출하는지 확인 필요
            // 만약 호출하지 않는다면 여기서 강제로 상태 변경 및 OnClose 호출 필요할 수 있음
             if(currentState != CoreWebSocketState.Closed && currentState != CoreWebSocketState.Closing)
             {
                 // HandleNWClose((ushort)WebSocketCloseCode.Abnormal); // 예시: 비정상 종료 코드로 Close 처리
             }
        }

        private void HandleNWClose(NativeWebSocket.WebSocketCloseCode closeCode)
        {
            // 이미 정리 중이거나 닫힌 상태면 무시
            if (currentState == CoreWebSocketState.Closed) return;

            currentState = CoreWebSocketState.Closed;
            Debug.Log($"[NWAdapter] WebSocket connection closed. Code: {closeCode}");
            OnClose?.Invoke((ushort)closeCode); // IWebSocketClient 이벤트 발생
            CleanupWebSocketInstance(); // 리소스 정리
        }

        /// <summary>
        /// NativeWebSocket 인스턴스 정리 및 이벤트 구독 해지
        /// </summary>
        private async Task CleanupPreviousConnection()
        {
             if (nativeWebSocket != null)
             {
                  // 기존 연결 닫기 시도 (이미 닫혔을 수도 있음)
                 var previousSocket = nativeWebSocket;
                 nativeWebSocket = null; // 참조를 먼저 제거하여 새 연결 시도 중 문제 방지

                 // 이벤트 핸들러 제거
                 previousSocket.OnOpen -= HandleNWOpen;
                 previousSocket.OnMessage -= HandleNWMessage;
                 previousSocket.OnError -= HandleNWError;
                 previousSocket.OnClose -= HandleNWClose;

                 if (previousSocket.State != NativeWebSocket.WebSocketState.Closed)
                 {
                     Debug.Log("[NWAdapter] Cleaning up previous WebSocket connection...");
                     await previousSocket.Close();
                 }
             }
        }

         private void CleanupWebSocketInstance()
         {
             if (nativeWebSocket == null) return;

             // 이벤트 핸들러 제거 (메모리 누수 방지)
             nativeWebSocket.OnOpen -= HandleNWOpen;
             nativeWebSocket.OnMessage -= HandleNWMessage;
             nativeWebSocket.OnError -= HandleNWError;
             nativeWebSocket.OnClose -= HandleNWClose;
             nativeWebSocket = null;
             Debug.Log("[NWAdapter] WebSocket instance cleaned up.");
         }


        // --- IDisposable 구현 (선택 사항이지만 권장) ---
        public void Dispose()
        {
            // 비동기 Close 호출 후 완료를 기다리지 않고 정리 시작
            _ = Close(); // Close 호출 시작
             CleanupWebSocketInstance(); // 즉시 정리 시도
             GC.SuppressFinalize(this); // Finalizer 호출 방지
        }

         // ~NativeWebSocketAdapter() // Finalizer (비권장: 관리되지 않는 리소스가 없다면 불필요)
         // {
         //      Dispose();
         // }
    }
}