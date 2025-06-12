# UI Cleanup System Setup Guide

## Overview
Mobile 앱에서 메뉴를 통해 생성되는 모든 UI 요소들(Canvas 포함)을 추적하고 정리하는 시스템입니다.

## Unity Editor 설정

### 1. Tag 생성
Unity Editor에서 다음 태그를 생성하세요:
- **MenuUI** - 메뉴를 통해 생성된 모든 UI 요소에 사용

### 2. Layer 생성 (선택사항)
필요한 경우 UI Layer가 있는지 확인하세요 (기본적으로 Unity에 포함되어 있음)

### 3. UICleanupManager 설정
1. 빈 GameObject 생성
2. `UICleanupManager` 컴포넌트 추가
3. Inspector에서 설정:
   - Menu UI Tag: "MenuUI"
   - Menu UI Layer: "UI"

### 4. MobileMenuController 설정
1. UI Canvas에 빈 GameObject 생성 (예: "MenuController")
2. `MobileMenuController` 컴포넌트 추가
3. UI 요소 생성 및 연결:
   - **Menu Panel**: 메뉴 컨테이너
   - **Menu Toggle Button**: 메뉴 열기/닫기 버튼
   - **Disconnect Button**: 연결 해제 버튼
   - **Cleanup UI Button**: UI 정리 버튼
   - **Refresh Rooms Button**: 룸 목록 새로고침 버튼
   - **Debug Mode Toggle**: 디버그 모드 토글
   - **Connection Status Text**: 연결 상태 표시
   - **Cleanup Status Text**: 정리 상태 메시지

## 사용 방법

### 코드에서 UI 요소 추적하기
```csharp
// GameObject 생성 시 추적
GameObject uiElement = Instantiate(prefab);
UICleanupManager.Instance.TrackGameObject(uiElement);

// Canvas 생성 및 추적
Canvas canvas = UICleanupManager.Instance.CreateTrackedCanvas("MyCanvas");
```

### 메뉴를 통한 정리
1. 메뉴 토글 버튼 클릭하여 메뉴 열기
2. "Cleanup UI" 버튼 클릭하여 모든 추적된 UI 정리
3. "Disconnect" 버튼으로 연결 해제 및 UI 정리

### 자동 정리
- `UnityVerseBridgeManager.Disconnect()` 호출 시 자동으로 UI 정리
- Scene 전환 시 자동 정리
- 앱 종료 시 자동 정리

## 주의사항

1. **Tag 생성 필수**: "MenuUI" 태그가 없으면 시스템이 제대로 작동하지 않습니다.
2. **메모리 누수 방지**: 동적으로 생성한 UI는 반드시 `TrackGameObject()`로 추적하세요.
3. **이벤트 리스너**: Button, Toggle 등의 이벤트 리스너는 자동으로 정리됩니다.

## 디버깅

### 추적 상태 확인
```csharp
var (objectCount, canvasCount) = UICleanupManager.Instance.GetTrackedCounts();
Debug.Log($"Tracking {objectCount} objects and {canvasCount} canvases");
```

### Inspector에서 확인
`UICleanupManager`의 Inspector에서:
- Tracked Objects: 현재 추적 중인 GameObject 목록
- Tracked Canvases: 현재 추적 중인 Canvas 목록

## 확장 방법

새로운 UI 생성 로직 추가 시:
1. UI 생성 후 `UICleanupManager.Instance.TrackGameObject()` 호출
2. Canvas가 포함된 경우 자동으로 별도 추적됨
3. 특별한 정리 로직이 필요한 경우 `OnDestroy()`에서 `UntrackGameObject()` 호출