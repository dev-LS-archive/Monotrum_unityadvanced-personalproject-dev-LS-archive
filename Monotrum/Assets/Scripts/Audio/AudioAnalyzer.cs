using Core;
using UnityEngine;

namespace Audio
{
    public class AudioAnalyzer : MonoBehaviour
    {
        private float[] _samples = new float[512];       // FFT 결과를 담는 배열 (512개 주파수 bin)
        private float[] _freqBands = new float[8];       // 512 bin → 8밴드 압축 원본 (매 프레임 급변)
        private float[] _bandBuffer = new float[8];      // 스무딩된 최종 출력값 (외부 제공용)
        private float[] _bufferDecrease = new float[8];  // 밴드별 현재 감쇠 속도

        public float[] BandBuffer => _bandBuffer;
        
        private bool _isAnalyzing = false;

        private void OnEnable()
        {
            // 매니저가 살아있다면 재생 상태 방송 구독
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnPlayStateChanged += SetAnalyzeState;
                // 이미 재생 중일 때 씬에 들어왔을 경우를 대비한 동기화
                _isAnalyzing = AudioManager.Instance.IsPlaying; 
            }
        }

        private void OnDisable()
        {
            if (Singleton<AudioManager>.IsQuitting) return;
            
            // 씬이 파괴될 때 가비지가 남지 않도록 구독 해제 (메모리 누수 방지)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.OnPlayStateChanged -= SetAnalyzeState;
            }
        }
        
        private void SetAnalyzeState(bool isPlaying)
        {
            _isAnalyzing = isPlaying; // 방송 내용에 따라 스스로 상태 변경
        }
        
        void Update()
        {
            // if (AudioManager.Instance != null && AudioManager.Instance.IsPlaying)
            
            // 매니저한테 매번 물어보는 게 아니라, 자기 변수만 체크
            if (_isAnalyzing) 
            {
                // GetSpectrumData는 _samples를 새로 생성하지 않고 덮어쓴다
                AudioManager.Instance.GetSpectrumData(_samples);
                ComputeFreqBands();
                ApplySmoothing();
            }
        }

        /// <summary>
        /// 512개 FFT bin을 8개 주파수 밴드로 압축한다.
        /// 각 밴드의 bin 수는 2^i * 2로 지수적으로 증가하며,
        /// 저음부는 세밀하게, 고음부는 넓게 묶는다.
        /// (인간의 청각이 저주파 변화에 더 민감하기 때문)
        /// </summary>
        void ComputeFreqBands()
        {
            // count는 리셋 없이 증가하여 512 bin을 순서대로 훑는다
            // 밴드0: bin[0~1], 밴드1: bin[2~5], ... 밴드7: bin[254~511]
            int count = 0;

            for (int i = 0; i < 8; i++)
            {
                float average = 0;

                // 밴드별 담당 bin 수: 2, 4, 8, 16, 32, 64, 128, 258(+2보정) = 총 512
                int sampleCount = (int)Mathf.Pow(2, i) * 2;
                if (i == 7) sampleCount += 2; // 510 → 512 맞추기 위한 보정

                for (int j = 0; j < sampleCount; j++)
                {
                    // (count + 1) 가중치: 고주파 bin의 자연 에너지 감소를 보상
                    average += _samples[count] * (count + 1);
                    count++;
                }

                // count는 누적값이므로 뒤쪽 밴드일수록 분모가 커져 자연스러운 정규화 효과
                average /= count;
                _freqBands[i] = average * 10; // 시각적으로 다루기 편한 스케일로 보정
            }
        }

        /// <summary>
        /// "올라갈 때는 즉시, 내려갈 때는 부드럽게" 스무딩을 적용한다.
        /// 이 처리가 없으면 FFT 원본값이 매 프레임 급변하여 큐브가 떨리듯 보인다.
        /// </summary>
        void ApplySmoothing()
        {
            for (int g = 0; g < 8; g++)
            {
                // 상승: 비트가 들어오는 순간의 반응성을 위해 즉시 반영
                if (_freqBands[g] > _bandBuffer[g])
                {
                    _bandBuffer[g] = _freqBands[g];
                    _bufferDecrease[g] = 0.005f; // 감쇠 속도 초기화
                }

                // 하강: 천천히 내려가되 매 프레임 1.2배씩 가속 → 자연스러운 감쇠 곡선
                if (_freqBands[g] < _bandBuffer[g])
                {
                    _bandBuffer[g] -= _bufferDecrease[g];
                    _bufferDecrease[g] *= 1.2f;
                }
            }
        }
    }
}