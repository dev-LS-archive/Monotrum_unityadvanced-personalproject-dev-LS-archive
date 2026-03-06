using UnityEngine;

namespace Tunnel
{
    public class TunnelRing : MonoBehaviour
    {
        // TunnelGenerator가 풀에서 꺼낸 큐브를 넘겨주면 여기에 보관
        [SerializeField] private Transform[] _cubes;
        
        private Vector3[] _baseScales; // 원래 스케일 보존용
        
        /// <summary>
        /// 외부에서 받은 큐브 배열을 원형으로 배치하여 링을 구성한다.
        /// SetParent -> localPosition → localRotation 순서로 설정하여
        /// 부모 위치/스케일에 관계없이 안전하게 배치한다.
        /// </summary>
        public void Initialize(Transform[] cubes, float radius)
        {
            _cubes = cubes;
            _baseScales = new Vector3[cubes.Length];
            int count = _cubes.Length;

            for (int i = 0; i < count; i++)
            {
                // 360도를 큐브 수로 나눠 균등 간격 각도를 구한 뒤 라디안 변환
                float angle = (360f / count) * i * Mathf.Deg2Rad;
                // Cos = X좌표, Sin = Y좌표 -> 반지름을 곱하면 원 위의 점
                // Z는 0이라 링 하나가 XY 평면에 존재한다
                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f
                );
                
                // 부모를 먼저 지정한 뒤 로컬 좌표를 세팅해야
                // 부모의 위치/스케일이 달라도 틀어지지 않는다
                
                // 링의 자식으로 설정하여 링 이동/회수 시 큐브가 함께 따라가도록 한다
                _cubes[i].SetParent(transform);
                // 부모 기준의 로컬 좌표를 세팅한다
                _cubes[i].localPosition = pos;
                
                // 배치 각도만큼 Z축 회전 -> 로컬 X축이 방사 방향과 일치
                // 터널 벽처럼 중심에서 바깥을 향하는 형태가 된다
                // localRotation을 사용하여 부모 기준의 로컬 회전을 명확히 한다
                _cubes[i].localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
                
                // SpawnRing에서 설정한 직육면체 스케일을 기억해둔다
                _baseScales[i] = _cubes[i].localScale;
            }
        }
        
        /// <summary>
        /// AudioAnalyzer의 BandBuffer를 받아 각 큐브의 스케일을 갱신한다.
        /// 큐브 수가 밴드 수(8)보다 많으면 모듈러(%)로 순환 매핑된다.
        /// 예: 큐브 16개 -> 밴드 0~7이 두 바퀴 돌며 할당
        /// </summary>
        public void UpdateVisual(float[] bandBuffer, float scaleMultiplier)
        {
            for (int i = 0; i < _cubes.Length; i++)
            {
                // 큐브 인덱스를 밴드 수로 나눈 나머지로 밴드 순환 할당
                int bandIndex = i % bandBuffer.Length;
                
                // 기본 스케일 1에 밴드 값을 더해 음악 반응 크기 변화 생성
                // scaleMultiplier로 반응 강도 조절 (인스펙터 튜닝용)
                float scale = 1f + bandBuffer[bandIndex] * scaleMultiplier;
                
                // 원래 비율을 유지한 채 오디오 반응값을 곱한다
                //_cubes[i].localScale = _baseScales[i] * scale;
                
                // Y(방사 방향)만 반응시켜 옆/앞뒤 큐브와 겹치지 않게 한다
                _cubes[i].localScale = new Vector3(
                    _baseScales[i].x + scale,
                    _baseScales[i].y,
                    _baseScales[i].z
                );
            }
        }
        
        /// <summary>
        /// 링 회수 시 큐브를 부모에서 분리하여 반환 준비한다.
        /// TunnelGenerator가 큐브를 풀에 되돌릴 때 사용한다.
        /// </summary>
        public Transform[] DetachCubes()
        {
            if (_cubes == null) return null;

            for (int i = 0; i < _cubes.Length; i++)
            {
                _cubes[i].SetParent(null);
            }

            Transform[] cubes = _cubes;
            _cubes = null;
            return cubes;
        }
    }
}
