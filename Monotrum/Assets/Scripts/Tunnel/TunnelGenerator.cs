using System;
using System.Collections.Generic;
using UnityEngine;
using Audio;
using Beautify.Universal;
using Core;
using Player;

namespace Tunnel
{
    public class TunnelGenerator : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [Header("References")]
        [SerializeField] private TunnelRing _ringPrefab;
        [SerializeField] private Transform _cubePrefab;
        [SerializeField] private Transform _player;
        [SerializeField] private AudioAnalyzer _audioAnalyzer;
        [SerializeField] private TrackData _currentTrack;

        [Header("Ring Layout")]
        [Tooltip("링 하나를 구성하는 큐브 수 (많을수록 원형에 가까움)")]
        [SerializeField] private int _cubesPerRing = 16;
        [Tooltip("터널의 반지름 (캐릭터 높이의 5~8배 권장)")]
        [SerializeField] private float _tunnelRadius = 8f;
        [Tooltip("링 간 Z축 간격 (좁을수록 빈틈 없음)")]
        [SerializeField] private float _ringSpacing = 2f;

        [Header("Cube Scale")]
        [SerializeField] private float _overlapRatio = 1.05f;
        
        [Header("Cube Visualization")]
        [SerializeField] private Material _cubeMaterial;
        [SerializeField] private float _minEmissionIntensity = 0.1f;
        
        [Header("Pool & Spawn")]
        [Tooltip("동시에 활성화될 링 수 (카메라 Far Clip Plane 커버)")]
        [SerializeField] private int _poolSize = 40;
        [Tooltip("플레이어 뒤로 이만큼 벗어나면 회수")]
        [SerializeField] private float _despawnDistance = 10f;

        [Header("Audio Reaction")]
        [SerializeField] private float _scaleMultiplier = 1.5f;

        [Tooltip("Beautify 색수차 최대 수치")]
        [SerializeField, Range(0.005f, 0.1f)] 
        private float _maxChromaticAberration = 0.1f;

        [Tooltip("오디오 비트에 따른 이미션 발광 강도 증폭량"), SerializeField, Range(0, 1)]  
        private float _emissionPulseMultiplier = 0.5f;

        // 링 풀과 큐브 풀을 분리하여 각각 독립적으로 관리
        private ObjectPool<TunnelRing> _ringPool;
        private ObjectPool<Transform> _cubePool;

        // 현재 활성화된 링을 FIFO로 관리 → 가장 오래된 링을 앞에서 꺼내 회수
        private Queue<TunnelRing> _activeRings;

        // 다음 링이 배치될 Z 좌표를 추적
        private float _nextSpawnZ = 0f;
        
        // 플레이어의 입력으로 터널 속도 연동
        private PlayerController _playerController;

        private void Awake()
        {
            // 큐브 풀: 링 수 × 링당 큐브 수만큼 미리 생성
            _cubePool = new ObjectPool<Transform>(_cubePrefab, _poolSize * _cubesPerRing, transform);

            // 링 풀: 빈 링 오브젝트를 미리 생성
            _ringPool = new ObjectPool<TunnelRing>(_ringPrefab, _poolSize, transform);

            _activeRings = new Queue<TunnelRing>();

            if (_audioAnalyzer == null)
                _audioAnalyzer = FindFirstObjectByType<AudioAnalyzer>();
            
            _playerController = _player.GetComponent<PlayerController>();

            _currentTrack = AudioManager.Instance.SelectedTrack;
        }

        private void OnDestroy()
        {
            _cubeMaterial.SetColor(EmissionColor, _currentTrack.themeColor);
        }

        private void Start()
        {
            SpawnInitialRings();
        }

        private void Update()
        {
            RecycleRings();
            UpdateRingVisuals();
            UpdateEmission();
        }

        /// <summary>
        /// 시작 시 poolSize만큼 링을 Z축으로 일렬 배치하여 초기 터널을 구축한다.
        /// </summary>
        private void SpawnInitialRings()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                SpawnRing();
            }
        }

        /// <summary>
        /// 링 하나를 생성한다.
        /// 1) 링 풀에서 링을 꺼낸다
        /// 2) 큐브 풀에서 큐브를 꺼내 배열로 묶는다
        /// 3) 링에 큐브 배열과 반지름을 넘겨 원형 배치를 수행한다
        /// </summary>
        private void SpawnRing()
        {
            TunnelRing ring = _ringPool.Get();
            ring.transform.position = new Vector3(0f, 0f, _nextSpawnZ);

            // 큐브 풀에서 링 하나분의 큐브를 꺼냄
            Transform[] cubes = new Transform[_cubesPerRing];
            for (int i = 0; i < _cubesPerRing; i++)
            {
                cubes[i] = _cubePool.Get();
            }

            // 큐브 스케일을 호 길이와 링 간격에 맞춰 빈틈 방지
            float arcLength = 2f * Mathf.PI * _tunnelRadius / _cubesPerRing;
            for (int i = 0; i < _cubesPerRing; i++)
            {
                // X: 방사 방향(두께), Y: 호 방향, Z: 터널 진행 방향
                // 1.05f 오버랩으로 카메라 각도에 따른 미세한 틈 방지
                cubes[i].localScale = new Vector3(
                    arcLength,
                    arcLength * _overlapRatio,
                    _ringSpacing * _overlapRatio
                );
            }

            ring.Initialize(cubes, _tunnelRadius);
            _activeRings.Enqueue(ring);
            _nextSpawnZ += _ringSpacing;
        }

        /// <summary>
        /// 트레드밀(런닝머신) 방식 무한 스크롤.
        /// 모든 링을 -Z로 이동시키고, 플레이어 뒤로 벗어난 링은
        /// 회수 후 터널 맨 앞에 재배치한다.
        /// 플레이어는 제자리에서 애니메이션만 취하면 된다.
        /// 플레이어가 무한히 앞으로 나아가면 스케일 한계상 지터링 발생 가능성 있음
        /// </summary>
        private void RecycleRings()
        {
            // 전체 링을 TrackData 속도에 맞춰 플레이어 쪽으로 이동 + 플레이어의 이동에 따라 속도 조절(SpeedRatio)
           float moveStep = _currentTrack.trackScrollSpeed * _playerController.SpeedRatio * Time.deltaTime;
            
            foreach (var ring in _activeRings)
            {
                ring.transform.position += Vector3.back * moveStep;
            }
            
            // 링들이 다가온 만큼 다음 스폰 좌표도 함께 당겨온다
            _nextSpawnZ -= moveStep;
            
            // 수정 제안 코드
            while (_activeRings.Count > 0 && _activeRings.Peek().transform.position.z < _player.position.z - _despawnDistance)
            {
                TunnelRing oldestRing = _activeRings.Dequeue();
    
                // 기존 큐브를 링에서 분리하여 풀에 반환
                Transform[] oldCubes = oldestRing.DetachCubes();
                if (oldCubes != null)
                {
                    for (int i = 0; i < oldCubes.Length; i++)
                        _cubePool.Return(oldCubes[i]);
                }

                // 링 자체도 풀에 반환
                _ringPool.Return(oldestRing);

                // 터널 맨 앞에 새 링을 생성
                SpawnRing();
            }
        }

        /// <summary>
        /// 활성 링 전체에 오디오 데이터를 전달하여 비주얼을 갱신한다.
        /// </summary>
        private void UpdateRingVisuals()
        {
            if (_audioAnalyzer == null || _audioAnalyzer.BandBuffer == null) return;

            foreach (var ring in _activeRings)
            {
                ring.UpdateVisual(_audioAnalyzer.BandBuffer, _scaleMultiplier);
            }
        }
        
        // 플레이어 입력 및 오디오 데이터에 따라 이미션 강도 조절
        private void UpdateEmission()
        {
            // 1. 오디오 데이터 가져오기 (예: 밴드 0번 또는 1번이 킥/베이스 담당)
            float audioPulse = 0f;
            if (_audioAnalyzer != null && _audioAnalyzer.BandBuffer != null)
            {
                // 저음부(베이스)의 값을 가져와 증폭
                audioPulse = _audioAnalyzer.BandBuffer[0] * _emissionPulseMultiplier;
                audioPulse = Mathf.Clamp01(audioPulse);
            }

            // 2. 테이프 스톱 비율(SpeedRatio)과 오디오 펄스를 결합
            // SpeedRatio가 0(정지)이면 audioPulse도 무효화되어 서서히 암전됨
            float targetIntensity = 1f + audioPulse;
            float intensity = Mathf.Lerp(_minEmissionIntensity, targetIntensity, _playerController.SpeedRatio);
    
            _cubeMaterial.SetColor(EmissionColor, _currentTrack.themeColor * intensity);

            UpdateAberrationIntensity(audioPulse);
        }
        
        // UpdateEmission()에서 추출한 audioPulse(0~1) 값을 넘겨받아 사용
        public void UpdateAberrationIntensity(float pulseValue)
        {
            // BeautifySettings 싱글톤을 통해 현재 씬의 프로필 값을 즉시 오버라이드
            // pulseValue에 최대 강도를 곱해서 적용
            float finalIntensity = Mathf.Clamp(pulseValue * _maxChromaticAberration, 0f, 0.1f);
            BeautifySettings.settings.chromaticAberrationIntensity.Override(finalIntensity);
        }
    }
}