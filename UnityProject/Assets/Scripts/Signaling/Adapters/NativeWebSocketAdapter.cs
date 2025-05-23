using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityVerseBridge.Core.Signaling;
using UnityVerseBridge.Core.Signaling.Data;
// 네임스페이스 충돌 해결을 위한 별칭 사용
using NativeWS = NativeWebSocket;
using CoreWS = UnityVerseBridge.Core.Signaling;

namespace UnityVerseBridge.MobileApp.Signaling
{
    /// <summary>
    /// NativeWebSocket 라이브러리를 사용하여 ISignalingClient 및 IWebSocketClient 인터페이스를 구현합니다.
    /// </summary>
    public class NativeWebSocketAdapter : ISignalingClient, IWebSocketClient
    {
        // NativeWebSocket.WebSocket 인스턴스를 사용하여 WebSocket 통신 수행
        private NativeWS.WebSocket webSocket;

        // --- ISignalingClient 이벤트 구현 ---
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string, string> OnSignalingMessageReceived;

        // --- IWebSocketClient 이벤트 구현 ---
        public event Action OnOpen;
        public event Action<byte[]> OnMessage;
        public event Action<string> OnError;
        public event Action<ushort> OnClose;

        // --- 속성 구현 ---
        // ISignalingClient 속성
        public bool IsConnected => webSocket != null && webSocket.State == NativeWS.WebSocketState.Open;
        
        // IWebSocketClient 속성
        public CoreWS.WebSocketState State
        {
            get
            {
                if (webSocket == null) return CoreWS.WebSocketState.Closed;
                
                // NativeWebSocket.WebSocketState를 Core.WebSocketState로 변환
                switch (webSocket.State)
                {
                    case NativeWS.WebSocketState.Connecting:
                        return CoreWS.WebSocketState.Connecting;
                    case NativeWS.WebSocketState.Open:
                        return CoreWS.WebSocketState.Open;
                    case NativeWS.WebSocketState.Closing:
                        return CoreWS.WebSocketState.Closing;
                    case NativeWS.WebSocketState.Closed:
                    default:
                        return CoreWS.WebSocketState.Closed;
                }
            }
        }

        #region ISignalingClient Implementation

        /// <summary>
        /// 지정된 URL의 시그널링 서버에 비동기적으로 연결을 시도합니다.
        /// </summary>
        public async Task Connect(string url)
        {
            try
            {
                // 이미 연결되어 있는 경우 처리
                if (webSocket != null && (webSocket.State == NativeWS.WebSocketState.Open || webSocket.State == NativeWS.WebSocketState.Connecting))
                {
                    Debug.LogWarning("[NativeWebSocketAdapter] 이미 WebSocket 연결이 진행 중입니다.");
                    return;
                }

                // 새 WebSocket 인스턴스 생성
                webSocket = new NativeWS.WebSocket(url);

                // 이벤트 핸들러 설정
                SetupEventHandlers();

                // 연결 시도
                await webSocket.Connect();
                
                Debug.Log("[NativeWebSocketAdapter] WebSocket 연결 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NativeWebSocketAdapter] 연결 오류: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// WebSocket 연결을 닫습니다.
        /// </summary>
        public async Task Disconnect()
        {
            try
            {
                if (webSocket == null)
                {
                    Debug.LogWarning("[NativeWebSocketAdapter] WebSocket 인스턴스가 없습니다.");
                    return;
                }

                if (webSocket.State == NativeWS.WebSocketState.Closed)
                {
                    Debug.Log("[NativeWebSocketAdapter] WebSocket이 이미 닫혀 있습니다.");
                    return;
                }

                // 연결 종료
                await webSocket.Close();
                
                Debug.Log("[NativeWebSocketAdapter] WebSocket 연결 종료 요청됨");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NativeWebSocketAdapter] 연결 종료 오류: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// 시그널링 메시지 객체를 서버로 전송합니다.
        /// </summary>
        public async Task SendMessage<T>(T message) where T : SignalingMessageBase
        {
            try
            {
                if (webSocket == null || webSocket.State != NativeWS.WebSocketState.Open)
                {
                    Debug.LogWarning("[NativeWebSocketAdapter] WebSocket이 연결되어 있지 않아 메시지를 보낼 수 없습니다.");
                    return;
                }

                // SignalingMessageBase 객체를 JSON 문자열로 변환
                string jsonMessage = JsonUtility.ToJson(message);
                
                // 메시지 전송
                await webSocket.SendText(jsonMessage);
                
                Debug.Log($"[NativeWebSocketAdapter] 메시지 전송: {jsonMessage}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NativeWebSocketAdapter] 메시지 전송 오류: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Update 루프에서 호출되어야 하는 메시지 처리 메서드입니다.
        /// </summary>
        public void DispatchMessages()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (webSocket != null)
            {
                webSocket.DispatchMessageQueue();
            }
#endif
        }

        /// <summary>
        /// IWebSocketClient 인터페이스 구현: 메시지 큐를 처리합니다.
        /// </summary>
        public void DispatchMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (webSocket != null)
            {
                webSocket.DispatchMessageQueue();
            }
#endif
        }

        /// <summary>
        /// ISignalingClient의 필수 구현 - WebSocket 어댑터와 서버 URL을 설정하고 연결을 시작합니다.
        /// </summary>
        public async Task InitializeAndConnect(IWebSocketClient webSocketClient, string serverUrl)
        {
            // 이 어댑터는 직접 WebSocket을 구현하므로 webSocketClient 매개변수는 무시됨
            Debug.Log($"[NativeWebSocketAdapter] InitializeAndConnect: {serverUrl}");
            
            try
            {
                await Connect(serverUrl);
            }
            catch (Exception e)
            {
                Debug.LogError($"[NativeWebSocketAdapter] InitializeAndConnect 오류: {e.Message}");
                throw;
            }
        }

        #endregion

        #region IWebSocketClient Implementation

        /// <summary>
        /// IWebSocketClient 인터페이스 구현: 연결을 닫습니다.
        /// </summary>
        public async Task Close()
        {
            await Disconnect(); // Disconnect 메서드 재사용
        }

        /// <summary>
        /// IWebSocketClient 인터페이스 구현: 바이트 배열 데이터를 전송합니다.
        /// </summary>
        public async Task Send(byte[] bytes)
        {
            if (webSocket == null || webSocket.State != NativeWS.WebSocketState.Open)
            {
                Debug.LogWarning("[NativeWebSocketAdapter] WebSocket이 연결되어 있지 않아 데이터를 보낼 수 없습니다.");
                return;
            }

            await webSocket.Send(bytes);
        }

        /// <summary>
        /// IWebSocketClient 인터페이스 구현: 텍스트 메시지를 전송합니다.
        /// </summary>
        public async Task SendText(string message)
        {
            if (webSocket == null || webSocket.State != NativeWS.WebSocketState.Open)
            {
                Debug.LogWarning("[NativeWebSocketAdapter] WebSocket이 연결되어 있지 않아 메시지를 보낼 수 없습니다.");
                return;
            }

            await webSocket.SendText(message);
        }

        #endregion

        /// <summary>
        /// WebSocket 이벤트 핸들러를 설정합니다.
        /// </summary>
        private void SetupEventHandlers()
        {
            if (webSocket == null) return;

            // 연결 성공 시 호출될 함수 등록
            webSocket.OnOpen += () =>
            {
                Debug.Log("[NativeWebSocketAdapter] WebSocket 연결 성공!");
                OnConnected?.Invoke(); // ISignalingClient 이벤트
                OnOpen?.Invoke();      // IWebSocketClient 이벤트
            };

            // 에러 발생 시 호출될 함수 등록
            webSocket.OnError += (e) =>
            {
                Debug.LogError($"[NativeWebSocketAdapter] WebSocket 오류: {e}");
                OnError?.Invoke(e); // IWebSocketClient 이벤트
            };

            // 연결 종료 시 호출될 함수 등록
            webSocket.OnClose += (e) =>
            {
                Debug.Log($"[NativeWebSocketAdapter] WebSocket 연결 종료 (코드: {e})");
                OnDisconnected?.Invoke(); // ISignalingClient 이벤트
                OnClose?.Invoke((ushort)e); // IWebSocketClient 이벤트
            };

            // 메시지 수신 시 호출될 함수 등록
            webSocket.OnMessage += (byte[] data) =>
            {
                // 바이트 배열을 문자열로 변환
                string message = System.Text.Encoding.UTF8.GetString(data);
                Debug.Log($"[NativeWebSocketAdapter] 메시지 수신: {message}");
                
                // IWebSocketClient 이벤트 발생
                OnMessage?.Invoke(data);
                
                try
                {
                    // 기본 메시지 타입 파악을 위해 SignalingMessageBase로 파싱
                    SignalingMessageBase baseMsg = JsonUtility.FromJson<SignalingMessageBase>(message);
                    if (baseMsg != null && !string.IsNullOrEmpty(baseMsg.type))
                    {
                        // OnSignalingMessageReceived 이벤트를 발생시킴
                        OnSignalingMessageReceived?.Invoke(baseMsg.type, message);
                    }
                    else
                    {
                        Debug.LogWarning($"[NativeWebSocketAdapter] 메시지 타입을 확인할 수 없음: {message}");
                        OnSignalingMessageReceived?.Invoke("unknown", message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[NativeWebSocketAdapter] 메시지 파싱 오류: {e.Message}");
                    OnSignalingMessageReceived?.Invoke("error", message);
                }
            };
        }

        /// <summary>
        /// 리소스 정리를 위한 메서드입니다.
        /// </summary>
        public void Dispose()
        {
            if (webSocket != null)
            {
                if (webSocket.State == NativeWS.WebSocketState.Open)
                {
                    // 비동기 종료를 동기적으로 처리 (권장되지 않지만 Dispose 패턴에 맞춤)
                    webSocket.Close();
                }
                
                webSocket = null;
            }
        }
    }
}