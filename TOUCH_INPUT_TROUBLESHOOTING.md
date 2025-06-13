# Touch Input Troubleshooting Guide for Mobile App

## Overview
This guide helps you troubleshoot touch input issues in the UnityVerse mobile app where touches are not being transmitted to the Quest app.

## Common Issues and Solutions

### 1. MobileInputExtension Not Working

The `MobileInputExtension` component is automatically added when running in Client mode, but it requires proper setup.

#### Check List:
- [ ] **Enhanced Touch Support**: Unity's Enhanced Touch API must be enabled
- [ ] **WebRTC Connection**: Data channel must be open before touches can be sent
- [ ] **Touch Area**: Either configure a touch area or leave it null for full-screen touch

### 2. Touch Area Configuration

#### Option A: Full Screen Touch (Easiest)
1. In Unity, select the `UnityVerseBridge` GameObject
2. In the `UnityVerseBridgeManager` component:
   - Leave `Mobile Touch Area` field **empty (None)**
3. The MobileInputExtension will capture touches on the entire screen

#### Option B: Specific Touch Area
1. Create a UI element for touch capture:
   ```
   GameObject > UI > Panel (or Raw Image)
   Name: "TouchArea"
   ```
2. Configure the TouchArea:
   - Set anchors to cover desired area (e.g., stretch to full screen)
   - Set Image component's Color Alpha to 0 (transparent)
   - **Important**: Enable "Raycast Target" checkbox
3. Assign to UnityVerseBridgeManager:
   - Drag TouchArea to `Mobile Touch Area` field

### 3. Debug Touch Input

Enable debug mode to see what's happening:

1. **On UnityVerseBridge GameObject**:
   - UnityVerseBridgeManager: `Enable Debug Logging` = ✓
   - UnityVerseBridgeManager: `Show Debug UI` = ✓

2. **On MobileInputExtension component** (if manually added):
   - `Debug Mode` = ✓
   - `Show Touch Visualizer` = ✓

3. **Check Console Logs for**:
   ```
   [MobileInputExtension] Sending touch...
   [WebRtcManager] Data channel state: Open
   [WebRtcManager] Sending data channel message...
   ```

### 4. Verify WebRTC Connection

Touch input only works when WebRTC connection is established:

1. **Check connection status**:
   - Look for "Connected" status in debug UI
   - Console should show: `[WebRtcManager] Peer connection established`

2. **Verify data channel**:
   - Console should show: `[WebRtcManager] Data channel opened`
   - If not, the connection isn't fully established

### 5. Manual Component Setup (If Auto-Setup Fails)

If MobileInputExtension isn't automatically added:

1. Select `UnityVerseBridge` GameObject
2. Add Component > UnityVerseBridge > Core > Extensions > Mobile > `MobileInputExtension`
3. Configure:
   ```
   Send Interval: 0.016 (60 FPS)
   Enable Multi Touch: ✓
   Max Touch Count: 10
   Touch Area: [Your TouchArea or None]
   Normalize To Touch Area: ✓ (if using touch area)
   Debug Mode: ✓
   Show Touch Visualizer: ✓
   ```

### 6. Testing Touch Input

1. **Simple Test Setup**:
   - Start Quest app first (creates room)
   - Start Mobile app (joins room)
   - Wait for "Connected" status
   - Touch the mobile screen

2. **What to Expect**:
   - Mobile: Touch visualizers appear (if enabled)
   - Mobile: Console shows "Sending touch..." messages
   - Quest: Objects with VRClickHandler respond
   - Quest: Touch visualizers appear (if QuestTouchExtension is configured)

### 7. Platform-Specific Issues

#### iOS
- Ensure Info.plist has proper permissions
- Check that Input System Package is installed
- Verify Enhanced Touch Support is available

#### Android
- Check AndroidManifest.xml for INTERNET permission
- Ensure minimum API level is 26+
- Verify touch input works in other Unity apps

### 8. Code Verification

The touch sending flow should be:
1. `MobileInputExtension.ProcessTouches()` captures Unity touch events
2. Normalizes coordinates to 0-1 range
3. Creates `TouchData` message
4. Sends via `webRtcManager.SendDataChannelMessage(touchData)`

### 9. Common Mistakes

1. **Touch Area Behind Other UI**: Ensure TouchArea is in front in hierarchy
2. **Raycast Target Disabled**: TouchArea must have Raycast Target enabled
3. **Wrong Mode**: Mobile app must be in Client mode
4. **Connection Not Ready**: Wait for full WebRTC connection before testing
5. **Missing Enhanced Touch**: Input System package must be installed

### 10. Advanced Debugging

Add this script to verify touch detection:

```csharp
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class TouchDebugger : MonoBehaviour
{
    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }
    
    void Update()
    {
        foreach (var touch in Touch.activeTouches)
        {
            Debug.Log($"Touch detected at: {touch.screenPosition}");
        }
    }
}
```

If this shows touches but MobileInputExtension doesn't send them, the issue is with WebRTC connection or component setup.