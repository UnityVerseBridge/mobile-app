using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// 터치 입력 테스트용 간단한 스크립트
    /// </summary>
    public class TouchInputTester : MonoBehaviour
    {
        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            Debug.Log("[TouchInputTester] Enhanced Touch Support Enabled");
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Update()
        {
            // New Input System Enhanced Touch 테스트
            var enhancedTouches = Touch.activeTouches;
            if (enhancedTouches.Count > 0)
            {
                Debug.Log($"[TouchInputTester] Enhanced Touch: {enhancedTouches.Count} touches detected");
                foreach (var touch in enhancedTouches)
                {
                    Debug.Log($"  Touch {touch.touchId}: {touch.phase} at {touch.screenPosition}");
                }
            }

            // 마우스 클릭 테스트 (Editor에서)
#if UNITY_EDITOR
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                Debug.Log($"[TouchInputTester] Mouse Click at {mouse.position.ReadValue()}");
            }
#endif
        }
    }
}
