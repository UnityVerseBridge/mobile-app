# Mobile App Android Visibility Fix

## 수정 완료 사항

Mobile 앱이 Android 기기의 앱 목록에서 보이지 않는 문제를 해결했습니다.

### AndroidManifest.xml 수정 내용

1. **Activity 속성 추가**
   - `android:exported="true"` - Android 12+ 필수
   - `android:excludeFromRecents="false"` - 최근 앱 목록에 표시
   - `android:screenOrientation="fullUser"` - 화면 회전 지원

2. **Intent Filter 카테고리 추가**
   - `android.intent.category.DEFAULT` - 기본 카테고리
   - `android.intent.category.LEANBACK_LAUNCHER` - Android TV 지원

3. **앱 정보 설정**
   - `android:label="UnityVerse Mobile"` - 앱 이름
   - `android:icon="@mipmap/app_icon"` - 앱 아이콘
   - `android:theme="@style/UnityThemeSelector"` - Unity 테마

4. **추가 권한**
   - 햅틱 피드백 기능 지원
   - 파일 시스템 접근 권한 (선택적)

### 문제 해결 방법

#### 앱이 여전히 보이지 않는다면:

1. **앱 설치 확인**
   ```bash
   adb shell pm list packages | grep unityverse
   ```

2. **직접 실행**
   ```bash
   adb shell am start -n com.unityversebridge.mobile/com.unity3d.player.UnityPlayerActivity
   ```

3. **설정에서 확인**
   - 설정 > 앱 > 모든 앱 표시
   - "UnityVerse Mobile" 검색

4. **런처 앱 문제**
   - 다른 런처 앱 사용해보기
   - 기본 런처로 변경 후 확인

### 빌드 시 주의사항

1. **Package Name 확인**
   - Unity의 Player Settings에서 Package Name 확인
   - 기본값: `com.unityversebridge.mobile`

2. **Target API Level**
   - Android 12 (API 31) 이상은 `android:exported` 필수
   - Unity Player Settings에서 Target API Level 확인

3. **아이콘 설정**
   - Unity Player Settings > Icon에서 설정
   - 또는 `Assets/Plugins/Android/res/mipmap-*/app_icon.png` 추가

### 권한 요청 처리

앱 실행 시 다음 권한들을 요청합니다:
- **카메라**: WebRTC 비디오 스트리밍 (선택적)
- **마이크**: WebRTC 오디오 (선택적)
- **인터넷**: 필수 (시그널링 서버 연결)

Unity의 Permission Request Manager를 사용하거나 런타임에 요청하세요.