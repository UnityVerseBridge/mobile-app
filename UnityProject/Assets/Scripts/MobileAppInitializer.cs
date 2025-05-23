using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Signaling; // IWebSocketClient, ISignalingClient, SignalingClient 필요
using UnityVerseBridge.Core.Signaling.Data; // SignalingMessageBase 필요
using UnityVerseBridge.MobileApp.Signaling; // NativeWebSocketAdapter 필요 (네임스페이스 확인!)

namespace UnityVerseBridge.MobileApp
{
    [System.Serializable]
    public class RegisterMessage : SignalingMessageBase
    {
        public string peerId;
        public string clientType;
        public string roomId;
        
        public RegisterMessage()
        {
            type = "register";
        }
    }
    
    public class MobileAppInitializer : MonoBehaviour
    {
        private string clientId;
        private string roomId = "room_123"; // 테스트용 룸 ID
        private SystemWebSocketAdapter webSocketAdapter;
        private SignalingClient signalingClient;
        
        [Header("필수 참조")]
        [SerializeField] private WebRtcManager webRtcManager;

        [Header("선택 설정")]
        [SerializeField] private WebRtcConfiguration webRtcConfiguration;
        // WebRtcManager에서 signalingServerUrl을 public get으로 접근 가능하게 하거나 여기서 관리
        // [SerializeField] private string signalingServerUrl = "ws://localhost:8080"; // 또는 WebRtcManager에서 가져옴

        void Start()
        {
            // 클라이언트 ID 생성
            clientId = "mobile_" + SystemInfo.deviceUniqueIdentifier;
            
            if (webRtcManager == null) webRtcManager = FindObjectOfType<WebRtcManager>();
            if (webRtcManager == null)
            {
                Debug.LogError("[MobileAppInitializer] WebRtcManager not found!");
                enabled = false;
                return;
            }

            // 1. SystemWebSocketAdapter 생성 (Quest app과 동일)
            webSocketAdapter = new SystemWebSocketAdapter();
            Debug.Log("[MobileAppInitializer] SystemWebSocketAdapter created.");
            
            // 2. SignalingClient 생성
            signalingClient = new SignalingClient();
            Debug.Log("[MobileAppInitializer] SignalingClient created.");

            // 3. Mobile App은 Answerer로 설정
            webRtcManager.SetRole(false); // false = Answerer
            
            // 4. WebRtcManager에 SignalingClient 설정
            webRtcManager.SetupSignaling(signalingClient);
            Debug.Log("[MobileAppInitializer] WebRtcManager에 SignalingClient 설정 완료.");
            
            // 5. 시그널링 연결
            string serverUrl = webRtcManager.SignalingServerUrl;
            StartSignalingConnection(serverUrl);

            // 5. (선택 사항) WebRtc Configuration 설정
            if (webRtcConfiguration != null) 
            webRtcManager.SetConfiguration(webRtcConfiguration);

            Debug.Log("[MobileAppInitializer] Initialization complete.");
        }
        
        private async void StartSignalingConnection(string serverUrl)
        {
            try
            {
                // SignalingClient에 WebSocket 어댑터를 연결하고 서버에 연결
                await signalingClient.InitializeAndConnect(webSocketAdapter, serverUrl);
                Debug.Log("[MobileAppInitializer] SignalingClient 연결 성공");
                
                // WebSocket이 열려있는지 확인
                await Task.Delay(100); // 연결 안정화 대기
                
                // 클라이언트 등록
                await RegisterClient();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAppInitializer] Connection failed: {ex.Message}");
                // 재시도
                Debug.Log("[MobileAppInitializer] 5초 후 재연결 시도...");
                await Task.Delay(5000);
                StartSignalingConnection(serverUrl);
            }
        }
        
        private async Task RegisterClient()
        {
            try
            {
                var registerMessage = new RegisterMessage
                {
                    peerId = clientId,
                    clientType = "mobile",
                    roomId = roomId
                };
                
                string jsonMessage = JsonUtility.ToJson(registerMessage);
                Debug.Log($"[MobileAppInitializer] Registering client: {jsonMessage}");
                
                // JSON 문자열로 직접 전송 (Quest app과 동일)
                if (webSocketAdapter != null)
                {
                    await webSocketAdapter.SendText(jsonMessage);
                    Debug.Log("[MobileAppInitializer] Client registered successfully");
                }
                else
                {
                    Debug.LogError("[MobileAppInitializer] WebSocketAdapter is null");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MobileAppInitializer] Failed to register client: {ex.Message}");
            }
        }
        
        void Update()
        {
            // SystemWebSocket의 메시지 큐 처리
            webSocketAdapter?.DispatchMessageQueue();
            
            // SignalingClient의 메시지 처리
            signalingClient?.DispatchMessages();
        }
    }
}