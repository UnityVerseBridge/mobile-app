using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC; // Unity.WebRTC 패키지 참조 추가
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Signaling;
using UnityVerseBridge.Core.Signaling.Adapters;
using System.Threading.Tasks;
using TMPro;
using System.Collections;

namespace UnityVerseBridge.MobileApp.Test
{
    /// <summary>
    /// 모바일 앱에서 WebRTC 연결을 테스트하기 위한 클래스입니다.
    /// </summary>
    public class WebRtcConnectionTester : MonoBehaviour
    {
        [Header("필수 컴포넌트")]
        [SerializeField] private WebRtcManager webRtcManager;
        [SerializeField] private MobileInputSender inputSender;
        [SerializeField] private MobileHapticReceiver hapticReceiver;

        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Button sendTestTouchButton;
        [SerializeField] private InputField serverUrlInput;
        [SerializeField] private RawImage videoDisplay; // 스트림 표시용

        [Header("테스트 설정")]
        [SerializeField] private string defaultServerUrl = "ws://localhost:8080";
        [SerializeField] private bool autoConnectOnStart = true; // 자동 연결 옵션 추가

        private VideoStreamTrack videoStreamTrack;
        private Texture receivedTexture;
        private ISignalingClient signalingClient;
        private SystemWebSocketAdapter webSocketAdapter;
        private bool isConnected = false;

        void Start()
        {
            // 기본값 설정
            if (serverUrlInput != null)
                serverUrlInput.text = defaultServerUrl;

            // UI 이벤트 연결 (UI 요소가 있는 경우에만)
            if (connectButton != null)
                connectButton.onClick.AddListener(Connect);
            
            if (disconnectButton != null)
                disconnectButton.onClick.AddListener(Disconnect);
            
            if (sendTestTouchButton != null)
                sendTestTouchButton.onClick.AddListener(SendTestTouch);

            // 필수 컴포넌트 확인
            CheckComponents();

            // 이벤트 구독
            if (webRtcManager != null)
            {
                webRtcManager.OnWebRtcConnected += HandleWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected += HandleWebRtcDisconnected;
                webRtcManager.OnDataChannelOpened += HandleDataChannelOpened;
                webRtcManager.OnDataChannelClosed += HandleDataChannelClosed;
                webRtcManager.OnDataChannelMessageReceived += HandleDataChannelMessage;
                webRtcManager.OnTrackReceived += HandleTrackReceived; // 비디오 트랙 이벤트 구독 복원
            }

            UpdateUI();
            
            // 자동 연결 옵션이 켜져 있으면 시작 시 자동으로 연결
            if (autoConnectOnStart && webRtcManager != null)
            {
                Debug.Log("[WebRtcConnectionTester] 자동 연결 시작...");
                Connect();
            }
        }

        void OnDestroy()
        {
            // 이벤트 구독 해제
            if (webRtcManager != null)
            {
                webRtcManager.OnWebRtcConnected -= HandleWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected -= HandleWebRtcDisconnected;
                webRtcManager.OnDataChannelOpened -= HandleDataChannelOpened;
                webRtcManager.OnDataChannelClosed -= HandleDataChannelClosed;
                webRtcManager.OnDataChannelMessageReceived -= HandleDataChannelMessage;
                webRtcManager.OnTrackReceived -= HandleTrackReceived; // 비디오 트랙 이벤트 구독 해제 복원
            }

            // 리소스 정리
            CleanupVideoDisplay();
        }

        private void CleanupVideoDisplay()
        {
            // 비디오 디스플레이 초기화 (UI가 없어도 작동하도록)
            if (videoDisplay != null)
                videoDisplay.texture = null;
            
            if (videoStreamTrack != null)
            {
                videoStreamTrack.Dispose();
                videoStreamTrack = null;
            }
            
            receivedTexture = null;
        }

        private void CheckComponents()
        {
            if (webRtcManager == null)
            {
                LogStatus("오류: WebRtcManager가 할당되지 않았습니다!");
                return;
            }

            if (inputSender == null)
                LogStatus("경고: MobileInputSender가 할당되지 않았습니다. 터치 입력을 보낼 수 없습니다.");

            if (hapticReceiver == null)
                LogStatus("경고: MobileHapticReceiver가 할당되지 않았습니다. 햅틱 피드백을 받을 수 없습니다.");

            if (videoDisplay == null)
                LogStatus("경고: RawImage가 할당되지 않았습니다. 비디오 스트림을 표시할 수 없습니다.");
        }

        public async void Connect()
        {
            if (isConnected || webRtcManager == null)
            {
                LogStatus("이미 연결되어 있거나 WebRtcManager가 없습니다.");
                return;
            }

            // 서버 URL 가져오기 (UI가 없으면 기본값 사용)
            string serverUrl = (serverUrlInput != null) ? serverUrlInput.text : defaultServerUrl;
            if (string.IsNullOrEmpty(serverUrl))
                serverUrl = defaultServerUrl;
                
            LogStatus($"시그널링 서버에 연결 시도 중: {serverUrl}");

            // WebSocket 어댑터 및 시그널링 클라이언트 생성
            webSocketAdapter = new SystemWebSocketAdapter();
            signalingClient = new SignalingClient();

            try
            {
                // 시그널링 클라이언트 초기화 및 연결
                await signalingClient.InitializeAndConnect(webSocketAdapter as IWebSocketClient, serverUrl);
                
                // 시그널링 클라이언트가 성공적으로 연결됐다면
                if (signalingClient.IsConnected)
                {
                    LogStatus("시그널링 서버에 연결되었습니다. WebRTC 연결을 대기합니다...");
                    webRtcManager.SetupSignaling(signalingClient);
                }
                else
                {
                    LogStatus("시그널링 서버 연결 실패");
                }
            }
            catch (System.Exception e)
            {
                LogStatus($"연결 오류: {e.Message}");
            }

            UpdateUI();
        }

        public void Disconnect()
        {
            if (!isConnected || webRtcManager == null)
            {
                LogStatus("연결되어 있지 않거나 WebRtcManager가 없습니다.");
                return;
            }

            try
            {
                // CloseConnection 대신 Disconnect 메서드 호출
                webRtcManager.Disconnect();
                LogStatus("연결 종료 요청됨");
            }
            catch (System.Exception e)
            {
                LogStatus($"연결 종료 오류: {e.Message}");
            }

            UpdateUI();
        }

        public void SendTestTouch()
        {
            if (!isConnected || webRtcManager == null || !webRtcManager.IsDataChannelOpen)
            {
                LogStatus("데이터 채널이 열려있지 않아 터치 데이터를 보낼 수 없습니다.");
                return;
            }

            try
            {
                // 테스트용 터치 데이터 생성 - 생성자 수정
                var touchData = new UnityVerseBridge.Core.DataChannel.Data.TouchData(
                    id: 1, 
                    touchPhase: UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Began,
                    normalizedPosition: new Vector2(0.5f, 0.5f) // 화면 중앙
                );
                
                webRtcManager.SendDataChannelMessage(touchData);
                LogStatus($"테스트 터치 전송됨: ID={touchData.touchId}, 위치=({touchData.positionX:F2}, {touchData.positionY:F2})");
            }
            catch (System.Exception e)
            {
                LogStatus($"터치 전송 오류: {e.Message}");
            }
        }

        private void HandleWebRtcConnected()
        {
            isConnected = true;
            LogStatus("WebRTC 연결 성공!");
            UpdateUI();
        }

        private void HandleWebRtcDisconnected()
        {
            isConnected = false;
            LogStatus("WebRTC 연결 종료");
            
            // 비디오 디스플레이 정리
            CleanupVideoDisplay();
            
            UpdateUI();
        }

        private void HandleDataChannelOpened(string channelId)
        {
            LogStatus($"데이터 채널이 열렸습니다. 채널 ID: {channelId}");
            UpdateUI();
        }

        private void HandleDataChannelClosed()
        {
            LogStatus("데이터 채널이 닫혔습니다.");
            UpdateUI();
        }

        private void HandleDataChannelMessage(string message)
        {
            LogStatus($"메시지 수신: {message}");
        }

        // 비디오 트랙 수신 처리 메서드
        private void HandleTrackReceived(MediaStreamTrack track)
        {
            if (track == null)
            {
                LogStatus("수신된 트랙이 null입니다.");
                return;
            }
            
            LogStatus($"트랙이 수신되었습니다: {track.Kind}");
            
            // 비디오 트랙인 경우에만 처리
            if (track.Kind == TrackKind.Video)
            {
                // VideoStreamTrack으로 변환
                videoStreamTrack = track as VideoStreamTrack;
                if (videoStreamTrack != null)
                {
                    LogStatus("비디오 트랙이 수신되었습니다.");
                    
                    // VideoStreamTrack에서 Texture 가져오기
                    receivedTexture = videoStreamTrack.Texture;
                    
                    // RawImage가 할당되어 있지 않아도 로깅은 계속
                    if (receivedTexture != null)
                    {
                        LogStatus($"비디오 텍스처 수신 완료. 해상도: {receivedTexture.width}x{receivedTexture.height}");
                        
                        // UI가 있으면 화면에 표시
                        if (videoDisplay != null)
                        {
                            videoDisplay.texture = receivedTexture;
                            LogStatus("비디오 디스플레이에 연결 완료.");
                        }
                        else
                        {
                            LogStatus("videoDisplay가 할당되지 않았습니다. 텍스처는 수신했지만 화면에 표시하지 않습니다.");
                        }
                    }
                    else
                    {
                        LogStatus("비디오 텍스처를 가져올 수 없습니다.");
                    }
                }
                else
                {
                    LogStatus("비디오 트랙을 VideoStreamTrack으로 변환할 수 없습니다.");
                }
            }
            else
            {
                LogStatus($"비디오가 아닌 트랙이 수신되었습니다: {track.Kind}");
            }
        }

        private void LogStatus(string message)
        {
            Debug.Log($"[WebRtcConnectionTester] {message}");
            
            // UI가 없어도 로그는 계속 기록됨
            if (statusText != null)
            {
                // 최대 5줄만 유지
                string[] lines = statusText.text.Split('\n');
                string newText = message;
                
                if (lines.Length >= 5)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        newText += "\n" + lines[i];
                    }
                }
                else
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        newText += "\n" + lines[i];
                    }
                }
                
                statusText.text = newText;
            }
        }

        private void UpdateUI()
        {
            // UI가 없어도 작동하도록 모든 요소에 null 체크
            if (connectButton != null)
                connectButton.interactable = !isConnected;
            
            if (disconnectButton != null)
                disconnectButton.interactable = isConnected;
            
            if (sendTestTouchButton != null)
                sendTestTouchButton.interactable = isConnected && webRtcManager != null && webRtcManager.IsDataChannelOpen;
            
            if (serverUrlInput != null)
                serverUrlInput.interactable = !isConnected;
        }
        
        void Update()
        {
            // WebSocket 메시지 큐 처리
            webSocketAdapter?.DispatchMessageQueue();
            signalingClient?.DispatchMessages();
        }
    }
} 