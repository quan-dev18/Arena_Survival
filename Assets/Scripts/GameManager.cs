using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        CountdownToStart,
        Start,
        Playing,
        Pause,
        GameOver
    }
    [SerializeField] private InputManager _inputManager;

    [Header("Timer")]
    [SerializeField] private float _eventInterval = 60f;

    [Header("Difficulty")]
    [SerializeField] private AnimationCurve _difficultyCurve = AnimationCurve.Linear(0, 1, 600, 5);

    [Header("Player Systems")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private ParticleSystem _footstepParticle;

    [Header("UI")]
    [SerializeField] private TimerUI _timerUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gamePauseUI;

    [Header("TimeAnimation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshProUGUI _timeCountDownTMP;
    [SerializeField] private float _countDownTime = 3f;

    private WaitForSeconds _delay3s = new WaitForSeconds(3f);

    public float SurvivalTime { get; private set; }
    public float DifficultyMultiplier => _difficultyCurve.Evaluate(SurvivalTime);
    public int CurrentMinute { get; private set; }
    public float MinuteProgress => (SurvivalTime % _eventInterval) / _eventInterval;
    public GameState CurrentState { get; private set; } = GameState.None;

    public static GameManager Instance { get; private set; }
    public event System.Action<int> OnMinuteEvent;
    public event System.Action OnGameOver;
    public event System.Action<GameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _gameOverUI.SetActive(false);
        _gamePauseUI.SetActive(false);
        if (_timeCountDownTMP != null)
            _timeCountDownTMP.gameObject.SetActive(false);
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
        switch (CurrentState)
        {
            case GameState.Playing:
                UpdatePlaying();
                break;
        }
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
                StartCountdown();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                _gamePauseUI.SetActive(false);
                SetPlayerSystems(true);
                break;

            case GameState.Pause:
                Time.timeScale = 0f;
                _gamePauseUI.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 1f;
                if (_timeCountDownTMP != null)
                    _timeCountDownTMP.gameObject.SetActive(false);
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

    private void StartCountdown()
    {
        if (_timeCountDownTMP != null)
            _timeCountDownTMP.gameObject.SetActive(true);


        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        yield return new WaitForSeconds(_countDownTime);

        if (_timeCountDownTMP != null)
            _timeCountDownTMP.gameObject.SetActive(false);

        ChangeState(GameState.Playing);
    }

    public void OnPlayerDied()
    {
        if (CurrentState == GameState.GameOver) return;
        ChangeState(GameState.GameOver);

        if (_playerController != null)
            _playerController.enabled = false;
        if (_playerAttack != null)
            _playerAttack.enabled = false;
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
        yield return _delay3s;
        _gameOverUI.SetActive(true);
    }

    private void TriggerMinuteEvent(int minute)
    {
        EnemySpawner spawner = EnemySpawner.Instance;
        if (spawner != null)
            spawner.OnMinuteEvent(minute);
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
