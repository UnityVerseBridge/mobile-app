# Clean Build Guide for Mobile App

## res 폴더 에러 해결 방법

Unity 최신 버전에서 `Assets/Plugins/Android/res` 폴더 사용이 금지되어 발생하는 에러입니다.

### 해결 단계:

1. **Unity 에디터 종료**

2. **캐시 정리** (터미널에서):
```bash
# Library 폴더의 Android 관련 캐시 삭제
rm -rf Library/Bee/Android
rm -rf Library/Bee/artifacts/Android
```

3. **Unity 에디터 재시작**

4. **Clean Build 수행**:
   - File > Build Settings
   - Android 플랫폼 선택
   - "Clean Build" 옵션 체크 (있는 경우)
   - Build

### 대안: 전체 Library 폴더 삭제

더 확실한 방법:
1. Unity 에디터 종료
2. 프로젝트 폴더에서 Library 폴더 전체 삭제
3. Unity 에디터로 프로젝트 다시 열기 (자동으로 Library 재생성)
4. 빌드 수행

### 현재 AndroidManifest.xml 상태

최소화된 버전으로 설정됨:
- CAMERA 권한만 명시
- android:exported="true" 포함
- 앱 이름은 Unity Player Settings에서 관리

### Unity Player Settings 확인

- Product Name: UnityVerse Mobile
- Package Name: com.unityversebridge.mobile
- Internet Access: Require
- Custom Main Manifest: ✅ 체크