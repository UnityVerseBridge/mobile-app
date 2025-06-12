# UnityVerse Mobile App

iOS/Android app that receives VR stream and sends touch input.

## Quick Setup

1. **Open in Unity Hub**
   - Unity version: 6 LTS (6000.0.33f1) or 2022.3 LTS
   - Open folder: `mobile-app/UnityProject`

2. **Build Settings**
   - iOS: File → Build Settings → iOS → Switch Platform
   - Android: File → Build Settings → Android → Switch Platform

3. **Connection Setup**
   - Open scene: `Assets/Scenes/MobileScene`
   - Select `UnityVerseBridge` GameObject
   - In Inspector, update `MobileConnectionConfig`:
     - Signaling Server URL: Your server IP
     - Room ID: Same as Quest app
     - Require Authentication: ✓
     - Auth Key: Same as server

4. **Build & Run**
   - iOS: Build → Open in Xcode → Run
   - Android: Build And Run with device connected

## Testing in Editor

1. Make sure Quest app is running first
2. Enter same room ID
3. Click Play - will connect automatically

## Features

- Live VR video streaming
- Touch input to VR
- Auto-reconnection
- Portrait/Landscape support