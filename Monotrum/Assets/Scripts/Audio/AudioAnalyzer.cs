using Core;
using UnityEngine;

namespace Audio
{
    public class AudioAnalyzer : MonoBehaviour
    {
        private float[] _samples = new float[512];
        private float[] _freqBands = new float[8];
        private float[] _bandBuffer = new float[8];
        private float[] _bufferDecrease = new float[8];
    
        public float[] BandBuffer => _bandBuffer;
    
        void Update()
        {
            if (AudioManager.Instance.IsPlaying)
            {
                // _audioSource.GetSpectrumData로 인해 _samples 값이 덮어씌워짐
                AudioManager.Instance.GetSpectrumData(_samples);
                ComputeFreqBands();
                ApplySmoothing();
            }
        }

        void ComputeFreqBands()
        {
            int count = 0;
            // 8개 주파수 대역으로 압축
            for (int i = 0; i < 8; i++)
            {
                float average = 0;
                // f의 p제곱
                int sampleCount = (int)Mathf.Pow(2, i) * 2;
                if (i == 7) sampleCount += 2; 

                for (int j = 0; j < sampleCount; j++)
                {
                    average += _samples[count] * (count + 1);
                    count++;
                }

                average /= count;
                _freqBands[i] = average * 10; 
            }
        }

        void ApplySmoothing()
        {
            // 스무딩 (부드러운 하강 곡선)
            for (int g = 0; g < 8; g++)
            {
                // 상승 시 즉시 반영
                if (_freqBands[g] > _bandBuffer[g])
                {
                    _bandBuffer[g] = _freqBands[g];
                    _bufferDecrease[g] = 0.005f;
                }
                // 하강 시 가속 감쇠
                if (_freqBands[g] < _bandBuffer[g])
                {
                    _bandBuffer[g] -= _bufferDecrease[g];
                    _bufferDecrease[g] *= 1.2f; 
                }
                // Debug.Log($"{g}: {_bandBuffer[g]}");
            }
        }
    }
}