using Core;
using Michsky.MUIP;
using Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger = Core.Logger;

[DefaultExecutionOrder(500)]
public class InGameManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private ModalWindowManager _pausePanel;
    [SerializeField] private ModalWindowManager _clearPanel;
    
    [Header("Progress UI")]
    [SerializeField] private Image _progressBar;
    [SerializeField] private TMP_Text _songTitleText;
    [SerializeField] private TMP_Text _songArtistText;
    [SerializeField] private TMP_Text _currentTimeText;
    [SerializeField] private TMP_Text _totalTimeText;
    
    private bool _isPaused = false;
    private bool _isCleared = false;
    
    private float _uiUpdateTimer = 0f;

    private void Awake()
    {
        _isPaused = false;
        _uiUpdateTimer = 0f;
        if(InputManager.Instance != null)
            InputManager.Instance.OnCancelAction += TogglePause;
        GameManager.Instance.SetCursorLocked(true);
    }

    private void OnDestroy()
    {
        if(InputManager.Instance != null) 
            InputManager.Instance.OnCancelAction -= TogglePause;
    }

    private void Start()
    {
        Time.timeScale = 1f; // 씬 진입 시 정상 속도 보장

        // 2. 곡 정보 UI 연동
        if (AudioManager.Instance != null && AudioManager.Instance.SelectedTrack != null)
        {
            _songTitleText.text = AudioManager.Instance.SelectedTrack.trackName;
            _songArtistText.text = AudioManager.Instance.SelectedTrack.trackArtist;
        }
    }
    
    private void Update()
    {
        // 클리어 상태면 업데이트 중지
        if (_isCleared) return;
        
        UpdateProgressBar();
        CheckGameClear();
    }

    private void TogglePause()
    {
        if (!_isPaused) 
        {
            OpenPausePanel();
        }
    }

    public void OpenPausePanel()
    {
        if (!_isPaused)
            _isPaused = true;
        _pausePanel.Open(); // 일시정지 UI 띄우기
        Time.timeScale = 0f;
        AudioManager.Instance.PauseMusic();
        InputManager.Instance.SetInputActive(false); // 조작 막기
        GameManager.Instance.SetCursorLocked(false); // 마우스 커서 보이게 하기
    }

    public void ClosePausePanel()
    {
        if (_isPaused)
            _isPaused = false;
        _pausePanel.Close(); // 일시정지 UI 숨기기
        Time.timeScale = 1f;
        AudioManager.Instance.ResumeMusic();
        InputManager.Instance.SetInputActive(true); // 조작 재개
        GameManager.Instance.SetCursorLocked(true); // 마우스 커서 다시 숨기기
    }
    
    // 곡 재생 바 및 시간 연동
    private void UpdateProgressBar()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.IsPlaying)
        {
            float current = AudioManager.Instance.CurrentTime;
            float total = AudioManager.Instance.TotalTime;
            
            if (total > 0)
            {
                _progressBar.fillAmount = current / total;
            }
            
            _uiUpdateTimer -= Time.deltaTime;
            if (_uiUpdateTimer > 0f) return;
            _uiUpdateTimer = 0.5f;
            
            _currentTimeText.text = FormatTime(current);
            _totalTimeText.text = FormatTime(total);
        }
    }
    
    // 클리어 체크 및 UI 띄우기
    private void CheckGameClear()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.TotalTime > 0)
        {
            // 재생 시간이 총 시간의 끝자락(예: 0.1초 전)에 도달하면 클리어로 판정
            if (AudioManager.Instance.CurrentTime >= AudioManager.Instance.TotalTime - 0.1f)
            {
                GameClear();
            }
        }
    }
    
    /// <summary>
    /// 완전한 게임 클리어 상태 처리
    /// - 음악 정지
    /// - Mixer 완전 초기화
    /// - 캐릭터 완전 정지
    /// - 모든 입력 차단
    /// - UI 표시
    /// </summary>
    private void GameClear()
    {
        _isCleared = true;
        Logger.Instance.LogInfo("===== GAME CLEAR START =====");

        // 1. 즉시 게임 시간 정지
        Time.timeScale = 0f;
        Logger.Instance.LogInfo("Time.timeScale = 0");

        // 2. 모든 입력 차단
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetInputActive(false);
            Logger.Instance.LogInfo("InputManager disabled");
        }
        else
        {
            Logger.Instance.LogWarning("InputManager.Instance is null");
        }

        // 3. 음악 정지 및 Mixer 완전 초기화
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.CompleteMixerReset(0.5f);
            Logger.Instance.LogInfo("AudioManager stopped and Mixer reset");
        }
        else
        {
            Logger.Instance.LogWarning("AudioManager.Instance is null");
        }

        // 4. 커서 활성화 (클리어 UI 상호작용용)
        GameManager.Instance.SetCursorLocked(false);
        Logger.Instance.LogInfo("Cursor unlocked");

        // 5. 클리어 UI 표시
        _clearPanel.Open();
        Logger.Instance.LogInfo("Clear Panel opened");
        
        Logger.Instance.LogInfo("===== GAME CLEAR COMPLETE =====");
    }

    /// <summary>
    /// 같은 씬 재시작 또는 다음 씬 로드 전 정리 작업
    /// </summary>
    public void ResetScene()
    {
        Logger.Instance.LogInfo("Scene Reset Start");
        
        _isPaused = false;
        _isCleared = false;
        
        // Pause 패널 해제
        if (_pausePanel != null)
        {
            _pausePanel.Close();
        }
        
        // 커서 상태 초기화
        if(GameManager.Instance != null)
            GameManager.Instance.SetCursorLocked(false);
        
        // AudioManager Mixer 즉시 초기화
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.CompleteMixerReset(0f);  // 즉시 리셋 (transition time = 0)
            Logger.Instance.LogInfo("AudioManager Mixer reset immediately");
        }
        
        // 게임 시간 복원
        Time.timeScale = 1f;
        
        // InputManager 활성화
        if (InputManager.Instance != null)
        {
            InputManager.Instance.SetInputActive(true);
            Logger.Instance.LogInfo("InputManager enabled");
        }
        
        Logger.Instance.LogInfo("===== SCENE RESET COMPLETE =====");
    }
    
    // 초(Seconds)를 M:SS 포맷으로 변환해주는 유틸리티 함수
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0}:{1:00}", minutes, seconds);
    }
}