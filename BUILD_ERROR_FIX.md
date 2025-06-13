# Mobile App Build Error Fix

## 해결된 빌드 에러들

### 1. AndroidManifest.xml 라벨 충돌
**에러 메시지:**
```
Attribute application@label value=(@string/app_name) from AndroidManifest.xml:3:48-80
is also present at [:unityLibrary] AndroidManifest.xml:66:9-42 value=(UnityVerse Mobile).
```

**해결:**
- AndroidManifest.xml을 최소화하여 Unity가 대부분 자동 처리하도록 함
- 앱 라벨은 Unity Player Settings에서 설정 (Product Name)
- 필수 요소만 포함: CAMERA 권한, 메인 액티비티, android:exported="true"
- res 폴더 사용 금지 (Unity 최신 버전에서 deprecated)

### 2. XR Simulation 관련 에러
**에러 메시지:**
```
Script attached to 'XRSimulationRuntimeSettings' in asset 'Assets/XR/Resources/XRSimulationRuntimeSettings.asset' is missing
Script attached to 'XRSimulationPreferences' in asset 'Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset' is missing
```

**해결:**
- Mobile 앱에는 XR Simulation이 필요 없으므로 관련 에셋 삭제
- `Assets/XR/Resources/XRSimulationRuntimeSettings.asset` 삭제
- `Assets/XR/UserSimulationSettings/Resources/XRSimulationPreferences.asset` 삭제

## 빌드 전 체크리스트

1. **AndroidManifest.xml 확인**
   - `tools:replace="android:label"` 속성이 있는지 확인
   - 앱 이름이 제대로 설정되어 있는지 확인

2. **불필요한 XR 에셋 제거**
   - Mobile 앱에서는 XR 관련 에셋이 필요 없음
   - XR 폴더가 있다면 삭제 고려

3. **Unity Player Settings**
   - Product Name: UnityVerse Mobile
   - Package Name: com.unityversebridge.mobile (또는 원하는 패키지명)
   - Target API Level: Android 12 (API 31) 이상

4. **Gradle 경고 (선택사항)**
   - compileSdk = 35 경고는 무시해도 됨
   - 최신 Android Gradle 플러그인 업데이트는 Unity 버전에 따라 결정

## 재빌드 방법

1. Unity에서 File > Build Settings
2. Player Settings에서 설정 확인
3. Build 또는 Build And Run

이제 빌드가 성공적으로 완료되어야 합니다!