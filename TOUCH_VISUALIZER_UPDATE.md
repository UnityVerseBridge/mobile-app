# Mobile App 터치 시각화 업데이트

## 변경 사항

### 1. 터치 시각화 항상 표시
- **이전**: Debug Mode가 활성화되어야 붉은 점 표시
- **현재**: Debug Mode와 관계없이 항상 붉은 점 표시
- `showTouchVisualizer` 기본값이 `true`로 변경됨

### 2. 붉은 점 크기 2배 증가
- **원 크기**: 100x100 → 200x200 픽셀
- **텍스처 크기**: 64x64 → 128x128 픽셀
- **스케일**: 
  - 일반: 1.0 → 2.0
  - 터치 시작: 1.2 → 2.4

## 적용 방법

### Unity에서 확인
1. Mobile App의 `UnityVerseBridge_Mobile` GameObject 선택
2. `MobileInputExtension` 컴포넌트 확인
3. 다음 설정 확인:
   - `Show Touch Visualizer`: ✓ (체크됨)
   - `Debug Mode`: 체크 여부와 관계없이 터치 표시됨

### 테스트
1. Mobile App 실행
2. WebRTC 연결 전에도 터치 시 붉은 점 표시 확인
3. 붉은 점이 이전보다 2배 크게 표시되는지 확인

## 커스터마이징

### 색상 변경
`touchColors` 배열을 수정하여 터치 색상 변경 가능:
```csharp
private readonly Color[] touchColors = new Color[] 
{ 
    Color.red, Color.green, Color.blue, Color.yellow, Color.magenta
};
```

### 크기 조정
더 크게 하려면:
1. `rect.sizeDelta = new Vector2(300, 300);` 으로 변경
2. `float scale = phase == ... ? 3.0f : 2.5f;` 으로 변경

### 터치 피드백 프리팹 사용
커스텀 터치 시각화를 원한다면:
1. UI Image를 포함한 Prefab 생성
2. `Touch Feedback Prefab` 필드에 할당
3. 자동으로 해당 프리팹 사용

## 주의사항
- 터치 시각화는 Canvas에 그려짐 (Screen Space Overlay)
- 최대 10개의 동시 터치 지원
- 터치가 끝나면 50% 투명도로 페이드 아웃