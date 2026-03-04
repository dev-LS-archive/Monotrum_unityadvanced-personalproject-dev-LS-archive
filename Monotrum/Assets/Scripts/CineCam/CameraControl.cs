using System;
using UnityEngine;
using Unity.Cinemachine;
using Audio;
using Player;

namespace CineCam
{
    [RequireComponent(typeof(CinemachineCamera))]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraControl : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private AudioAnalyzer _audioAnalyzer;
        
        [Header("Lens Settings (Physical Camera)")]
        [Tooltip("정지 시 렌즈 (망원)")]
        [SerializeField] private float _idleFocalLength = 50f;
        [Tooltip("최고 속도 시 렌즈 (광각)")]
        [SerializeField] private float _runFocalLength = 20f;
        [Tooltip("렌즈 전환 부드러움 (값이 클수록 즉각 반응)")]
        [SerializeField] private float _lensLerpSpeed = 5f;
        
        [Header("Impulse Settings")]
        [Tooltip("오디오 펄스 임계값 (이 값을 넘는 강한 비트에서만 진동)")]
        [SerializeField] private float _beatThreshold = 0.8f;
        [Tooltip("임펄스 강도 배수")]
        [SerializeField] private float _impulseMultiplier = 0.1f;
        
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        private float _currentFocalLength;
        
        // 센서 높이
        private float _sensorHeight;
        // Impulse 쿨타임 체크
        private float _shakeCooldown = 0f;
        // 이전 프레임의 오디오 펄스 값
        private float _previousPulse = 0f;

        private void Awake()
        {
            _sensorHeight = _cinemachineCamera.Lens.PhysicalProperties.SensorSize.y;
        }

        private void Update()
        {
            UpdateFocalLength();
            HandleAudioImpulse();
        }

        private void UpdateFocalLength()
        {
            if (_playerController == null) return;
            
            // SpeedRatio(0:정지 ~ 1:최고속도)에 따라 목표 렌즈 값(Focal Length) 계산
            float targetFocal = Mathf.Lerp(_idleFocalLength, _runFocalLength, _playerController.SpeedRatio);
            // 렌즈 화각의 부드러운 전환 (Damping 효과)
            _currentFocalLength = Mathf.Lerp(_currentFocalLength, targetFocal, Time.deltaTime * _lensLerpSpeed);

            // focal length(mm) → vertical FOV(degrees)
            float fov = 2f * Mathf.Atan(_sensorHeight / (2f * _currentFocalLength)) * Mathf.Rad2Deg;

            var lens = _cinemachineCamera.Lens;
            lens.FieldOfView = fov;
            _cinemachineCamera.Lens = lens;
        }
        
        private void HandleAudioImpulse()
        {
            if (_audioAnalyzer == null || _audioAnalyzer.BandBuffer == null) return;
            
            _shakeCooldown -= Time.deltaTime;
            
            // 킥/베이스 대역(0번 밴드) 데이터 추출
            float currentPulse = _audioAnalyzer.BandBuffer[0];
            
            // 변화량(Delta) 계산: 이전 프레임 대비 얼마나 값이 확 뛰었는지.
            float pulseDelta = currentPulse - _previousPulse;
            
            // 플레이어가 거의 멈춰있다면 진동 로직을 무시하고 리턴
            if (_playerController != null && _playerController.SpeedRatio < 0.05f)
            {
                _previousPulse = currentPulse; // 다음 프레임 계산을 위해 값은 저장해둠
                return;
            }

            // 설정한 임계값을 넘고 쿨타임이 돌았을 때만 단타 임펄스 발생
            // 절대값(currentPulse)이 아닌, 확 튀어오르는 폭(pulseDelta)이 임계값을 넘을 때만 Impulse
            if (pulseDelta > _beatThreshold && _shakeCooldown <= 0f)
            {
                // 인스펙터에서 세팅한 Source를 통해 진동 발생
                if (_impulseSource != null)
                {
                    // 흔들림 최종 강도 = 오디오 절대 크기 * 배수 * 현재 달리는 속도비율
                    // (빠르게 달릴수록 화면이 더 세게 흔들림)
                    float finalForce = currentPulse * _impulseMultiplier * _playerController.SpeedRatio;
                    
                    _impulseSource.GenerateImpulseWithForce(finalForce);
                }
                // 비트가 연속으로 터지는 것을 막기 위한 쿨타임
                _shakeCooldown = 0.25f;
            }
            // 다음 프레임 비교를 위해 현재 값을 저장
            _previousPulse = currentPulse;
        }
    }
}
