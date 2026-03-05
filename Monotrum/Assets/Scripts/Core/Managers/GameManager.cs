using UnityEngine;

namespace Core
{
    public class GameManager : Singleton<GameManager>
    {
        //[Header("Cursor Settings")]
        //[SerializeField] private bool _lockCursorOnStart = true;
        
        protected override void Awake()
        {
            base.Awake();
            
            // 중복 인스턴스면 초기화 건너뛰기
            if (Instance != this) return;
            
            QualitySettings.vSyncCount = 0; // VSync 강제 해제 (인풋렉 방지)
            Application.targetFrameRate = 144; // 144 프레임 고정 (GPU 보호 및 환경 통일)
            
            InitializeManagers();
            
            //if (_lockCursorOnStart)
            //    SetCursorLocked(true);
        }
        
        // 토글 관련 컨트롤은 전부 게임씬 안에서만 조정
        
        /*void Start()
        {
            Logger.Instance.LogInfo("Starting Game Manager");
            InputManager.Instance.OnCancelAction += ToggleCursorState;
        }
        
        private void OnDestroy()
        {
            if (IsQuitting) return; // 앱 종료 시에는 해제 불필요 (어차피 전부 파괴됨)
            
            if (InputManager.Instance != null)
                InputManager.Instance.OnCancelAction -= ToggleCursorState;
        }*/
        
        // [학습 기록] 폴링 방식 -> 옵저버 패턴으로 전환하기 전 코드
        /*
        private void Update()
        {
            // InputManager가 살아있고, 취소 키(ESC)가 눌렸다면
            if (InputManager.Instance != null && InputManager.Instance.ConsumeCancel())
            {
                ToggleCursorState();
                // Logger.Instance.LogInfo("Escape Pressed");
            }
        }
        */
        
        private void InitializeManagers()
        {
            // 매니저를 생성하고 transform을 GameManager 밑으로 옮겨서 정리
            RegisterManager<InputManager>();
            RegisterManager<AudioManager>();
            //RegisterManager<SceneController>();
            
            Logger.Instance.LogInfo("All System Managers Initialized.");
        }
        
        // 싱글톤 매니저를 생성(호출)하고 GameManager의 자식으로 넣는 제네릭 함수
        private void RegisterManager<T>() where T : Singleton<T>
        {
            // Instance를 호출하는 순간 없으면 자동 생성되고, 있으면 가져옵니다.
            var instance = Singleton<T>.Instance;
    
            if (instance != null)
            {
                instance.transform.SetParent(this.transform);
            }
        }
        
        private void ToggleCursorState()
        {
            bool isLocked = Cursor.lockState == CursorLockMode.Locked;
            // 커서 상태 반전 (잠김 <-> 풀림)
            SetCursorLocked(!isLocked);
    
            // 게임 시간 멈추기
            // Time.timeScale = isLocked ? 0f : 1f; 
    
            // UI 추후 관리
            // UIManager.Instance.OpenPauseMenu(!isLocked);
        }

        #region Cursor Control
        /// <summary>
        /// 커서 잠금/해제 (전역 접근)
        /// </summary>
        public void SetCursorLocked(bool isLocked)
        {
            Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !isLocked;
            Logger.Instance.LogInfo($"Cursor Locked: {isLocked}");
        }
        #endregion
    }
}
