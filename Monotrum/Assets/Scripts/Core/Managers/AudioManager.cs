using System;
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
        private const string PITCH_PARAM = "MasterPitch";
        
        [SerializeField] private AudioMixerSnapshot _normalSnapshot;
        [SerializeField] private AudioMixerSnapshot _tapeStopSnapshot;
        
        // 오디오 재생 상태가 변할 때 알리는 이벤트
        public event Action<bool> OnPlayStateChanged;
        
        public bool IsPlaying
        {
            get
            {
                if(_audioSource != null) 
                    return _audioSource.isPlaying;
                return false;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (_audioMixer != null)
            {
                _audioSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Master")[0];
            }
            else
            {
                Logger.Instance.LogError("AudioMixer가 할당되지 않았습니다.");
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
                
                OnPlayStateChanged?.Invoke(true);
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
            
            OnPlayStateChanged?.Invoke(false);
        }

        #endregion
        
        // AudioManager: 원본 데이터만 제공
        public void GetSpectrumData(float[] samples)
        {
            // 512개 파형 쪼개기
            _audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
        }
        
        public void SetPitch(float ratio)
        {
            // AudioMixer pitch 범위는 0에 가까우면 음이 늘어지는 효과
            // 0으로 가면 완전 정지 (테이프 스톱)
            if (_audioMixer == null) return;
            _audioMixer.SetFloat(PITCH_PARAM, Mathf.Clamp(ratio, 0.05f, 1f));
            
            // 완전 정속이면 Normal 스냅샷으로 즉시 전환 (필터 완전 해제)
            if (ratio >= 1f)
            {
                _normalSnapshot.TransitionTo(0f);
                return;
            }
    
            // 스냅샷 보간: ratio 1이면 Normal, 0이면 TapeStop
            _audioMixer.TransitionToSnapshots(
                new[] { _normalSnapshot, _tapeStopSnapshot },
                new[] { ratio, 1f - ratio },
                Time.deltaTime
            );
        }
    }
}