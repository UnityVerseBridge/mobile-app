using System;
using System.Collections;
using UnityEngine;
using Unity.WebRTC;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// 모바일 디바이스에서 마이크 오디오를 캡처하여
    /// WebRTC를 통해 Quest VR로 전송하는 컴포넌트입니다.
    /// </summary>
    public class MobileAudioSender : MonoBehaviour
    {
        [Header("WebRTC Manager")]
        [SerializeField] private WebRtcManager webRtcManager;

        [Header("Audio Settings")]
        [Tooltip("사용할 마이크 장치 이름입니다. 비어있으면 기본 마이크를 사용합니다.")]
        [SerializeField] private string microphoneDeviceName = "";
        
        [Header("Audio Quality")]
        [Tooltip("오디오 샘플링 레이트입니다.")]
        [SerializeField] private int sampleRate = 48000;
        
        [Tooltip("오디오 채널 수입니다. (1: 모노, 2: 스테레오)")]
        [SerializeField] private int channels = 1; // 모바일은 주로 모노 사용

        [Header("Permissions")]
        [Tooltip("앱 시작 시 자동으로 마이크 권한을 요청할지 여부입니다.")]
        [SerializeField] private bool autoRequestPermission = true;

        private AudioStreamTrack audioStreamTrack;
        private AudioSource audioSource;
        private bool isStreaming = false;
        private bool hasPermission = false;

        void Awake()
        {
            if (webRtcManager == null)
            {
                webRtcManager = FindObjectOfType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileAudioSender] WebRtcManager not found!");
                    enabled = false;
                }
            }
        }

        void Start()
        {
            if (autoRequestPermission)
            {
                StartCoroutine(RequestMicrophonePermission());
            }
        }

        void OnEnable()
        {
            if (webRtcManager != null)
            {
                webRtcManager.OnWebRtcConnected += OnWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected += StopAudioStreaming;
            }
        }

        void OnDisable()
        {
            if (webRtcManager != null)
            {
                webRtcManager.OnWebRtcConnected -= OnWebRtcConnected;
                webRtcManager.OnWebRtcDisconnected -= StopAudioStreaming;
            }
            StopAudioStreaming();
        }

        private IEnumerator RequestMicrophonePermission()
        {
            #if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                yield return new WaitForSeconds(0.5f); // 권한 요청 대기
                
                // 권한 재확인
                hasPermission = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
            }
            else
            {
                hasPermission = true;
            }
            #elif UNITY_IOS
            // iOS는 첫 마이크 사용 시 자동으로 권한 요청
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            hasPermission = Application.HasUserAuthorization(UserAuthorization.Microphone);
            #else
            hasPermission = true; // 에디터나 다른 플랫폼에서는 항상 true
            #endif

            if (!hasPermission)
            {
                Debug.LogError("[MobileAudioSender] Microphone permission denied!");
            }
            else
            {
                Debug.Log("[MobileAudioSender] Microphone permission granted!");
                
                // WebRTC가 이미 연결되어 있다면 오디오 스트리밍 시작
                if (webRtcManager != null && webRtcManager.IsWebRtcConnected)
                {
                    StartAudioStreaming();
                }
            }
        }

        private void OnWebRtcConnected()
        {
            if (hasPermission)
            {
                StartAudioStreaming();
            }
            else
            {
                Debug.LogWarning("[MobileAudioSender] Cannot start audio streaming: No microphone permission");
                StartCoroutine(RequestMicrophonePermission());
            }
        }

        private void StartAudioStreaming()
        {
            if (isStreaming)
            {
                Debug.LogWarning("[MobileAudioSender] Audio streaming already started.");
                return;
            }

            if (!hasPermission)
            {
                Debug.LogError("[MobileAudioSender] Cannot start audio streaming without microphone permission!");
                return;
            }

            try
            {
                SetupMicrophoneCapture();

                if (audioSource != null)
                {
                    // AudioStreamTrack 생성
                    audioStreamTrack = new AudioStreamTrack(audioSource);
                    
                    // WebRTC에 오디오 트랙 추가
                    webRtcManager.AddAudioTrack(audioStreamTrack);
                    
                    isStreaming = true;
                    Debug.Log($"[MobileAudioSender] Audio streaming started from microphone: {microphoneDeviceName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[MobileAudioSender] Failed to start audio streaming: {e.Message}");
            }
        }

        private void SetupMicrophoneCapture()
        {
            // 사용 가능한 마이크 확인
            string[] devices = Microphone.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("[MobileAudioSender] No microphone devices found!");
                return;
            }

            // 마이크 선택
            if (string.IsNullOrEmpty(microphoneDeviceName) || System.Array.IndexOf(devices, microphoneDeviceName) == -1)
            {
                microphoneDeviceName = devices[0];
                Debug.Log($"[MobileAudioSender] Using microphone: {microphoneDeviceName}");
            }

            // AudioSource 컴포넌트 생성 또는 가져오기
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // 마이크 녹음 시작
            audioSource.clip = Microphone.Start(microphoneDeviceName, true, 1, sampleRate);
            audioSource.loop = true;

            // 마이크가 준비될 때까지 대기
            while (!(Microphone.GetPosition(microphoneDeviceName) > 0)) { }

            audioSource.Play();
            
            // 로컬 피드백 방지 (자기 목소리가 들리지 않도록)
            audioSource.mute = true;
            audioSource.volume = 0f;
        }

        private void StopAudioStreaming()
        {
            if (!isStreaming)
            {
                return;
            }

            try
            {
                // 마이크 정지
                if (!string.IsNullOrEmpty(microphoneDeviceName))
                {
                    Microphone.End(microphoneDeviceName);
                }

                // AudioSource 정지
                if (audioSource != null)
                {
                    audioSource.Stop();
                    if (audioSource.clip != null)
                    {
                        Destroy(audioSource.clip);
                        audioSource.clip = null;
                    }
                }

                // WebRTC에서 트랙 제거
                if (audioStreamTrack != null && webRtcManager != null)
                {
                    webRtcManager.RemoveTrack(audioStreamTrack);
                    audioStreamTrack.Dispose();
                    audioStreamTrack = null;
                }

                isStreaming = false;
                Debug.Log("[MobileAudioSender] Audio streaming stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MobileAudioSender] Error stopping audio streaming: {e.Message}");
            }
        }

        void OnDestroy()
        {
            StopAudioStreaming();
        }

        // Inspector에서 설정 변경 시 유효성 검사
        void OnValidate()
        {
            sampleRate = Mathf.Clamp(sampleRate, 8000, 48000);
            channels = Mathf.Clamp(channels, 1, 2);
        }

        // 디버그 및 제어 메서드
        public void RequestPermission()
        {
            StartCoroutine(RequestMicrophonePermission());
        }

        public void ToggleMute(bool mute)
        {
            if (audioSource != null)
            {
                audioSource.mute = mute;
                Debug.Log($"[MobileAudioSender] Microphone mute: {mute}");
            }
        }

        public void ChangeMicrophone(string deviceName)
        {
            if (isStreaming)
            {
                StopAudioStreaming();
                microphoneDeviceName = deviceName;
                StartAudioStreaming();
            }
            else
            {
                microphoneDeviceName = deviceName;
            }
        }

        // 상태 프로퍼티
        public bool IsStreaming => isStreaming;
        public bool HasMicrophonePermission => hasPermission;
        public string CurrentMicrophone => microphoneDeviceName;
        public string[] AvailableMicrophones => Microphone.devices;
    }
}