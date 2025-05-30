using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.DataChannel.Data;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityVerseBridge.Core.DataChannel.Data.TouchPhase;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// 모바일 터치 입력을 WebRTC로 전송
    /// </summary>
    public class MobileInputSender : MonoBehaviour
    {
        [SerializeField] private WebRtcManager webRtcManager;
        [SerializeField] private float sendInterval = 0.016f; // 60fps
        
        private float lastSendTime;

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        void Start()
        {
            Debug.Log("[MobileInputSender] Starting...");
            
            if (webRtcManager == null)
            {
                webRtcManager = FindObjectOfType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileInputSender] WebRtcManager not found!");
                    enabled = false;
                    return;
                }
            }
            
            Debug.Log($"[MobileInputSender] WebRtcManager found: {webRtcManager.name}");
            
            // Input System 상태 확인
            Debug.Log($"[MobileInputSender] Enhanced Touch Enabled: {EnhancedTouchSupport.enabled}");
            Debug.Log($"[MobileInputSender] Touch simulation: {UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.instance?.enabled ?? false}");
        }

        void Update()
        {
            // DataChannel 상태 확인
            if (!webRtcManager.IsDataChannelOpen)
            {
                return;
            }
            
            // 전송 빈도 제한
            if (Time.time - lastSendTime < sendInterval) return;
            
            // Unity Editor나 Standalone에서 마우스 입력 처리
            #if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
            {
                SendMouseAsTouch();
                lastSendTime = Time.time;
                return;
            }
            #endif
            
            // 모바일에서 터치 입력 처리
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    SendLegacyTouchData(Input.GetTouch(i));
                }
                lastSendTime = Time.time;
            }
        }

        private void SendMouseAsTouch()
        {
            // 마우스 위치를 정규화 (0-1)
            float normalizedX = Input.mousePosition.x / Screen.width;
            float normalizedY = Input.mousePosition.y / Screen.height;
            
            TouchPhase phase = TouchPhase.Moved;
            if (Input.GetMouseButtonDown(0)) phase = TouchPhase.Began;
            else if (Input.GetMouseButtonUp(0)) phase = TouchPhase.Ended;
            
            var touchData = new TouchData
            {
                type = "touch",
                touchId = 0, // 마우스는 항상 ID 0
                phase = phase,
                positionX = normalizedX,
                positionY = normalizedY
            };

            webRtcManager.SendDataChannelMessage(touchData);
            Debug.Log($"[MobileInputSender] Sent mouse as touch: Phase={phase}, Pos=({normalizedX:F3}, {normalizedY:F3})");
        }
        
        private void SendLegacyTouchData(UnityEngine.Touch touch)
        {
            // 화면 좌표를 정규화 (0-1)
            float normalizedX = touch.position.x / Screen.width;
            float normalizedY = touch.position.y / Screen.height;

            var touchData = new TouchData
            {
                type = "touch",
                touchId = touch.fingerId,
                phase = ConvertLegacyPhase(touch.phase),
                positionX = normalizedX,
                positionY = normalizedY
            };

            webRtcManager.SendDataChannelMessage(touchData);
            Debug.Log($"[MobileInputSender] Sent touch: ID={touchData.touchId}, Phase={touchData.phase}, Pos=({normalizedX:F3}, {normalizedY:F3})");
        }
        
        private UnityVerseBridge.Core.DataChannel.Data.TouchPhase ConvertLegacyPhase(UnityEngine.TouchPhase phase)
        {
            return phase switch
            {
                UnityEngine.TouchPhase.Began => TouchPhase.Began,
                UnityEngine.TouchPhase.Moved => TouchPhase.Moved,
                UnityEngine.TouchPhase.Stationary => TouchPhase.Moved,
                UnityEngine.TouchPhase.Ended => TouchPhase.Ended,
                UnityEngine.TouchPhase.Canceled => TouchPhase.Canceled,
                _ => TouchPhase.Canceled
            };
        }

        private UnityVerseBridge.Core.DataChannel.Data.TouchPhase ConvertPhase(UnityEngine.InputSystem.TouchPhase inputPhase)
        {
            return inputPhase switch
            {
                UnityEngine.InputSystem.TouchPhase.Began => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Began,
                UnityEngine.InputSystem.TouchPhase.Moved => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Stationary => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Moved, // Stationary를 Moved로 매핑
                UnityEngine.InputSystem.TouchPhase.Ended => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Ended,
                UnityEngine.InputSystem.TouchPhase.Canceled => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Canceled,
                _ => UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Canceled
            };
        }
    }
}
