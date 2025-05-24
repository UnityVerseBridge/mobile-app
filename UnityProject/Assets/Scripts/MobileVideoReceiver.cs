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
                webRtcManager = FindObjectOfType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileVideoReceiver] WebRtcManager not found!");
                    enabled = false;
                    return;
                }
            }

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
                
                displayImage.texture = texture;
                
                // Adjust aspect ratio to prevent stretching
                var aspectRatio = (float)texture.width / texture.height;
                var rt = displayImage.rectTransform;
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.x / aspectRatio);
            }
        }
        
        private IEnumerator UpdateVideoTexture()
        {
            Debug.Log("[MobileVideoReceiver] Starting video texture update coroutine...");
            
            // Fallback polling method for edge cases
            yield return new WaitForSeconds(0.5f); // Initial delay
            
            // Check for texture availability
            float waitTime = 0f;
            while (receivedVideoTrack != null && waitTime < 5f)
            {
                if (receivedVideoTrack.Texture != null)
                {
                    Debug.Log($"[MobileVideoReceiver] Video texture ready via polling! Size: {receivedVideoTrack.Texture.width}x{receivedVideoTrack.Texture.height}");
                    break;
                }
                
                waitTime += Time.deltaTime;
                yield return null;
            }
            
            if (receivedVideoTrack == null || receivedVideoTrack.Texture == null)
            {
                Debug.LogError("[MobileVideoReceiver] Failed to get video texture after waiting");
                yield break;
            }
            
            // 텍스처 렌더링 루프
            while (isReceiving && receivedVideoTrack != null && receivedVideoTrack.ReadyState == TrackState.Live)
            {
                if (receivedVideoTrack.Texture != null)
                {
                    // RenderTexture에 복사하는 방식으로 시도
                    Graphics.Blit(receivedVideoTrack.Texture, receiveTexture);
                    displayImage.texture = receiveTexture;
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            Debug.Log("[MobileVideoReceiver] Video texture update coroutine ended");
        }
    }
}
