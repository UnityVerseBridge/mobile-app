using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.Extensions.Mobile;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace UnityVerseBridge.MobileApp.Setup
{
    /// <summary>
    /// Debug script to verify touch input is working in the mobile app
    /// </summary>
    public class TouchInputDebugger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool showTouchPositions = true;
        [SerializeField] private bool showConnectionStatus = true;
        
        private MobileInputExtension inputExtension;
        private WebRtcManager webRtcManager;
        private float lastStatusCheck = 0f;
        
        void Start()
        {
            // Enable Enhanced Touch Support
            EnhancedTouchSupport.Enable();
            Debug.Log("[TouchInputDebugger] Enhanced Touch Support enabled");
            
            // Find UnityVerseBridgeManager if not assigned
            if (bridgeManager == null)
            {
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
                if (bridgeManager == null)
                {
                    Debug.LogError("[TouchInputDebugger] UnityVerseBridgeManager not found!");
                    enabled = false;
                    return;
                }
            }
            
            // Get WebRtcManager
            webRtcManager = bridgeManager.WebRtcManager;
            
            Debug.Log($"[TouchInputDebugger] Started - Bridge Mode: {bridgeManager.Mode}");
            Debug.Log($"[TouchInputDebugger] Touch Area: {(bridgeManager.MobileTouchArea != null ? bridgeManager.MobileTouchArea.name : "NULL (full screen)")}");
        }
        
        void Update()
        {
            // Check for input extension periodically
            if (inputExtension == null && Time.time - lastStatusCheck > 1f)
            {
                inputExtension = bridgeManager.GetComponent<MobileInputExtension>();
                if (inputExtension != null)
                {
                    Debug.Log($"[TouchInputDebugger] MobileInputExtension found - Enabled: {inputExtension.enabled}");
                }
                lastStatusCheck = Time.time;
            }
            
            // Log connection status
            if (showConnectionStatus && Time.time - lastStatusCheck > 5f)
            {
                LogConnectionStatus();
                lastStatusCheck = Time.time;
            }
            
            // Debug touch positions
            if (showTouchPositions)
            {
                DebugTouchPositions();
            }
        }
        
        private void DebugTouchPositions()
        {
            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count > 0 && enableDebugLogs)
            {
                foreach (var touch in activeTouches)
                {
                    Vector2 normalizedPos = new Vector2(
                        touch.screenPosition.x / Screen.width,
                        touch.screenPosition.y / Screen.height
                    );
                    
                    Debug.Log($"[TouchInputDebugger] Touch {touch.touchId}: " +
                             $"Screen({touch.screenPosition.x:F0}, {touch.screenPosition.y:F0}) " +
                             $"Normalized({normalizedPos.x:F3}, {normalizedPos.y:F3}) " +
                             $"Phase: {touch.phase}");
                }
            }
        }
        
        private void LogConnectionStatus()
        {
            if (!enableDebugLogs) return;
            
            Debug.Log("[TouchInputDebugger] === Connection Status ===");
            Debug.Log($"  Bridge Initialized: {bridgeManager.IsInitialized}");
            Debug.Log($"  Bridge Connected: {bridgeManager.IsConnected}");
            
            if (webRtcManager != null)
            {
                Debug.Log($"  Signaling Connected: {webRtcManager.IsSignalingConnected}");
                Debug.Log($"  WebRTC Connected: {webRtcManager.IsWebRtcConnected}");
                Debug.Log($"  Data Channel Open: {webRtcManager.IsDataChannelOpen}");
                Debug.Log($"  Peer Connection State: {webRtcManager.GetPeerConnectionState()}");
            }
            
            if (inputExtension != null)
            {
                Debug.Log($"  MobileInputExtension Enabled: {inputExtension.enabled}");
            }
            else
            {
                Debug.Log("  MobileInputExtension: NOT FOUND");
            }
            
            Debug.Log("========================");
        }
        
        void OnGUI()
        {
            if (!showConnectionStatus) return;
            
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            
            float y = 10;
            GUI.Label(new Rect(10, y, 400, 20), $"Touch Debug Info", style);
            y += 25;
            
            // Connection status
            GUI.Label(new Rect(10, y, 400, 20), $"Connected: {(bridgeManager.IsConnected ? "YES" : "NO")}", style);
            y += 20;
            
            if (webRtcManager != null)
            {
                GUI.Label(new Rect(10, y, 400, 20), $"WebRTC: {(webRtcManager.IsWebRtcConnected ? "Connected" : "Disconnected")}", style);
                y += 20;
                GUI.Label(new Rect(10, y, 400, 20), $"Data Channel: {(webRtcManager.IsDataChannelOpen ? "Open" : "Closed")}", style);
                y += 20;
            }
            
            // Touch info
            var touches = Touch.activeTouches;
            GUI.Label(new Rect(10, y, 400, 20), $"Active Touches: {touches.Count}", style);
            y += 20;
            
            // Input extension status
            GUI.Label(new Rect(10, y, 400, 20), $"Input Extension: {(inputExtension != null ? "Found" : "Not Found")}", style);
            y += 20;
            
            // Touch area info
            string touchAreaInfo = bridgeManager.MobileTouchArea != null ? 
                bridgeManager.MobileTouchArea.name : "Full Screen";
            GUI.Label(new Rect(10, y, 400, 20), $"Touch Area: {touchAreaInfo}", style);
        }
        
        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }
    }
}