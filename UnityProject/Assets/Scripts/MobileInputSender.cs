using UnityEngine;
using UnityEngine.InputSystem; // Input System 사용
using UnityEngine.InputSystem.EnhancedTouch; // Enhanced Touch API 사용 (더 권장됨)
using UnityVerseBridge.Core; // WebRtcManager 사용
using UnityVerseBridge.Core.DataChannel.Data; // TouchData 사용

public class MobileInputSender : MonoBehaviour
{
    [SerializeField] private WebRtcManager webRtcManager;
    // 터치 입력을 받을 영역 지정 (선택 사항, 예: 특정 Panel)
    // [SerializeField] private RectTransform touchArea;

    void OnEnable()
    {
        // Enhanced Touch API 활성화 (더 정확하고 다양한 터치 정보 제공)
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable(); // 에디터에서 마우스로 테스트하려면 추가
    }

    void OnDisable()
    {
        // Enhanced Touch API 비활성화
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable(); // 에디터 테스트용 비활성화
    }

    void Update()
    {
        // WebRTC 연결 및 데이터 채널이 열려있을 때만 처리
        // 참고: WebRtcManager에 데이터 채널 상태를 확인하는 IsDataChannelOpen 같은 속성을 추가하면 더 명확함
        if (webRtcManager == null || !webRtcManager.IsWebRtcConnected)
        {
            return;
        }

        // 활성화된 모든 터치 정보 가져오기
        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches) // EnhancedTouch.Touch 사용
        {
            // 터치 상태 (Began, Moved, Ended 등) 확인
            UnityEngine.InputSystem.TouchPhase currentPhase = touch.phase;

            // 특정 영역 내 터치만 처리 (선택 사항)
            // if (touchArea != null && !RectTransformUtility.RectangleContainsScreenPoint(touchArea, touch.screenPosition, null))
            // {
            //     continue; // 터치 영역 밖이면 무시
            // }

            // 화면 좌표 (픽셀)
            Vector2 screenPosition = touch.screenPosition;
            // 정규화된 좌표 (0.0 ~ 1.0)
            Vector2 normalizedPosition = new Vector2(
                screenPosition.x / Screen.width,
                screenPosition.y / Screen.height
            );

            // TouchData 객체 생성 (Core 패키지의 클래스 사용)
            // InputSystem.TouchPhase -> 우리 Enum 타입으로 변환 필요
            UnityVerseBridge.Core.DataChannel.Data.TouchPhase bridgePhase = ConvertPhase(currentPhase);
            TouchData touchData = new TouchData(touch.touchId, bridgePhase, normalizedPosition);

            // 로그 출력 (디버깅용)
            Debug.Log($"[MobileInputSender] Sending Touch: ID={touchData.touchId}, Phase={touchData.phase}, Pos=({touchData.positionX:F3}, {touchData.positionY:F3})");

            // WebRtcManager를 통해 데이터 전송
            // WebRtcManager의 SendDataChannelMessage가 object를 받아 JsonUtility.ToJson을 내부적으로 호출한다고 가정
            webRtcManager.SendDataChannelMessage(touchData);

            // Ended 또는 Canceled 상태일 때 루프 종료 (선택 사항)
            // if (currentPhase == UnityEngine.InputSystem.TouchPhase.Ended || currentPhase == UnityEngine.InputSystem.TouchPhase.Canceled)
            // {
            //     // 필요시 추가 처리
            // }
        }
    }

    // InputSystem의 TouchPhase를 우리가 정의한 Enum으로 변환하는 헬퍼 함수
    private UnityVerseBridge.Core.DataChannel.Data.TouchPhase ConvertPhase(UnityEngine.InputSystem.TouchPhase inputPhase)
    {
        switch (inputPhase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began: 
                return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Began;
            case UnityEngine.InputSystem.TouchPhase.Moved: 
                return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Moved;
            case UnityEngine.InputSystem.TouchPhase.Ended: 
                return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Ended;
            case UnityEngine.InputSystem.TouchPhase.Canceled: 
                return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Canceled;
            // case UnityEngine.InputSystem.TouchPhase.Stationary: 
            //     return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Stationary;
            default: 
                return UnityVerseBridge.Core.DataChannel.Data.TouchPhase.Moved; // 기본값 또는 예외 처리
        }
    }
}