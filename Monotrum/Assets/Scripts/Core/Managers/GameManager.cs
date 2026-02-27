using UnityEngine;

namespace Core
{
    public class GameManager : Singleton<GameManager>
    {
        [Header("Cursor Settings")]
        [SerializeField] private bool _lockCursorOnStart = true;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeManagers();
            
            if (_lockCursorOnStart)
                SetCursorLocked(true);
        }
        
        void Start()
        {
            Logger.Instance.LogInfo("Starting Game Manager");
        }

        private void Update()
        {
            // InputManager가 살아있고, 취소 키(ESC)가 눌렸다면
            if (InputManager.Instance != null && InputManager.Instance.ConsumeCancel())
            {
                ToggleCursorState();
                Logger.Instance.LogInfo("Escape Pressed");
            }
        }
        
        private void InitializeManagers()
        {
            // 매니저를 생성하고 transform을 GameManager 밑으로 옮겨서 정리
            RegisterManager<InputManager>();
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
        
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}
