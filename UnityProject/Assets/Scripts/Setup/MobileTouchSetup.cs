using UnityEngine;
using UnityEngine.UI;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp.Setup
{
    /// <summary>
    /// Helper script to ensure touch input is properly configured in the mobile app
    /// </summary>
    public class MobileTouchSetup : MonoBehaviour
    {
        [Header("Touch Area Setup")]
        [SerializeField] private bool createTouchArea = true;
        [SerializeField] private string touchAreaName = "TouchArea";
        
        [Header("References")]
        [SerializeField] private UnityVerseBridgeManager bridgeManager;
        [SerializeField] private Canvas mainCanvas;
        
        void Awake()
        {
            // Find UnityVerseBridgeManager if not assigned
            if (bridgeManager == null)
            {
                bridgeManager = FindFirstObjectByType<UnityVerseBridgeManager>();
                if (bridgeManager == null)
                {
                    Debug.LogError("[MobileTouchSetup] UnityVerseBridgeManager not found!");
                    return;
                }
            }
            
            // Find main canvas if not assigned
            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("[MobileTouchSetup] No Canvas found in scene!");
                    return;
                }
            }
            
            SetupTouchArea();
        }
        
        private void SetupTouchArea()
        {
            // Check if touch area is already assigned
            if (bridgeManager.MobileTouchArea != null)
            {
                Debug.Log("[MobileTouchSetup] Touch area already assigned");
                return;
            }
            
            // Try to find existing touch area
            Transform existingTouchArea = mainCanvas.transform.Find(touchAreaName);
            if (existingTouchArea != null)
            {
                RectTransform touchAreaRect = existingTouchArea.GetComponent<RectTransform>();
                if (touchAreaRect != null)
                {
                    // Note: MobileTouchArea is read-only, so we can't assign it directly
                    // The touch area needs to be assigned in the Unity Editor
                    Debug.LogWarning("[MobileTouchSetup] Found touch area but cannot assign it (read-only property). Please assign it manually in the Unity Editor.");
                    return;
                }
            }
            
            // Create touch area if requested
            if (createTouchArea)
            {
                CreateFullScreenTouchArea();
            }
            else
            {
                // Leave touch area null for full screen touch capture
                Debug.Log("[MobileTouchSetup] Touch area left null - full screen touch enabled");
            }
        }
        
        private void CreateFullScreenTouchArea()
        {
            // Create touch area GameObject
            GameObject touchAreaObj = new GameObject(touchAreaName);
            touchAreaObj.transform.SetParent(mainCanvas.transform, false);
            
            // Add RectTransform and configure for full screen
            RectTransform rectTransform = touchAreaObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Add transparent image for raycasting
            Image image = touchAreaObj.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0); // Fully transparent
            image.raycastTarget = true; // Important!
            
            // Note: MobileTouchArea is read-only, so we can't assign it directly
            // The touch area needs to be assigned in the Unity Editor
            Debug.LogWarning($"[MobileTouchSetup] Created touch area '{touchAreaName}' but cannot assign it (read-only property). Please assign it manually in the Unity Editor.");
            
            // Make the touch area a child of the canvas for easy finding
            touchAreaObj.transform.SetAsLastSibling();
        }
        
        // Utility method to verify touch setup
        [ContextMenu("Verify Touch Setup")]
        public void VerifyTouchSetup()
        {
            Debug.Log("=== Touch Setup Verification ===");
            
            if (bridgeManager == null)
            {
                Debug.LogError("❌ UnityVerseBridgeManager not found");
                return;
            }
            
            Debug.Log($"✓ UnityVerseBridgeManager found: {bridgeManager.name}");
            Debug.Log($"  - Mode: {bridgeManager.Mode}");
            Debug.Log($"  - Touch Area: {(bridgeManager.MobileTouchArea != null ? bridgeManager.MobileTouchArea.name : "NULL (full screen)")}");
            
            var inputExtension = bridgeManager.GetComponent<Core.Extensions.Mobile.MobileInputExtension>();
            if (inputExtension != null)
            {
                Debug.Log("✓ MobileInputExtension component found");
                Debug.Log($"  - Enabled: {inputExtension.enabled}");
            }
            else
            {
                Debug.LogWarning("❌ MobileInputExtension component not found - will be added at runtime");
            }
            
            if (mainCanvas != null)
            {
                Debug.Log($"✓ Canvas found: {mainCanvas.name}");
                Debug.Log($"  - Render Mode: {mainCanvas.renderMode}");
                
                var touchArea = bridgeManager.MobileTouchArea;
                if (touchArea != null)
                {
                    var image = touchArea.GetComponent<Image>();
                    if (image != null)
                    {
                        Debug.Log($"✓ Touch Area Image component found");
                        Debug.Log($"  - Raycast Target: {image.raycastTarget}");
                        Debug.Log($"  - Alpha: {image.color.a}");
                    }
                }
            }
            
            Debug.Log("=== End Verification ===");
        }
    }
}