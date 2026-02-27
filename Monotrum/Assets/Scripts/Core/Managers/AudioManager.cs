using Audio;
using UnityEngine;
using UnityEngine.Audio;

namespace Core
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Components")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioMixer _audioMixer; // 테이프 스톱, 일시정지 필터용 믹서
        
        public bool IsPlaying => _audioSource.isPlaying;

        protected override void Awake()
        {
            base.Awake();
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        #region Audio Playback
        
        /// <summary>
        /// TrackData(SO)에 담긴 클립으로 음원을 즉시 로드하여 재생
        /// </summary>
        public void PlayTrack(TrackData track)
        {
            if (track.clip != null)
            {
                _audioSource.clip = track.clip;
                _audioSource.Play();
                Logger.Instance.LogInfo($"Track Loaded & Playing: {track.trackName}");
            }
            else
            {
                Logger.Instance.LogError($"AudioClip이 할당되지 않았습니다: {track.trackName}");
            }
        }

        public void StopTrack()
        {
            _audioSource.Stop();
        }

        #endregion
        
        // AudioManager: 원본 데이터만 제공
        public void GetSpectrumData(float[] samples)
        {
            // 512개 파형 쪼개기
            _audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        }
    }
}