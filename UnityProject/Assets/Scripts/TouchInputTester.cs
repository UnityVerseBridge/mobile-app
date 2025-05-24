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
            // Legacy Input System 테스트
            if (Input.touchCount > 0)
            {
                Debug.Log($"[TouchInputTester] Legacy Input: {Input.touchCount} touches detected");
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    Debug.Log($"  Touch {i}: {touch.phase} at {touch.position}");
                }
            }

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
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[TouchInputTester] Mouse Click at {Input.mousePosition}");
            }
        }
    }
}
