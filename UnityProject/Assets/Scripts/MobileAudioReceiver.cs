using System;
using UnityEngine;
using Unity.WebRTC;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// 모바일 디바이스에서 Quest VR로부터 오디오를 수신하여 재생하는 컴포넌트입니다.
    /// </summary>
    public class MobileAudioReceiver : MonoBehaviour
    {
        [Header("WebRTC Manager")]
        [SerializeField] private WebRtcManager webRtcManager;

        [Header("Audio Settings")]
        [Tooltip("수신된 오디오를 재생할 AudioSource입니다.")]
        [SerializeField] private AudioSource audioSource;
        
        [Tooltip("오디오 볼륨입니다.")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;
        
        [Tooltip("스피커로 출력할지 이어폰으로 출력할지 선택합니다.")]
        [SerializeField] private bool useSpeaker = true;

        private AudioStreamTrack receivedAudioTrack;
        private bool isReceiving = false;

        void Awake()
        {
            if (webRtcManager == null)
            {
                webRtcManager = FindObjectOfType<WebRtcManager>();
                if (webRtcManager == null)
                {
                    Debug.LogError("[MobileAudioReceiver] WebRtcManager not found!");
                    enabled = false;
                    return;
                }
            }

            // AudioSource 설정
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            SetupAudioSource();
        }

        void Start()
        {
            // 모바일 플랫폼별 오디오 설정
            SetupMobileAudio();
        }

        void OnEnable()
        {
            if (webRtcManager != null)
            {
                webRtcManager.OnAudioTrackReceived += HandleAudioTrackReceived;
                webRtcManager.OnWebRtcDisconnected += StopAudioReceiving;
            }
        }

        void OnDisable()
        {
            if (webRtcManager != null)
            {
                webRtcManager.OnAudioTrackReceived -= HandleAudioTrackReceived;
                webRtcManager.OnWebRtcDisconnected -= StopAudioReceiving;
            }
            StopAudioReceiving();
        }

        private void SetupAudioSource()
        {
            if (audioSource == null) return;

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.spatialBlend = 0f; // 항상 2D 사운드로 설정
        }

        private void SetupMobileAudio()
        {
            #if UNITY_IOS
            // iOS에서 스피커/이어폰 설정
            if (useSpeaker)
            {
                // 스피커로 출력 (핸즈프리)
                UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
            }
            #elif UNITY_ANDROID
            // Android에서는 AudioManager를 통해 제어해야 할 수 있음
            // Unity의 기본 설정으로도 대부분 잘 작동함
            #endif

            // 오디오 세션 카테고리 설정 (백그라운드 재생 등)
            AudioConfiguration config = AudioSettings.GetConfiguration();
            config.speakerMode = AudioSpeakerMode.Stereo;
            AudioSettings.Reset(config);
        }

        private void HandleAudioTrackReceived(AudioStreamTrack audioTrack)
        {
            if (audioTrack == null)
            {
                Debug.LogError("[MobileAudioReceiver] Received null audio track!");
                return;
            }

            Debug.Log($"[MobileAudioReceiver] Audio track received: {audioTrack.Id}");

            try
            {
                // 이전 트랙 정리
                if (receivedAudioTrack != null)
                {
                    StopAudioReceiving();
                }

                receivedAudioTrack = audioTrack;

                // Unity WebRTC에서 AudioStreamTrack을 AudioSource에 연결
                audioSource.SetTrack(audioTrack);
                audioSource.Play();

                isReceiving = true;
                Debug.Log("[MobileAudioReceiver] Started playing received audio");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MobileAudioReceiver] Failed to setup audio playback: {e.Message}");
            }
        }

        private void StopAudioReceiving()
        {
            if (!isReceiving) return;

            try
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.SetTrack(null);
                }

                if (receivedAudioTrack != null)
                {
                    receivedAudioTrack = null;
                }

                isReceiving = false;
                Debug.Log("[MobileAudioReceiver] Stopped audio receiving");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MobileAudioReceiver] Error stopping audio receiving: {e.Message}");
            }
        }

        // 런타임에 볼륨 조절
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        // 스피커/이어폰 전환
        public void SetUseSpeaker(bool speaker)
        {
            useSpeaker = speaker;
            
            #if UNITY_IOS
            // iOS 스피커 모드 변경 구현
            // 실제 구현은 네이티브 플러그인이 필요할 수 있음
            Debug.Log($"[MobileAudioReceiver] Speaker mode: {speaker}");
            #elif UNITY_ANDROID
            // Android 스피커 모드 변경 구현
            Debug.Log($"[MobileAudioReceiver] Speaker mode: {speaker}");
            #endif
        }

        // 오디오 일시정지/재개
        public void PauseAudio(bool pause)
        {
            if (audioSource != null && isReceiving)
            {
                if (pause)
                    audioSource.Pause();
                else
                    audioSource.UnPause();
            }
        }

        void OnDestroy()
        {
            StopAudioReceiving();
        }

        // Inspector에서 값 변경 시
        void OnValidate()
        {
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }

        // 백그라운드/포그라운드 전환 처리
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 백그라운드로 전환 시
                if (isReceiving && audioSource != null)
                {
                    audioSource.Pause();
                }
            }
            else
            {
                // 포그라운드로 복귀 시
                if (isReceiving && audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
            }
        }

        // 디버그 정보
        public bool IsReceiving => isReceiving;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0f;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;
        public bool IsSpeakerMode => useSpeaker;
    }
}