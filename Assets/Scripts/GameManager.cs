using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        CountdownToStart,
        Playing,
        Pause,
        GameOver
    }

    [Header("References")]
    [SerializeField] private InputManager _inputManager;

    [Header("Timer")]
    [Tooltip("Interval (in seconds) between each 'minute' gameplay event.")]
    [SerializeField] private float _eventInterval = 60f;

    [Header("Difficulty")]
    [Tooltip("Maps SurvivalTime -> difficulty multiplier used by systems like EnemySpawner.")]
    [SerializeField] private AnimationCurve _difficultyCurve = AnimationCurve.Linear(0, 1, 600, 5);

    [Header("Player Systems")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private ParticleSystem _footstepParticle;

    [Header("UI")]
    [SerializeField] private TimerUI _timerUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gamePauseUI;

    [Header("Countdown")]
    [Tooltip("How many seconds the CountdownToStart state lasts before Playing begins.")]
    [SerializeField] private float _countdownTime = 3f;

    private WaitForSeconds _gameOverDelay = new WaitForSeconds(3f);

    //Public read-only state
    public float SurvivalTime { get; private set; }
    public float DifficultyMultiplier => _difficultyCurve.Evaluate(SurvivalTime);
    public int CurrentMinute { get; private set; }
    public float MinuteProgress => (SurvivalTime % _eventInterval) / _eventInterval;
    public GameState CurrentState { get; private set; } = GameState.None;
    public float CountdownTime => _countdownTime;

    public static GameManager Instance { get; private set; }

    // ---- Events for UI / other systems to subscribe to ----
    public event System.Action<int> OnMinuteEvent;
    public event System.Action OnGameOver;
    public event System.Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate GameManager detected, destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (_gameOverUI != null) _gameOverUI.SetActive(false);
        if (_gamePauseUI != null) _gamePauseUI.SetActive(false);

        if (_inputManager != null)
            _inputManager.OnPause += TogglePause;

        ChangeState(GameState.CountdownToStart);
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
            _inputManager.OnPause -= TogglePause;
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
            UpdatePlaying();
    }

    private void UpdatePlaying()
    {
        SurvivalTime += Time.deltaTime;

        int minute = Mathf.FloorToInt(SurvivalTime / _eventInterval) + 1;
        if (minute > CurrentMinute)
        {
            CurrentMinute = minute;
            OnMinuteEvent?.Invoke(minute);
            TriggerMinuteEvent(minute);
        }
    }
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;

        OnStateChanged?.Invoke(newState);

        switch (newState)
        {
            case GameState.CountdownToStart:
                Time.timeScale = 1f;
                SetPlayerSystems(false);
                StartCoroutine(CountdownCoroutine());
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                if (_gamePauseUI != null) _gamePauseUI.SetActive(false);
                SetPlayerSystems(true);
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                if (_gamePauseUI != null) _gamePauseUI.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 1f;
                SetPlayerSystems(false);
                break;
        }
    }

    private void SetPlayerSystems(bool enable)
    {
        if (_playerController != null)
            _playerController.enabled = enable;
        if (_playerAttack != null)
            _playerAttack.enabled = enable;
    }

    // Only responsible for timing the state transition.
    // Any visual countdown (text/animation) should be its own script (e.g. CountdownUI)
    // that listens to OnStateChanged and reads CountdownTime.
    private IEnumerator CountdownCoroutine()
    {
        yield return new WaitForSeconds(_countdownTime);
        ChangeState(GameState.Playing);
    }

    public void OnPlayerDied()
    {
        if (CurrentState == GameState.GameOver) return;

        ChangeState(GameState.GameOver);

        if (_footstepParticle != null)
            _footstepParticle.Stop();
        if (_timerUI != null)
            _timerUI.enabled = false;
        if (_gameOverUI != null)
            StartCoroutine(ShowGameOverDelayed());

        OnGameOver?.Invoke();
    }

    private IEnumerator ShowGameOverDelayed()
    {
        yield return _gameOverDelay;
        _gameOverUI.SetActive(true);
    }

    private void TriggerMinuteEvent(int minute)
    {
        EnemySpawner spawner = EnemySpawner.Instance;
        if (spawner != null)
            spawner.OnTimeEvent(minute);
    }

    private void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Pause);
        else if (CurrentState == GameState.Pause)
            ChangeState(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainGame");
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Pause)
            ChangeState(GameState.Playing);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}