# Unity Player Settings 설정 가이드

## Mobile App을 위한 Unity Player Settings

### 1. 기본 설정 (Company Name & Product Name)
- **Company Name**: UnityVerseBridge (또는 원하는 회사명)
- **Product Name**: UnityVerse Mobile ← 이것이 앱 이름이 됩니다!

### 2. Android 설정
Edit > Project Settings > Player > Android 탭에서:

#### Identification
- **Package Name**: com.unityversebridge.mobile
- **Version**: 1.0
- **Bundle Version Code**: 1
- **Minimum API Level**: Android 7.0 'Nougat' (API level 24)
- **Target API Level**: Automatic (highest installed)

#### Configuration
- **Scripting Backend**: IL2CPP
- **Api Compatibility Level**: .NET Standard 2.1
- **Target Architectures**: ✅ ARMv7, ✅ ARM64

#### Publishing Settings
- **Custom Main Manifest**: ✅ 체크 (이미 AndroidManifest.xml이 있음)
- **Custom Main Gradle Template**: 필요시 체크
- **Custom Gradle Properties Template**: 필요시 체크

#### Other Settings
- **Auto Graphics API**: ✅ 체크
- **Multithreaded Rendering**: ✅ 체크
- **Static Batching**: ✅ 체크
- **Dynamic Batching**: ❌ 체크 해제 (성능 이슈)
- **GPU Skinning**: ✅ 체크

#### Internet Access
- **Internet Access**: Require (WebRTC 연결에 필수)

### 3. 아이콘 설정
- Player Settings > Icon
- Default Icon에 앱 아이콘 이미지 할당
- Adaptive Icon은 Android 8.0+를 위한 선택사항

### 4. 권한 설정
Unity가 자동으로 다음 권한들을 추가합니다:
- INTERNET (Internet Access 설정으로)
- ACCESS_NETWORK_STATE
- VIBRATE (햅틱 사용 시)
- RECORD_AUDIO (마이크 사용 시)
- MODIFY_AUDIO_SETTINGS

우리가 AndroidManifest.xml에 추가한 권한:
- CAMERA (WebRTC 비디오용)

### 5. 빌드 전 체크리스트
- [ ] Product Name이 "UnityVerse Mobile"로 설정됨
- [ ] Package Name이 올바르게 설정됨
- [ ] Internet Access가 "Require"로 설정됨
- [ ] Custom Main Manifest가 체크됨
- [ ] AndroidManifest.xml이 Assets/Plugins/Android/에 있음