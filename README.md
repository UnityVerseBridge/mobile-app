# UnityVerse Mobile App

iOS/Android 애플리케이션 - VR 스트림 수신 및 터치 입력 전송

## 개요

모바일 애플리케이션 주요 기능:
- VR 카메라 스트림 수신 및 표시
- VR 디바이스로 터치 입력 전송
- 룸 탐색 및 연결 UI
- 크로스 플랫폼 모바일 지원

## 요구사항

- Unity 6 LTS (6000.0.33f1) 또는 Unity 2022.3 LTS
- UnityVerseBridge Core 패키지
- iOS Build Support (iOS용)
- Android Build Support (Android용)
- 최소 iOS 12.0 / Android API 26

## 프로젝트 설정

1. 저장소 클론
2. Unity에서 `UnityProject` 폴더 열기
3. 필수 패키지 임포트:
   - UnityVerseBridge Core
   - Unity WebRTC

## 구성

1. 샘플 씬 열기: `Assets/Scenes/MobileStreamingDemo.unity`
2. `UnityVerseBridge_Mobile` GameObject 찾기
3. UnityVerseConfig 설정:
   - Signaling URL: 시그널링 서버 주소
   - Room ID: Quest 앱과 동일
   - Auto Connect: 자동 연결 활성화

## 모바일 빌드

### iOS 빌드
1. File > Build Settings > iOS
2. Player Settings:
   - Minimum iOS Version: 12.0
   - Camera Usage Description: AR 기능용
3. Xcode에서 빌드 및 열기
4. 서명 후 디바이스에 배포

### Android 빌드
1. File > Build Settings > Android
2. Player Settings:
   - Minimum API Level: 26
   - Target API Level: 33+
   - Internet Access: Required
3. Build and Run

## 주요 컴포넌트

### MobileRoomUIAdapter
룸 선택 및 연결 UI 관리:
- 룸 목록 동적 업데이트
- 수동 룸 ID 입력
- QR 코드 스캔 지원 (예정)

### MobileVideoDebugger
비디오 스트리밍 문제 디버깅:
- 다양한 렌더링 방법 테스트
- 디코더 초기화 모니터링
- 텍스처 업데이트 추적

### MobileMenuController
인앱 메뉴 시스템:
- 연결 상태 표시
- 디버그 모드 토글
- 비디오 품질 설정

## UI 컴포넌트

Core UI 컴포넌트 활용:
- **RoomListUI**: 동적 룸 탐색
- **RoomInputUI**: 수동 룸 입력
- **UIManager**: 중앙집중식 UI 관리

## 테스트 방법

1. 시그널링 서버 시작
2. Quest 앱 먼저 실행 (룸 생성)
3. Mobile 앱 실행
4. 동일한 룸 ID 입력 또는 목록에서 선택
5. 화면 터치로 VR에 입력 전송

## 문제 해결

### 비디오 미표시
- 양 디바이스가 동일한 룸에 있는지 확인
- 시그널링 서버 연결 확인
- 디버그 모드로 상세 로그 확인

### 터치 작동 안 함
- 데이터 채널 열림 확인
- 터치 영역 설정 확인
- VR 앱이 입력 수신 중인지 확인

### 연결 문제
- 네트워크 연결 확인
- 방화벽 설정 확인
- 시그널링 서버 접근 가능 확인

## 성능 팁

- WiFi 사용 권장
- 다른 앱 종료로 리소스 확보
- Player Settings에서 하드웨어 가속 활성화
- 기본 비디오 품질: Low (640x360)
- 지원 해상도: 360p, 720p, 1080p
- H264 코덱 하드웨어 디코딩 선호

## 라이선스

루트 저장소의 LICENSE 파일 참조