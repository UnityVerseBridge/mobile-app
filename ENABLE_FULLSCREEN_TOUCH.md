# Enable Full-Screen Touch Input (Option 1)

## Overview
This guide explains how to enable full-screen touch input in the mobile app, which is the simplest way to capture touch input and send it to the Quest app.

## Current Status
✅ The `mobileTouchArea` field in UnityVerseBridgeManager is already set to null, which enables full-screen touch capture.

## Verification Steps

### 1. Verify Current Configuration
In Unity Editor:
1. Open `Assets/Scenes/SampleScene.unity`
2. Select `UnityVerseBridge_Mobile` GameObject
3. Check UnityVerseBridgeManager component:
   - `Mobile Touch Area`: Should be **None (RectTransform)**
   - `Enable Auto Connect`: Should be **checked**

### 2. Enable Debug Mode
To see what's happening with touch input:
1. On `UnityVerseBridge_Mobile` GameObject:
   - Set `Show Debug UI` = ✓
   - Set `Enable Debug Logging` = ✓ (in UnityVerseConfig)

### 3. Add Touch Input Debugger (Optional)
For detailed touch debugging:
1. Select `UnityVerseBridge_Mobile` GameObject
2. Add Component → Scripts → UnityVerseBridge.MobileApp.Setup → `TouchInputDebugger`
3. Enable all debug options

## Testing Touch Input

### Test Procedure:
1. **Start Quest App First**
   - Ensure it shows "Waiting for peer..."
   - Note the room ID

2. **Start Mobile App**
   - Should auto-connect if room IDs match
   - Wait for "Connected" status

3. **Test Touch**
   - Touch anywhere on the screen
   - You should see in console:
     ```
     [MobileInputExtension] Sending touch...
     [TouchInputDebugger] Touch detected...
     ```

### What Should Happen:
- **Mobile App**: Captures touches on entire screen
- **Data Channel**: Sends normalized coordinates (0-1)
- **Quest App**: Receives touch and performs raycast

## Troubleshooting

### Touch Not Working Checklist:

1. **Check WebRTC Connection**:
   ```
   Console should show:
   - [WebRtcManager] Peer connection established
   - [WebRtcManager] Data channel opened
   ```

2. **Check MobileInputExtension**:
   ```
   Console should show:
   - [MobileInputExtension] Initialized
   - [MobileInputExtension] Sending touch...
   ```

3. **Common Issues**:
   - **No touches detected**: Check if Enhanced Touch Support is enabled
   - **Touches detected but not sent**: Data channel not open
   - **Sent but not received in Quest**: Check Quest touch receiver setup

### Quick Fix Script
If MobileInputExtension is not working, add this temporary script to test:

```csharp
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityVerseBridge.Core;

public class QuickTouchTest : MonoBehaviour
{
    private UnityVerseBridgeManager bridge;
    private WebRtcManager webRtc;
    
    void Start()
    {
        EnhancedTouchSupport.Enable();
        bridge = FindFirstObjectByType<UnityVerseBridgeManager>();
        if (bridge != null) webRtc = bridge.WebRtcManager;
    }
    
    void Update()
    {
        if (webRtc != null && webRtc.IsWebRtcConnected)
        {
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    Debug.Log($"Touch at: {touch.screenPosition}");
                    // Manual touch data sending can be added here
                }
            }
        }
    }
}
```

## Expected Behavior with Full-Screen Touch

1. **Any touch on screen** is captured (not just on UI elements)
2. **Coordinates are normalized** to 0-1 range:
   - X: 0 (left) to 1 (right)
   - Y: 0 (bottom) to 1 (top)
3. **Multi-touch supported** (up to 10 simultaneous touches)
4. **Touch phases** tracked: Began, Moved, Ended, Canceled

## Next Steps

If touch input is still not working after verification:
1. Check the Quest app has proper touch receiver (see `FIX_TOUCH_HANDLING.md`)
2. Use TouchInputDebugger to identify where the problem occurs
3. Check firewall/network settings if data channel won't open
4. Ensure both apps use the same room ID and signaling server