# UnityVerseBridge Mobile App

모바일 기기(iOS/Android)에서 실행되는 Unity 애플리케이션으로, Quest VR 앱과 WebRTC로 연결되어 VR 환경을 제어합니다.

## 🎯 프로젝트 개요

모바일 기기를 VR 컨트롤러로 활용하여 Quest VR 환경과 상호작용하는 앱입니다.

**주요 기능:**
- Quest VR 카메라 뷰 실시간 수신 및 표시
- 터치 입력을 VR 공간으로 전송
- 햅틱 피드백 수신 및 처리
- **[NEW] 양방향 오디오 통신 지원**
- **[NEW] UnityVerseBridge 통합 컴포넌트 사용**
- 저지연 P2P 통신

## 🎮 사용 시나리오

1. **VR 리모컨**: 모바일로 VR 내 UI 조작
2. **관전자 뷰**: VR 사용자의 시점 공유
3. **협업 도구**: VR-모바일 간 상호작용

## 🛠️ 기술 스택

- Unity 6 LTS (6000.0.33f1) 또는 Unity 2022.3 LTS
- Unity WebRTC 3.0.0-pre.8+
- Input System Package
- UnityVerseBridge.Core Package

## 📋 요구사항

### 하드웨어
- iOS 12+ 또는 Android 8+ 기기
- 개발용 PC (Windows/Mac)

### 소프트웨어
- Unity 6 LTS (6000.0.33f1) 이상 또는 Unity 2022.3 LTS
- iOS/Android Build Support
- Xcode (iOS 빌드 시)

## 🚀 설치 및 실행

### 1. 프로젝트 클론
```bash
git clone https://github.com/UnityVerseBridge/mobile-app.git
cd mobile-app
```

### 2. Unity 설정
1. Unity Hub에서 프로젝트 열기
2. Build Settings에서 플랫폼 선택 (iOS/Android)
3. Player Settings 확인:
   - Bundle Identifier 설정
   - Minimum iOS Version: 12.0
   - Minimum Android API: 26

### 3. 입력 시스템 설정
Project Settings > Player > Active Input Handling:
- "Both" 또는 "Input System Package" 선택

### 4. 빌드 및 실행
- iOS: Build > Xcode 프로젝트 생성 > 실기기 배포
- Android: Build and Run (USB 디버깅 활성화)

## 📁 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── MobileAppInitializer.cs   # 앱 초기화
│   ├── MobileVideoReceiver.cs    # 비디오 수신 및 표시
│   ├── MobileInputSender.cs      # 터치 입력 전송
│   ├── MobileHapticReceiver.cs   # 햅틱 피드백
│   ├── TouchInputTester.cs       # 터치 디버깅 도구
│   └── RoomIdInput.cs            # 룸 ID 입력 UI
├── Scenes/
│   └── SampleScene.unity         # 메인 씬
├── Prefabs/
│   └── UI/                       # UI 프리팹
└── ConnectionConfig.asset        # [NEW] 연결 설정
```

## 💡 핵심 컴포넌트 설명

### MobileAppInitializer
앱 시작 시 WebRTC 연결을 초기화하고 시그널링을 관리합니다.

**주요 기능:**
- WebRTC.Update() 코루틴 시작
- Answerer 역할로 설정 (Quest가 Offerer)
- 자동 재연결 로직

### MobileVideoReceiver
Quest에서 스트리밍되는 비디오를 수신하여 화면에 표시합니다.

**구현 특징:**
- OnVideoReceived 이벤트 기반 텍스처 업데이트
- 폴백 폴링 메커니즘
- 자동 종횡비 조정

### MobileInputSender
터치 입력을 감지하여 정규화된 좌표로 Quest에 전송합니다.

**처리 방식:**
- Editor/Desktop: 마우스 클릭을 터치로 변환
- Mobile: 네이티브 터치 입력 처리
- 60fps 전송률 제한

## 🔧 씬 구성

```
SampleScene
├── EventSystem
├── Main Camera
├── Canvas
│   ├── VideoDisplay (RawImage)
│   └── ConnectionStatus (Text)
├── WebRTC Manager
└── Mobile App Manager
    ├── MobileVideoReceiver
    ├── MobileInputSender
    └── MobileHapticReceiver
```

### 컴포넌트 설정

**MobileVideoReceiver:**
- Display Image: Canvas/VideoDisplay
- Receive Texture: (자동 생성됨)

**MobileInputSender:**
- Send Interval: 0.016 (60fps)
- WebRtcManager: (자동 검색)

## 🎨 UI 레이아웃

### 비디오 디스플레이
- Canvas Scaler: Scale With Screen Size
- Reference Resolution: 1920x1080
- RawImage: Stretch to fill

### 터치 영역
- 전체 화면이 터치 가능 영역
- UI 요소와 겹치지 않도록 주의

## 🔌 연결 프로세스

1. **Quest 앱 실행** (Offerer로 먼저 대기)
2. **Mobile 앱 실행** (자동으로 Quest 검색)
3. **P2P 연결 수립** (시그널링 서버 중재)
4. **스트림 시작** (양방향 통신)

## 🌐 네트워크 설정

### ConnectionConfig 설정
```
Signaling Server URL: ws://YOUR_SERVER_IP:YOUR_PORT
Room ID: default-room (Quest와 동일해야 함)
Client Type: Mobile
Auto Connect: true
Connection Timeout: 30
```

포트는 시그널링 서버의 .env 파일에서 설정한 포트를 사용합니다.

### 원격 서버 사용
1. ConnectionConfig.asset 선택
2. Inspector에서 URL 수정
3. 방화벽 포트 열기

## 📱 플랫폼별 고려사항

### iOS
- Info.plist에 카메라/마이크 권한 추가 (향후 오디오 기능용)
- Background Modes 설정 (필요시)
- IPv6 네트워크 지원

### Android
- AndroidManifest.xml 권한:
  ```xml
  <uses-permission android:name="android.permission.INTERNET" />
  <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
  <uses-permission android:name="android.permission.VIBRATE" />
  ```
- ProGuard 규칙 추가 (난독화 시)

## 🐛 문제 해결

### 비디오가 표시되지 않는 경우
1. `[MobileVideoReceiver] Video received` 로그 확인
2. RawImage 컴포넌트 활성화 확인
3. Canvas 렌더링 순서 확인

### 터치가 전송되지 않는 경우
1. DataChannel 상태 확인
2. Input System 설정 확인
3. `[MobileInputSender] Sent touch` 로그 확인

### 연결이 끊어지는 경우
1. 네트워크 안정성 확인
2. 시그널링 서버 상태 확인
3. 자동 재연결 대기

## 🚀 성능 최적화

### 비디오 스트리밍
- 해상도: 720p 권장
- 프레임레이트: 30fps
- 하드웨어 디코딩 활용

### 터치 입력
- 전송 빈도: 60Hz
- 배치 처리로 네트워크 부하 감소

### 메모리 관리
- 텍스처 풀링
- 적절한 가비지 컬렉션

## 🔒 보안

- TLS/SSL 지원 (wss://)
- 룸 기반 격리
- 인증 토큰 지원 (향후 JWT 구현)

## 📊 디버깅 도구

### TouchInputTester
터치 입력 문제 진단용:
1. 씬에 빈 GameObject 생성
2. TouchInputTester 컴포넌트 추가
3. Console에서 터치 로그 확인

### Input Debugger
Window > Analysis > Input Debugger:
- 실시간 입력 상태 모니터링
- 터치 시뮬레이션 설정

### 오디오 레벨 모니터링
AudioStreamManager는 마이크 입력 레벨을 실시간으로 제공합니다:
- OnMicrophoneLevelChanged 이벤트 구독
- UI에 오디오 레벨 표시 가능

## 🚧 향후 개발 계획

### 우선순위 높음
- 오디오 수신/송신 기능
- AR 뷰 모드

### 중간 우선순위
- 멀티터치 지원
- 제스처 인식
- 1:N 연결 대응

### 장기 계획
- 백그라운드 모드 최적화
- 적응형 스트리밍
- 클라우드 연결 지원

## 📄 라이선스

이 프로젝트는 BSD 3-Clause 라이선스를 따릅니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참고하세요.

## 👥 제작자

- **kugorang** - [GitHub](https://github.com/kugorang)

---

문제가 있거나 제안사항이 있으시면 [Issues](https://github.com/UnityVerseBridge/mobile-app/issues)에 등록해주세요.
