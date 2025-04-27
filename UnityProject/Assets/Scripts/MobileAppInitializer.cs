using UnityEngine;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Signaling; // IWebSocketClient, ISignalingClient, SignalingClient 필요
using UnityVerseBridge.MobileApp.Signaling; // NativeWebSocketAdapter 필요 (네임스페이스 확인!)

namespace UnityVerseBridge.MobileApp
{
    public class MobileAppInitializer : MonoBehaviour
    {
        [Header("필수 참조")]
        [SerializeField] private WebRtcManager webRtcManager;

        [Header("선택 설정")]
        [SerializeField] private WebRtcConfiguration webRtcConfiguration;
        // WebRtcManager에서 signalingServerUrl을 public get으로 접근 가능하게 하거나 여기서 관리
        // [SerializeField] private string signalingServerUrl = "ws://localhost:8080"; // 또는 WebRtcManager에서 가져옴

        void Start()
        {
            if (webRtcManager == null) webRtcManager = FindObjectOfType<WebRtcManager>();
            if (webRtcManager == null)
            {
                Debug.LogError("[MobileAppInitializer] WebRtcManager not found!");
                enabled = false;
                return;
            }

            // 1. 플랫폼에 맞는 WebSocket 어댑터 생성
            IWebSocketClient webSocketAdapter = new NativeWebSocketAdapter(); // 이 네임스페이스 확인!
            Debug.Log("[MobileAppInitializer] NativeWebSocketAdapter created.");

            // 2. SignalingClient 생성 (ISignalingClient 타입으로 참조)
            ISignalingClient signalingClientImpl = new SignalingClient();
            Debug.Log("[MobileAppInitializer] SignalingClient created.");

            // 3. SignalingClient 초기화 (어댑터 주입 및 서버 URL 전달)
            //    WebRtcManager에서 URL 가져오기 (public 속성 SignalingServerUrl 사용)
            string serverUrl = webRtcManager.SignalingServerUrl;
            _ = signalingClientImpl.InitializeAndConnect(webSocketAdapter, serverUrl); // InitializeAndConnect 호출!
            Debug.Log("[MobileAppInitializer] SignalingClient initialization requested.");

            // 4. WebRtcManager에 초기화된 SignalingClient 주입/설정
            webRtcManager.SetupSignaling(signalingClientImpl); // SetupSignaling 호출!

            // 5. (선택 사항) WebRtc Configuration 설정
            if (webRtcConfiguration != null) 
            webRtcManager.SetConfiguration(webRtcConfiguration);

            Debug.Log("[MobileAppInitializer] Initialization complete.");
        }
    }
}