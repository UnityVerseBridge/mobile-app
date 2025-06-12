using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Extensions.Mobile;
using System.Collections;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Mobile 앱에서 비디오 스트리밍 문제를 디버깅하기 위한 헬퍼 컴포넌트
    /// </summary>
    public class MobileVideoDebugger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        [SerializeField] private MobileVideoExtension videoExtension;
        [SerializeField] private RawImage debugDisplayImage;
        [SerializeField] private Text debugText;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDetailedLogging = true;
        [SerializeField] private bool testDirectTexture = false;
        [SerializeField] private bool testRenderTexture = true;
        
        private WebRtcManager webRtcManager;
        private VideoStreamTrack receivedVideoTrack;
        private RenderTexture debugRenderTexture;
        
        void Start()
        {
            // Find components if not assigned
            if (bridgeManager == null)
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
                
            if (videoExtension == null)
                videoExtension = FindFirstObjectByType<MobileVideoExtension>();
                
            if (bridgeManager != null)
            {
                StartCoroutine(WaitForInitialization());
            }
            else
            {
                UpdateDebugText("ERROR: UnityVerseBridgeManager not found!");
            }
        }
        
        private IEnumerator WaitForInitialization()
        {
            // Wait for bridge manager initialization
            while (!bridgeManager.IsInitialized)
            {
                UpdateDebugText("Waiting for BridgeManager initialization...");
                yield return null;
            }
            
            webRtcManager = bridgeManager.WebRtcManager;
            if (webRtcManager != null)
            {
                // Subscribe to video track events
                webRtcManager.OnVideoTrackReceived += OnVideoTrackReceived;
                webRtcManager.OnWebRtcConnected += OnWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected += OnWebRtcDisconnected;
                
                UpdateDebugText("Debugger initialized. Waiting for video track...");
            }
            else
            {
                UpdateDebugText("ERROR: WebRtcManager not found!");
            }
        }
        
        private void OnWebRtcConnected()
        {
            UpdateDebugText("WebRTC Connected! Waiting for video track...");
        }
        
        private void OnWebRtcDisconnected()
        {
            UpdateDebugText("WebRTC Disconnected!");
            receivedVideoTrack = null;
        }
        
        private void OnVideoTrackReceived(MediaStreamTrack track)
        {
            var videoTrack = track as VideoStreamTrack;
            if (videoTrack == null)
            {
                UpdateDebugText("ERROR: Received track is not a video track!");
                return;
            }
            
            UpdateDebugText($"Video track received: {videoTrack.Id}");
            receivedVideoTrack = videoTrack;
            
            // Start testing different display methods
            StartCoroutine(TestVideoDisplay());
        }
        
        private IEnumerator TestVideoDisplay()
        {
            UpdateDebugText("Starting video display test...");
            
            // Wait for decoder
            yield return new WaitForSeconds(1.5f);
            
            if (receivedVideoTrack == null || receivedVideoTrack.ReadyState != TrackState.Live)
            {
                UpdateDebugText($"ERROR: Track not ready! State: {receivedVideoTrack?.ReadyState}");
                yield break;
            }
            
            // Test 1: Direct texture assignment
            if (testDirectTexture && debugDisplayImage != null)
            {
                UpdateDebugText("Test 1: Direct texture assignment");
                
                // Poll for texture
                float waitTime = 0f;
                while (receivedVideoTrack.Texture == null && waitTime < 5f)
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }
                
                if (receivedVideoTrack.Texture != null)
                {
                    debugDisplayImage.texture = receivedVideoTrack.Texture;
                    UpdateDebugText($"Direct assignment SUCCESS: {receivedVideoTrack.Texture.width}x{receivedVideoTrack.Texture.height}");
                }
                else
                {
                    UpdateDebugText("Direct assignment FAILED: Texture is null");
                }
                
                yield return new WaitForSeconds(2f);
            }
            
            // Test 2: RenderTexture with Graphics.Blit
            if (testRenderTexture && debugDisplayImage != null)
            {
                UpdateDebugText("Test 2: RenderTexture with Graphics.Blit");
                
                // Create debug render texture
                if (debugRenderTexture == null)
                {
                    debugRenderTexture = new RenderTexture(1280, 720, 24, RenderTextureFormat.BGRA32);
                    debugRenderTexture.name = "DebugRenderTexture";
                    debugRenderTexture.Create();
                }
                
                // Start update loop
                StartCoroutine(UpdateVideoWithRenderTexture());
            }
        }
        
        private IEnumerator UpdateVideoWithRenderTexture()
        {
            while (receivedVideoTrack != null && receivedVideoTrack.ReadyState == TrackState.Live)
            {
                if (receivedVideoTrack.Texture != null)
                {
                    try
                    {
                        // Test different blit methods
                        Graphics.Blit(receivedVideoTrack.Texture, debugRenderTexture);
                        debugDisplayImage.texture = debugRenderTexture;
                        
                        // Log success periodically
                        if (Time.frameCount % 60 == 0)
                        {
                            var tex = receivedVideoTrack.Texture;
                            UpdateDebugText($"Blit SUCCESS: {tex.width}x{tex.height}, Format: {tex.graphicsFormat}");
                        }
                    }
                    catch (System.Exception e)
                    {
                        UpdateDebugText($"Blit ERROR: {e.Message}");
                    }
                }
                else if (Time.frameCount % 60 == 0)
                {
                    UpdateDebugText("Texture is null in update loop");
                }
                
                yield return new WaitForEndOfFrame();
            }
            
            UpdateDebugText("Update loop ended");
        }
        
        private void UpdateDebugText(string message)
        {
            if (enableDetailedLogging)
            {
                Debug.Log($"[MobileVideoDebugger] {message}");
            }
            
            if (debugText != null)
            {
                debugText.text = $"[{Time.time:F1}] {message}";
            }
        }
        
        void OnDestroy()
        {
            if (webRtcManager != null)
            {
                webRtcManager.OnVideoTrackReceived -= OnVideoTrackReceived;
                webRtcManager.OnWebRtcConnected -= OnWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected -= OnWebRtcDisconnected;
            }
            
            if (debugRenderTexture != null)
            {
                debugRenderTexture.Release();
                Destroy(debugRenderTexture);
            }
        }
    }
}