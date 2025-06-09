using UnityEngine;
using UnityVerseBridge.Core;
using UnityVerseBridge.Core.DataChannel.Data;
using System;
using System.Collections;
#if UNITY_ANDROID
using UnityEngine.Android;
#elif UNITY_IOS
using UnityEngine.iOS;
#endif

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Quest 앱으로부터 햅틱 데이터를 수신하여 모바일 진동으로 변환합니다.
    /// Android VibrationEffect API와 iOS Core Haptics를 지원합니다.
    /// </summary>
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

        // 진동 패턴 변환을 위한 헬퍼 클래스
        private class HapticPattern
        {
            public long[] timings;
            public int[] amplitudes;
            
            public HapticPattern(long[] timings, int[] amplitudes)
            {
                this.timings = timings;
                this.amplitudes = amplitudes;
            }
        }
        
        // Quest 햅틱 강도를 모바일 진동 강도로 매핑
        private int ConvertToMobileAmplitude(float questIntensity)
        {
            // Quest: 0.0-1.0 → Mobile: 0-255
            return Mathf.RoundToInt(questIntensity * 255f);
        }
        
        private void ProcessHapticCommand(HapticCommand command)
        {
            Debug.Log($"[MobileHapticReceiver] Processing Haptic Command: {command.commandType}, Duration: {command.duration}");
            
            #if UNITY_ANDROID && !UNITY_EDITOR
            ProcessAndroidHaptic(command);
            #elif UNITY_IOS && !UNITY_EDITOR
            ProcessiOSHaptic(command);
            #else
            // 에디터나 지원하지 않는 플랫폼에서는 기본 진동
            if (command.commandType != HapticCommandType.PlaySound)
            {
                Handheld.Vibrate();
            }
            #endif
        }
        
        #if UNITY_ANDROID
        private void ProcessAndroidHaptic(HapticCommand command)
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null) return;
                
                // Android API 26+ (Oreo) VibrationEffect 지원
                int sdkInt = 0;
                using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdkInt = versionClass.GetStatic<int>("SDK_INT");
                }
                
                if (sdkInt >= 26)
                {
                    using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        AndroidJavaObject vibrationEffect = null;
                        
                        switch (command.commandType)
                        {
                            case HapticCommandType.VibrateDefault:
                                vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                    "createOneShot", 100L, 128); // 100ms, 중간 강도
                                break;
                                
                            case HapticCommandType.VibrateShort:
                                vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                    "createOneShot", 50L, 255); // 50ms, 최대 강도
                                break;
                                
                            case HapticCommandType.VibrateLong:
                                vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                    "createOneShot", (long)(command.duration * 1000f), 200); // duration 사용
                                break;
                                
                            case HapticCommandType.VibrateCustom:
                                // 커스텀 패턴 생성 (예: 진동-멈춤-진동)
                                long[] timings = new long[] { 0, 100, 50, 100 };
                                int[] amplitudes = new int[] { 0, 255, 0, 128 };
                                vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                                    "createWaveform", timings, amplitudes, -1); // -1 = no repeat
                                break;
                        }
                        
                        if (vibrationEffect != null)
                        {
                            vibrator.Call("vibrate", vibrationEffect);
                        }
                    }
                }
                else
                {
                    // 구형 Android API
                    long duration = command.commandType == HapticCommandType.VibrateShort ? 50L : 
                                   command.commandType == HapticCommandType.VibrateLong ? (long)(command.duration * 1000f) : 100L;
                    vibrator.Call("vibrate", duration);
                }
            }
        }
        #endif
        
        #if UNITY_IOS
        private void ProcessiOSHaptic(HapticCommand command)
        {
            // iOS Haptic Feedback API 사용
            switch (command.commandType)
            {
                case HapticCommandType.VibrateDefault:
                    // Medium impact
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
                    break;
                    
                case HapticCommandType.VibrateShort:
                    // Light impact
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
                    break;
                    
                case HapticCommandType.VibrateLong:
                    // Heavy impact 또는 연속 진동
                    StartCoroutine(ContinuousHapticForiOS(command.duration));
                    break;
                    
                case HapticCommandType.VibrateCustom:
                    // Selection change (가벼운 틱 진동)
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.SelectionChange);
                    break;
            }
        }
        
        private IEnumerator ContinuousHapticForiOS(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
                yield return new WaitForSeconds(0.1f); // 100ms 간격
                elapsed += 0.1f;
            }
        }
        #endif
        
        // iOS Haptic Feedback 헬퍼 클래스 (별도 구현 필요)
        #if UNITY_IOS
        public class iOSHapticFeedback
        {
            public enum iOSFeedbackType
            {
                SelectionChange,
                ImpactLight,
                ImpactMedium,
                ImpactHeavy,
                Success,
                Warning,
                Error
            }
            
            private static iOSHapticFeedback _instance;
            public static iOSHapticFeedback Instance
            {
                get
                {
                    if (_instance == null)
                        _instance = new iOSHapticFeedback();
                    return _instance;
                }
            }
            
            public void Trigger(iOSFeedbackType type)
            {
                // iOS Native Plugin 호출 또는 Unity iOS Haptic API 사용
                // 간단한 구현을 위해 Handheld.Vibrate() 사용
                Handheld.Vibrate();
            }
        }
        #endif
    }
}