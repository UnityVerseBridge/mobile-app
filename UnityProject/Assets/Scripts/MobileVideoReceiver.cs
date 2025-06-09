using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;
using UnityVerseBridge.Core;
using System.Collections;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Quest 앱으로부터 비디오 스트림을 수신하여 화면에 표시
    /// </summary>
    public class MobileVideoReceiver : MonoBehaviour
    {
        [SerializeField] private WebRtcManager webRtcManager;
        [SerializeField] private RawImage displayImage; // 비디오를 표시할 UI RawImage
        [SerializeField] private RenderTexture receiveTexture; // Inspector에서 할당 가능
        
        private VideoStreamTrack receivedVideoTrack;
        private bool isReceiving = false;
        private Coroutine updateCoroutine;

        void Start()
        {
            if (webRtcManager == null)
            {
                webRtcManager = FindFirstObjectByType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileVideoReceiver] WebRtcManager not found!");
                    enabled = false;
                    return;
                }
            }
            
            Debug.Log($"[MobileVideoReceiver] WebRTC connection state at start: {webRtcManager.IsWebRtcConnected}");

            if (displayImage == null)
            {
                Debug.LogError("[MobileVideoReceiver] Display RawImage not assigned!");
                enabled = false;
                return;
            }

            // RenderTexture 생성 또는 확인
            if (receiveTexture == null)
            {
                Debug.Log("[MobileVideoReceiver] Creating RenderTexture for receiving video...");
                // Quest와 동일한 포맷으로 생성
                receiveTexture = new RenderTexture(1280, 720, 24, RenderTextureFormat.BGRA32, RenderTextureReadWrite.sRGB);
                receiveTexture.name = "MobileReceiveTexture";
                receiveTexture.Create();
            }
            else
            {
                Debug.Log($"[MobileVideoReceiver] Using existing RenderTexture: {receiveTexture.name}");
                if (!receiveTexture.IsCreated())
                {
                    receiveTexture.Create();
                }
            }

            // 비디오 트랙 수신 이벤트 구독
            webRtcManager.OnVideoTrackReceived += HandleVideoTrackReceived;
            Debug.Log("[MobileVideoReceiver] Subscribed to OnVideoTrackReceived event");
        }

        void OnDestroy()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
            }

            if (webRtcManager != null)
            {
                webRtcManager.OnVideoTrackReceived -= HandleVideoTrackReceived;
            }

            if (receivedVideoTrack != null)
            {
                receivedVideoTrack.OnVideoReceived -= OnVideoFrameReceived;
                receivedVideoTrack.Dispose();
                receivedVideoTrack = null;
            }

            if (receiveTexture != null && !Application.isEditor)
            {
                receiveTexture.Release();
                Destroy(receiveTexture);
            }
        }

        private void HandleVideoTrackReceived(VideoStreamTrack videoTrack)
        {
            Debug.Log($"[MobileVideoReceiver] Video track received: {videoTrack.Id}");
            Debug.Log($"[MobileVideoReceiver] Track enabled: {videoTrack.Enabled}, ReadyState: {videoTrack.ReadyState}");
            
            receivedVideoTrack = videoTrack;
            
            // 트랙이 활성화되어 있는지 확인
            if (!receivedVideoTrack.Enabled)
            {
                receivedVideoTrack.Enabled = true;
                Debug.Log("[MobileVideoReceiver] Enabled video track");
            }
            
            // Check decoder initialization status
            StartCoroutine(WaitForDecoder());
        }
        
        private IEnumerator WaitForDecoder()
        {
            Debug.Log("[MobileVideoReceiver] Waiting for decoder to be ready...");
            
            // Wait a bit for decoder to initialize internally
            yield return new WaitForSeconds(0.5f);
            
            // Check if track is ready by checking ReadyState
            if (receivedVideoTrack != null && receivedVideoTrack.ReadyState == TrackState.Live)
            {
                Debug.Log("[MobileVideoReceiver] Track is live and ready");
                
                // Primary method: Use OnVideoReceived event (recommended)
                receivedVideoTrack.OnVideoReceived += OnVideoFrameReceived;
                
                isReceiving = true;
                
                // Fallback method: polling (for edge cases)
                if (updateCoroutine != null)
                {
                    StopCoroutine(updateCoroutine);
                }
                updateCoroutine = StartCoroutine(UpdateVideoTexture());
            }
            else
            {
                Debug.LogError($"[MobileVideoReceiver] Track not ready after wait. State: {receivedVideoTrack?.ReadyState}");
            }
        }

        private void OnVideoFrameReceived(Texture texture)
        {
            // Texture is guaranteed to be ready here
            if (texture != null)
            {
                Debug.Log($"[MobileVideoReceiver] Video received via OnVideoReceived: {texture.width}x{texture.height}");
                
                // Stop polling if it's running
                if (updateCoroutine != null)
                {
                    StopCoroutine(updateCoroutine);
                    updateCoroutine = null;
                }
                
                // Platform-specific handling
                #if UNITY_ANDROID
                // Android may need texture alignment
                if (texture.width % 16 != 0 || texture.height % 16 != 0)
                {
                    Debug.LogWarning("[MobileVideoReceiver] Android texture alignment issue detected");
                }
                #endif
                
                displayImage.texture = texture;
                
                // Adjust aspect ratio to prevent stretching
                var aspectRatio = (float)texture.width / texture.height;
                var rt = displayImage.rectTransform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.x / aspectRatio);
            }
            else
            {
                Debug.LogWarning("[MobileVideoReceiver] OnVideoFrameReceived called with null texture");
            }
        }
        
        /// <summary>
        /// 폴링 방식으로 비디오 텍스처를 확인하는 폴백 메커니즘입니다.
        /// OnVideoReceived 이벤트가 발생하지 않는 경우를 대비한 보호 로직입니다.
        /// 일반적으로 OnVideoReceived가 잘 작동하면 이 코루틴은 자동 종료됩니다.
        /// </summary>
        private IEnumerator UpdateVideoTexture()
        {
            Debug.Log("[MobileVideoReceiver] Starting video texture update coroutine...");
            
            // 초기 대기: 디코더 초기화를 위한 시간
            yield return new WaitForSeconds(0.5f);
            
            // 최대 5초 동안 텍스처 생성을 기다림
            // Unity WebRTC는 디코더 초기화와 첫 프레임 수신에 시간이 걸릴 수 있음
            float waitTime = 0f;
            const float maxWaitTime = 5f;
            
            while (receivedVideoTrack != null && waitTime < maxWaitTime)
            {
                if (receivedVideoTrack.Texture != null)
                {
                    Debug.Log($"[MobileVideoReceiver] Video texture ready via polling! Size: {receivedVideoTrack.Texture.width}x{receivedVideoTrack.Texture.height}");
                    break;
                }
                
                waitTime += Time.deltaTime;
                yield return null; // 다음 프레임까지 대기
            }
            
            if (receivedVideoTrack == null || receivedVideoTrack.Texture == null)
            {
                Debug.LogError("[MobileVideoReceiver] Failed to get video texture after waiting");
                yield break;
            }
            
            // 폴링 기반 텍스처 업데이트 루프
            // 일반적으로 OnVideoReceived가 호출되면 이 루프는 사용되지 않음
            while (isReceiving && receivedVideoTrack != null && receivedVideoTrack.ReadyState == TrackState.Live)
            {
                if (receivedVideoTrack.Texture != null)
                {
                    // Graphics.Blit: GPU에서 텍스처를 효율적으로 복사
                    // WebRTC 텍스처는 직접 사용할 수 없는 경우가 있어 RenderTexture로 복사
                    Graphics.Blit(receivedVideoTrack.Texture, receiveTexture);
                    displayImage.texture = receiveTexture;
                }
                
                yield return new WaitForEndOfFrame(); // 프레임 렌더링 종료 후 실행
            }
            
            Debug.Log("[MobileVideoReceiver] Video texture update coroutine ended");
        }
    }
}
