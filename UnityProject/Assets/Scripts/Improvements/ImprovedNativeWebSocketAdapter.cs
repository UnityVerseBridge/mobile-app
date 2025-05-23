using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityVerseBridge.Core.Signaling;
using NativeWS = NativeWebSocket;
using CoreWS = UnityVerseBridge.Core.Signaling;

namespace UnityVerseBridge.MobileApp.Signaling.Improved
{
    /// <summary>
    /// 개선된 NativeWebSocket 어댑터 - IWebSocketClient만 구현
    /// </summary>
    public class ImprovedNativeWebSocketAdapter : IWebSocketClient
    {
        private NativeWS.WebSocket webSocket;
        
        // IWebSocketClient 이벤트
        public event Action OnOpen;
        public event Action<byte[]> OnMessage;
        public event Action<string> OnError;
        public event Action<ushort> OnClose;

        public CoreWS.WebSocketState State
        {
            get
            {
                if (webSocket == null) return CoreWS.WebSocketState.Closed;
                
                return webSocket.State switch
                {
                    NativeWS.WebSocketState.Connecting => CoreWS.WebSocketState.Connecting,
                    NativeWS.WebSocketState.Open => CoreWS.WebSocketState.Open,
                    NativeWS.WebSocketState.Closing => CoreWS.WebSocketState.Closing,
                    _ => CoreWS.WebSocketState.Closed
                };
            }
        }

        public async Task Connect(string url)
        {
            try
            {
                if (webSocket?.State == NativeWS.WebSocketState.Open || 
                    webSocket?.State == NativeWS.WebSocketState.Connecting)
                {
                    Debug.LogWarning("[ImprovedNativeWebSocket] Already connected or connecting");
                    return;
                }

                webSocket = new NativeWS.WebSocket(url);
                SetupEventHandlers();
                
                await webSocket.Connect();
                Debug.Log("[ImprovedNativeWebSocket] Connected");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ImprovedNativeWebSocket] Connect error: {e.Message}");
                OnError?.Invoke(e.Message);
                throw;
            }
        }

        public async Task Close()
        {
            if (webSocket == null || webSocket.State == NativeWS.WebSocketState.Closed)
                return;

            try
            {
                await webSocket.Close();
                Debug.Log("[ImprovedNativeWebSocket] Closed");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ImprovedNativeWebSocket] Close error: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        public async Task Send(byte[] bytes)
        {
            if (webSocket?.State != NativeWS.WebSocketState.Open)
            {
                Debug.LogError("[ImprovedNativeWebSocket] Cannot send - not open");
                return;
            }

            try
            {
                await webSocket.Send(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ImprovedNativeWebSocket] Send error: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        public async Task SendText(string message)
        {
            if (webSocket?.State != NativeWS.WebSocketState.Open)
            {
                Debug.LogError("[ImprovedNativeWebSocket] Cannot send - not open");
                return;
            }

            try
            {
                await webSocket.SendText(message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ImprovedNativeWebSocket] SendText error: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        public void DispatchMessageQueue()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            webSocket?.DispatchMessageQueue();
#endif
        }

        private void SetupEventHandlers()
        {
            if (webSocket == null) return;

            webSocket.OnOpen += () => OnOpen?.Invoke();
            webSocket.OnError += (e) => OnError?.Invoke(e);
            webSocket.OnClose += (e) => OnClose?.Invoke((ushort)e);
            webSocket.OnMessage += (data) => OnMessage?.Invoke(data);
        }
    }
}
