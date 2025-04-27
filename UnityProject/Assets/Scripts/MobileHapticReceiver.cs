// MobileHapticReceiver.cs 예시
using UnityEngine;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.DataChannel.Data;
using System;

public class MobileHapticReceiver : MonoBehaviour
{
    [SerializeField] private WebRtcManager webRtcManager;

    void Start()
    {
        if (webRtcManager != null)
            webRtcManager.OnDataChannelMessageReceived += HandleDataChannelMessageReceived;
        else
            Debug.LogError("WebRtcManager not assigned!");
    }

    void OnDestroy()
    {
        if (webRtcManager != null)
            webRtcManager.OnDataChannelMessageReceived -= HandleDataChannelMessageReceived;
    }

    private void HandleDataChannelMessageReceived(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData)) return;
        try
        {
            DataChannelMessageBase baseMsg = JsonUtility.FromJson<DataChannelMessageBase>(jsonData);
            if (baseMsg?.type == "haptic")
            {
                HapticCommand command = JsonUtility.FromJson<HapticCommand>(jsonData);
                if (command != null)
                {
                    ProcessHapticCommand(command);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[MobileHapticReceiver] Failed to parse JSON: '{jsonData}' | Error: {e.Message}");
        }
    }

    private void ProcessHapticCommand(HapticCommand command)
    {
        Debug.Log($"[MobileHapticReceiver] Processing Haptic Command: {command.commandType}");
        switch (command.commandType)
        {
            case HapticCommandType.VibrateDefault:
            case HapticCommandType.VibrateShort: // 길이를 구분하려면 플랫폼별 API 필요
            case HapticCommandType.VibrateLong:  // Handheld.Vibrate는 기본 진동만 지원
            case HapticCommandType.VibrateCustom: // Handheld.Vibrate는 강도/시간 제어 어려움
                Handheld.Vibrate(); // 가장 기본적인 진동 실행
                break;
            case HapticCommandType.PlaySound:
                // TODO: 사운드 재생 로직 구현 (command.soundName 사용)
                Debug.Log($"TODO: Play sound '{command.soundName}'");
                break;
        }
    }
}