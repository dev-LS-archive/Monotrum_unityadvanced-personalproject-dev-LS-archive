using Audio;
using UnityEngine;

namespace Core
{
    public class TrackManager : MonoBehaviour
    {
        [Header("Track Cubes")]
        [Tooltip("8개의 주파수 대역을 표현할 큐브(비주얼라이저)를 저음부터 고음 순서대로 넣어주세요.")]
        [SerializeField] private Transform[] _trackCubes = new Transform[8];

        [Header("Scale Settings")]
        [SerializeField] private float _scaleMultiplier = 10f; // 음악 소리에 맞춰 튀어 오르는 강도
        [SerializeField] private float _baseHeight = 1f;       // 큐브의 기본 높이
        
        [SerializeField] private AudioAnalyzer _audioAnalyzer;

        private void Update()
        {
            // AudioManager가 살아있고 음악이 재생 중일 때만 비주얼라이징 실행
            if (AudioManager.Instance != null && AudioManager.Instance.IsPlaying)
            {
                VisualizeAudio();
            }
        }

        private void VisualizeAudio()
        {
            // AudioAnalyzer에서 스무딩 처리된 8개의 주파수 배열 가져오기
            float[] audioBands = _audioAnalyzer.BandBuffer;

            for (int i = 0; i < 8; i++)
            {
                if (_trackCubes[i] != null)
                {
                    // 큐브의 원래 X, Z 스케일은 유지하고 Y축(높이)만 음악 데이터에 맞춰 변경
                    Vector3 newScale = _trackCubes[i].localScale;
                    newScale.y = _baseHeight + (audioBands[i] * _scaleMultiplier);
                    
                    _trackCubes[i].localScale = newScale;
                }
            }
        }
    }
}