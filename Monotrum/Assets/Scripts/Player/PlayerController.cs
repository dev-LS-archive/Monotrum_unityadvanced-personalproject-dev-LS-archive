using Core;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;

        [Header("Movement (Tape Stop)")]
        [Tooltip("전진 입력 시 목표로 하는 Speed 파라미터 값 (Run_N의 Threshold인 6)")]
        [SerializeField] private float _maxRunSpeed = 6f;
        [Tooltip("가속 및 감속(테이프 스탑)에 걸리는 시간 민감도")]
        [SerializeField] private float _speedLerpRate = 3f;

        [Header("Jump (Math based)")]
        [SerializeField] private float _jumpForce = 5f;
        [SerializeField] private float _gravity = -15f;
    
        private float _currentSpeed = 0f;
        private float _currentVerticalVelocity = 0f;
        private float _currentYPosition = 0f;
        private bool _isGrounded = true;
        
        // 테이프 스톱 연동용
        public float SpeedRatio => _currentSpeed / _maxRunSpeed;

        private void Start()
        {
            if (_animator == null) _animator = GetComponent<Animator>();
        
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnJumpAction += TriggerJump;
            }
        }

        private void OnDestroy()
        {
            if (Singleton<InputManager>.IsQuitting) return;
            
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnJumpAction -= TriggerJump;
            }
        }

        private void Update()
        {
            HandleRunAndTapeStop();
            HandleMathJump();
        }

        private void HandleRunAndTapeStop()
        {
            float moveY = InputManager.Instance != null ? InputManager.Instance.MoveInput.y : 0f;
            
            // 1. Speed 처리 (0 ~ 6) : 애니메이션 포즈 블렌딩 (달리기 -> 걷기 -> 대기)
            float targetSpeed = Mathf.Clamp01(moveY) * _maxRunSpeed;
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _speedLerpRate);
            
            // 목표에 충분히 가까우면 스냅
            if (Mathf.Abs(_currentSpeed - targetSpeed) < 0.01f)
                _currentSpeed = targetSpeed;
            
            _animator.SetFloat(Define.Anim.Speed, _currentSpeed);

            // 2. MotionSpeed 처리 (0 ~ 1) : 애니메이션 재생 속도 조절 (Tape Stop 연출과 연동)
            // 현재 스피드가 최대 스피드에서 차지하는 비율(0.0 ~ 1.0)을 계산하여 바로 대입
            float currentMotionSpeed = _currentSpeed / _maxRunSpeed;
            _animator.SetFloat(Define.Anim.MotionSpeed, currentMotionSpeed);
            
            // 3. 오디오 피치 동기화 (테이프 스톱)
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetPitch(SpeedRatio);
        }

        private void TriggerJump()
        {
            if (_isGrounded)
            {
                _isGrounded = false;
                _currentVerticalVelocity = _jumpForce;
                _animator.SetBool(Define.Anim.IsJump, true);
            }
        }

        private void HandleMathJump()
        {
            if (!_isGrounded)
            {
                _currentVerticalVelocity += _gravity * Time.deltaTime;
                _currentYPosition += _currentVerticalVelocity * Time.deltaTime;

                if (_currentVerticalVelocity < 0f)
                {
                    _animator.SetBool(Define.Anim.IsFalling, true);
                }

                if (_currentYPosition <= 0f)
                {
                    _currentYPosition = 0f;
                    _isGrounded = true;
                    _currentVerticalVelocity = 0f;
                
                    _animator.SetBool(Define.Anim.IsJump, false);
                    _animator.SetBool(Define.Anim.IsFalling, false);
                }
            }

            _animator.SetBool(Define.Anim.IsGrounded, _isGrounded);

            transform.localPosition = new Vector3(
                transform.localPosition.x, 
                _currentYPosition, 
                transform.localPosition.z
            );
        }
    }
}