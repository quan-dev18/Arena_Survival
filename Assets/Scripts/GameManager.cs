using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class GameManager : MonoBehaviour
{
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
    [SerializeField] private Button _playAgainButton;

    private WaitForSeconds _delay3s = new WaitForSeconds(3f);

    public float SurvivalTime { get; private set; }
    public float DifficultyMultiplier => _difficultyCurve.Evaluate(SurvivalTime);
    public int CurrentMinute { get; private set; }
    public float MinuteProgress => (SurvivalTime % _eventInterval) / _eventInterval;
    public bool IsGameOver { get; private set; }

    public static GameManager Instance { get; private set; }
    public event System.Action<int> OnMinuteEvent;
    public event System.Action OnGameOver;

    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _gameOverUI.SetActive(false);
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(RestartGame);
    }

    private void Update()
    {
        if (IsGameOver) return;

        SurvivalTime += Time.deltaTime;

        int minute = Mathf.FloorToInt(SurvivalTime / _eventInterval) + 1;
        if (minute > CurrentMinute)
        {
            CurrentMinute = minute;
            OnMinuteEvent?.Invoke(minute);
            TriggerMinuteEvent(minute);
        }
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

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}