using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.PlayerLoop;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    private bool _isPause = false;
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

    private WaitForSeconds _delay3s = new WaitForSeconds(3f);

    public float SurvivalTime { get; private set; }
    public float DifficultyMultiplier => _difficultyCurve.Evaluate(SurvivalTime);
    public int CurrentMinute { get; private set; }
    public float MinuteProgress => (SurvivalTime % _eventInterval) / _eventInterval;
    public bool IsGameOver { get; private set; }

    public static GameManager Instance { get; private set; }
    public event System.Action<int> OnMinuteEvent;
    public event System.Action OnGameOver;

    [Header("TimeAnimation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private TextMeshPro _timeCountDownTMP;
    [SerializeField] private float _countDownTime = 3f;
    private bool isGameStart = false;

    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _gameOverUI.SetActive(false);
        _gamePauseUI.SetActive(false);
        _inputManager.OnPause += TogglePause;
    }

    private void OnDestroy()
    {
        if (_inputManager != null)
            _inputManager.OnPause -= TogglePause;
    }

    private void Update()
    {
        if (IsGameOver) return;
        // if(!isGameStart) return;

        SurvivalTime += Time.deltaTime;

        int minute = Mathf.FloorToInt(SurvivalTime / _eventInterval) + 1;
        if (minute > CurrentMinute)
        {
            CurrentMinute = minute;
            OnMinuteEvent?.Invoke(minute);
            TriggerMinuteEvent(minute);
        }
        _gamePauseUI.SetActive(_isPause);
    }

    public void OnPlayerDied()
    {
        IsGameOver = true;

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

    void TogglePause()
    {
        _isPause = !_isPause;
        Time.timeScale = _isPause ? 0 : 1f;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        _isPause = false;
        SceneManager.LoadScene("MainGame");
    }

    public void ResumeGame()
    {
        _gamePauseUI.SetActive(false);
        _isPause = false;
        Time.timeScale = 1f;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        _isPause = false;
        SceneManager.LoadScene("Menu");
    }

    // public void GameStart()
    // {
    //     isGameStart = true;
    // }
}