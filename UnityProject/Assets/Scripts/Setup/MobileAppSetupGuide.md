# Mobile App Unity Editor Setup Guide

## 비디오 스트리밍 수신을 위한 필수 설정

### 1. UnityVerseBridgeManager 설정
1. Scene에 빈 GameObject 생성 (이름: "UnityVerseBridge")
2. `UnityVerseBridgeManager` 컴포넌트 추가
3. Inspector 설정:
   - **Configuration**: ConnectionConfig ScriptableObject 할당
   - **Mode**: `Client` 선택 (중요!)
   - **Mobile Video Display**: RawImage 할당 (비디오가 표시될 UI)

### 2. ConnectionConfig 생성 및 설정
1. Project 창에서 우클릭 → Create → UnityVerse Bridge → Connection Config
2. 생성된 ConnectionConfig 설정:
   ```
   - Signaling Server URL: ws://YOUR_SERVER_IP:8080
   - Room ID: test-room (Quest 앱과 동일하게)
   - Client Type: Mobile
   - Use Session Room ID: false (수동으로 룸 ID 입력)
   - Auto Connect: true
   ```

### 3. UI 구성
1. **Canvas 생성**
   - GameObject → UI → Canvas
   - Canvas Scaler 설정:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080

2. **Video Display RawImage 생성**
   - Canvas 하위에 RawImage 생성 (이름: "VideoDisplay")
   - Rect Transform 설정:
     - Anchor: Stretch (전체 화면)
     - Left/Top/Right/Bottom: 0
   - Raw Image 컴포넌트:
     - Texture: 비워둠 (런타임에 할당됨)
     - Color: White (Alpha = 1)

3. **AspectRatioFitter 추가** (선택사항)
   - VideoDisplay에 Aspect Ratio Fitter 컴포넌트 추가
   - Aspect Mode: Fit In Parent
   - Aspect Ratio: 1.777778 (16:9)

### 4. MobileVideoExtension 설정
1. UnityVerseBridge GameObject에 `MobileVideoExtension` 컴포넌트 추가
2. Inspector 설정:
   ```
   Display Settings:
   - Display Image: VideoDisplay RawImage 할당
   - Receive Texture: 비워둠 (자동 생성됨)
   - Auto Create Texture: true (체크)
   - Texture Width: 1280
   - Texture Height: 720
   
   Aspect Ratio:
   - Maintain Aspect Ratio: true
   - Aspect Mode: Fit In Parent
   
   Debug:
   - Debug Mode: true (문제 해결 시)
   - Show Debug Info: true
   ```

### 5. VideoStreamHandler 설정 (Core 컴포넌트)
UnityVerseBridgeManager가 자동으로 생성하지만, 수동 설정이 필요한 경우:
1. `VideoStreamHandler` 컴포넌트 확인
2. Client 모드 설정:
   - Display Image: VideoDisplay RawImage
   - Receive Texture: null (자동 생성)

### 6. WebRTC 패키지 확인
Package Manager에서 확인:
- com.unity.webrtc: 3.0.0-pre.8 이상

### 7. Platform 설정
Build Settings:
- Platform: Android 또는 iOS
- Android:
  - Minimum API Level: 26
  - Target API Level: 30 이상
  - Internet Access: Required
- iOS:
  - Target minimum iOS Version: 12.0
  - Camera Usage Description: 필요 시 추가

### 8. 실행 순서
1. **Signaling Server 시작**
   ```bash
   cd signaling-server
   npm start
   ```

2. **Quest 앱 실행** (Host)
   - Room ID 확인 (예: test-room)
   - "Connected to signaling server" 로그 확인

3. **Mobile 앱 실행** (Client)
   - 같은 Room ID 입력
   - 자동 연결 시작

### 9. 디버깅 체크리스트
로그에서 확인해야 할 순서:
1. `[UnityVerseBridgeManager] Initializing in Client mode`
2. `[SignalingClient] Connected to server`
3. `[WebRtcManager] Received offer`
4. `[WebRtcManager] Video track received`
5. `[MobileVideoExtension] Video received via OnVideoReceived`

### 10. 일반적인 문제 해결

#### 비디오가 표시되지 않는 경우:
1. **RawImage 확인**
   - Active 상태인지 확인
   - Alpha 값이 1인지 확인
   - Canvas 내 올바른 위치에 있는지 확인

2. **RenderTexture 포맷**
   - Android: BGRA32 또는 ARGB32
   - iOS: BGRA32 권장

3. **디버그 모드 활성화**
   - MobileVideoExtension의 Debug Mode 켜기
   - Scene에 Debug Text UI 추가하여 상태 확인

#### 연결은 되지만 비디오가 없는 경우:
1. Quest 앱에서 카메라가 RenderTexture에 렌더링되는지 확인
2. VideoStreamTrack이 생성되고 활성화되었는지 확인
3. 네트워크 방화벽 설정 확인

### 샘플 Scene 구조
```
Canvas
├── VideoDisplay (RawImage)
│   ├── AspectRatioFitter
│   └── MobileVideoExtension (선택사항)
├── MenuUI
│   ├── ConnectButton
│   ├── RoomIdInput
│   └── StatusText
└── DebugUI
    └── DebugText

UnityVerseBridge (GameObject)
├── UnityVerseBridgeManager
├── WebRtcManager (자동 생성)
├── VideoStreamHandler (자동 생성)
└── MobileVideoExtension

UICleanupManager (GameObject)
└── UICleanupManager (Component)