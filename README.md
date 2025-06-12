# UnityVerse Mobile App

iOS/Android application demonstrating UnityVerseBridge Core package usage for receiving VR streams and sending touch input.

## Overview

This is a sample mobile application that showcases:
- Receiving and displaying VR camera streams
- Sending touch input to VR devices
- Room discovery and connection UI
- Cross-platform mobile support

## Requirements

- Unity 6 LTS (6000.0.33f1) or Unity 2022.3 LTS
- UnityVerseBridge Core package
- iOS Build Support (for iOS)
- Android Build Support (for Android)
- Minimum iOS 12.0 / Android API 26

## Project Setup

1. Clone this repository
2. Open `UnityProject` folder in Unity
3. Import required packages:
   - UnityVerseBridge Core
   - Unity WebRTC

## Configuration

1. Open the sample scene: `Assets/Scenes/MobileStreamingDemo.unity`
2. Find `UnityVerseBridge_Mobile` GameObject
3. Configure the UnityVerseConfig:
   - Signaling URL: Your signaling server address
   - Room ID: Same as Quest app
   - Auto Connect: Enable for automatic connection

## Building for Mobile

### iOS Build
1. File > Build Settings > iOS
2. Player Settings:
   - Minimum iOS Version: 12.0
   - Camera Usage Description: Required for future AR features
3. Build and open in Xcode
4. Sign and deploy to device

### Android Build
1. File > Build Settings > Android
2. Player Settings:
   - Minimum API Level: 26
   - Target API Level: 33+
   - Internet Access: Required
3. Build and Run

## Features Demonstrated

### Room UI Adapter
- `MobileRoomUIAdapter.cs`: Demonstrates room selection and connection
- Integrates with core UI components
- Supports QR code scanning (placeholder)

### Video Debugger
- `MobileVideoDebugger.cs`: Debugging tool for video streaming issues
- Tests different rendering methods
- Provides detailed logging
- Monitors decoder initialization and texture updates

### Menu Controller
- `MobileMenuController.cs`: In-app menu system
- Connection status display
- Debug mode toggle

## UI Components

The mobile app uses core UI components:
- **RoomListUI**: Dynamic room discovery
- **RoomInputUI**: Manual room entry
- **UIManager**: Centralized UI management

## Testing

1. Start the signaling server
2. Run Quest app first (creates room)
3. Run Mobile app
4. Enter same room ID or select from list
5. Touch the screen to send input to VR

## Troubleshooting

### Video Not Displaying
1. Check both devices are in same room
2. Verify signaling server connection
3. Enable debug mode for detailed logs

### Touch Not Working
1. Ensure data channel is open
2. Check touch area configuration
3. Verify VR app is receiving input

### Connection Issues
1. Check network connectivity
2. Verify firewall settings
3. Ensure signaling server is accessible

## Sample Scripts

- **RoomIdInput.cs**: Simple room connection example
- **MobileMenuController.cs**: UI management example
- **MobileVideoDebugger.cs**: Debugging utilities

## Performance Tips

- Use WiFi for best results
- Close other apps to free resources
- Enable hardware acceleration in Player Settings
- Default video quality: Low (640x360) for optimal performance
- Supported resolutions: 360p, 720p, 1080p
- H264 codec preferred for hardware decoding

## License

See LICENSE file in the root repository.