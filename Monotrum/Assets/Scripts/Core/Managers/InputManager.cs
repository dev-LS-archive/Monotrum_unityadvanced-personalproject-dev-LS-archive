using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    public class InputManager : Singleton<InputManager>
    {
        private MonotrumInput _input;
        
        public event Action OnCancelAction;
        public event Action OnJumpAction;
        
        // 현재 입력 상태
        public Vector2 MoveInput { get; private set; }

        // private bool _cancelInput;
        
        private bool _isInputBlocked = false;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 인스펙터 할당 없이 코드로 직접 생성
            _input = new MonotrumInput();
            
            // 이벤트 구독
            _input.Player.Move.performed += OnMove;
            _input.Player.Move.canceled += OnMove;
            _input.Player.Jump.performed += OnJump;
            // _input.Player.Interact.performed += OnInteract;
            _input.Player.Cancel.performed += OnCancel;
        }
        
        private void OnEnable()
        {
            _input?.Enable(); // Input System 활성화
        }

        private void OnDisable()
        {
            _input?.Disable(); // Input System 비활성화
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (_input != null)
            {
                _input.Player.Move.performed -= OnMove;
                _input.Player.Move.canceled -= OnMove;
                _input.Player.Jump.performed -= OnJump;
                // _input.Player.Interact.performed -= OnInteract;
                _input.Player.Cancel.performed -= OnCancel;
            }
        }
        
        #region Input Event Callbacks
        void OnMove(InputAction.CallbackContext ctx)
        {
            MoveInput = ctx.ReadValue<Vector2>();
        }
        
        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (_isInputBlocked) return;
            // Logger.Instance.LogInfo("OnJump");
            OnJumpAction?.Invoke();
        }

        /*private void OnInteract(InputAction.CallbackContext ctx)
        {
            if (_isInputBlocked) return;
            Logger.Instance.LogInfo("OnInteract");
        }*/

        private void OnCancel(InputAction.CallbackContext ctx)
        {
            if (_isInputBlocked) return;
            OnCancelAction?.Invoke(); // 폴링용 bool 변수 대신 즉시 이벤트 발송
            // _cancelInput = true;
        }
        #endregion
        
        // [학습 기록] Consume 폴링 방식 → 이벤트(OnCancelAction) 발송으로 전환하기 전 코드
        /*
        // GameManager에서 ESC 입력 확인 후 해제할 때 사용
        public bool ConsumeCancel()
        {
            if (_cancelInput)
            {
                _cancelInput = false;
                return true;
            }
            return false;
        }
        */
        
        public void SetInputActive(bool active)
        {
            _isInputBlocked = !active;
            if (_isInputBlocked)
            {
                MoveInput = Vector2.zero;
            }
        }
    }
}
