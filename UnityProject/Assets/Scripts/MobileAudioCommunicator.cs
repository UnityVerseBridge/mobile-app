using UnityEngine;
using UnityVerseBridge.Core;

namespace UnityVerseBridge.MobileApp
{
    /// <summary>
    /// Mobile 앱에서 양방향 오디오 통신을 담당하는 클래스
    /// Core의 AudioStreamManager를 Mobile 환경에 맞게 설정합니다.
    /// </summary>
    public class MobileAudioCommunicator : MonoBehaviour
    {
        [SerializeField] private AudioStreamManager audioStreamManager;
        
        void Start()
        {
            if (audioStreamManager == null)
            {
                audioStreamManager = GetComponent<AudioStreamManager>();
                if (audioStreamManager == null)
                {
                    audioStreamManager = gameObject.AddComponent<AudioStreamManager>();
                }
            }
            
            // Mobile 전용 설정 적용
            ConfigureForMobile();
        }
        
        private void ConfigureForMobile()
        {
            // Mobile에서도 기본적으로 마이크와 스피커 모두 활성화 (양방향 통화)
            audioStreamManager.SetMicrophoneEnabled(true);
            audioStreamManager.SetSpeakerEnabled(true);
            
            Debug.Log("[MobileAudioCommunicator] Configured AudioStreamManager for Mobile bidirectional audio");
        }
    }
}
