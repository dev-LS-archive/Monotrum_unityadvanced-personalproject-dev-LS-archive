using System;
using System.Collections;
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
        private const string LOWPASS_PARAM = "LowpassCutoff";
        private const string REVERB_PARAM = "ReverbRoom";
        
        [SerializeField] private AudioMixerSnapshot _normalSnapshot;
        [SerializeField] private AudioMixerSnapshot _tapeStopSnapshot;

        public TrackData SelectedTrack;
        // 믹서 잠금용 플래그
        private bool _isMixerLocked = false;
        
        // 현재 돌고 있는 코루틴을 추적하기 위한 변수
        private Coroutine _clearMixerCoroutine;
        
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
        
        public float TotalTime
        {
            get
            {
                if (SelectedTrack == null) return 0f;
                return SelectedTrack.clip.length;
            }
        }

        public float CurrentTime
        {
            get
            {
                // _audioSource.clip 자체가 비어있으면 0 반환
                if (_audioSource == null || SelectedTrack == null || _audioSource.clip == null) 
                    return 0f;
                return _audioSource.time;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            // 중복 인스턴스면 초기화 건너뛰기
            if (Instance != this) return;
            
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
        public void PlayTrack(TrackData track, bool loop = false)
        {
            // 새 곡이 시작될 때 믹서 잠금을 해제하여 다시 Tape Stop이 먹히게 함
            _isMixerLocked = false;
            if (track.clip != null)
            {
                _audioSource.clip = track.clip;
                _audioSource.loop = loop;
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
        
        public void SelectTrack(TrackData track)
        {
            SelectedTrack = track;
            PlayTrack(track);
        }

        #endregion
        
        #region Audio Control
        public void PauseMusic()
        {
            if (_audioSource.isPlaying)
                _audioSource.Pause();
        }

        public void ResumeMusic()
        {
            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }

        public void StopMusic()
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
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
            // 잠겨있다면 다른 스크립트에서 접근해도 무시한다.
            // AudioMixer pitch 범위는 0에 가까우면 음이 늘어지는 효과
            // 0으로 가면 완전 정지 (테이프 스톱)
            if (_audioMixer == null || _isMixerLocked) return;
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
        
        /// <summary>
        /// 완전한 Mixer 초기화 (씬 전환 전 호출)
        /// 모든 파라미터를 기본값으로 설정하고 Snapshot을 Normal로 전환
        /// </summary>
        public void CompleteMixerReset(float transitionTime = 0.5f)
        {
            if (_audioMixer == null)
            {
                Logger.Instance.LogError("AudioMixer가 null입니다. CompleteMixerReset 실패");
                return;
            }
            
            // 초기화가 시작되는 순간 믹서를 잠가서 외부(PlayerController 등)의 간섭을 원천 차단
            _isMixerLocked = true;
            
            try
            {
                // 1. 모든 파라미터를 기본값으로 설정
                _audioMixer.SetFloat(PITCH_PARAM, 1f);
                _audioMixer.SetFloat(LOWPASS_PARAM, 22000f);  // 최대값 (필터 해제)
                _audioMixer.SetFloat(REVERB_PARAM, -10000f);   // 최소값 (비활성화)

                // 2. Normal Snapshot으로 즉시 전환
                if (_normalSnapshot != null)
                {
                    _normalSnapshot.TransitionTo(transitionTime);
                }

                // 3. 기존에 돌고 있던 초기화 코루틴이 있다면 강제 종료 (빠른 씬 전환 시 꼬임 방지)
                if (_clearMixerCoroutine != null)
                {
                    StopCoroutine(_clearMixerCoroutine);
                }
                
                // 4. 새 코루틴 시작 및 추적
                _clearMixerCoroutine = StartCoroutine(ClearMixerRoutine(transitionTime));

                Logger.Instance.LogInfo($"Complete Mixer Reset Done (TransitionTime: {transitionTime}s)");
            }
            catch (System.Exception ex)
            {
                Logger.Instance.LogError($"CompleteMixerReset 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// CompleteMixerReset() 후 Float을 완전히 제거하는 코루틴
        /// </summary>
        private IEnumerator ClearMixerRoutine(float delay)
        {
            // delay가 0보다 클 때만 대기 (0초면 즉시 실행)
            if (delay > 0f)
            {
                // Time.timeScale = 0 상황에서도 멈추지 않도록 무조건 '현실 시간(Realtime)' 기준 대기!
                yield return new WaitForSecondsRealtime(delay);
            }

            if (_audioMixer == null) yield break;

            try
            {
                _audioMixer.ClearFloat(PITCH_PARAM);
                _audioMixer.ClearFloat(LOWPASS_PARAM);
                _audioMixer.ClearFloat(REVERB_PARAM);
                
                Logger.Instance.LogInfo("Mixer Float parameters cleared successfully");
            }
            catch (System.Exception ex)
            {
                Logger.Instance.LogError($"ClearMixerRoutine 중 오류: {ex.Message}");
            }
        }
    }
}