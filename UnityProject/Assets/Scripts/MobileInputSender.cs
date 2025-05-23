using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.DataChannel.Data;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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
            if (webRtcManager == null)
            {
                webRtcManager = FindObjectOfType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileInputSender] WebRtcManager not found!");
                    enabled = false;
                }
            }
        }

        void Update()
        {
            if (!webRtcManager.IsDataChannelOpen) return;
            
            // 전송 빈도 제한
            if (Time.time - lastSendTime < sendInterval) return;
            
            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count == 0) return;

            foreach (var touch in activeTouches)
            {
                SendTouchData(touch);
            }
            
            lastSendTime = Time.time;
        }

        private void SendTouchData(Touch touch)
        {
            // 화면 좌표를 정규화 (0-1)
            float normalizedX = touch.screenPosition.x / Screen.width;
            float normalizedY = touch.screenPosition.y / Screen.height;

            var touchData = new TouchData
            {
                type = "touch",
                touchId = touch.touchId,
                phase = ConvertPhase(touch.phase),
                positionX = normalizedX,
                positionY = normalizedY
            };

            webRtcManager.SendDataChannelMessage(touchData);
            
            Debug.Log($"[MobileInputSender] Sent touch: ID={touchData.touchId}, Pos=({normalizedX:F3}, {normalizedY:F3})");
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
