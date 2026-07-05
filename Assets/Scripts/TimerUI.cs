using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private float _gameDuration = 900f;

    [Header("Boss Settings")]
    [SerializeField] private float _bossInterval = 300f;
    [SerializeField] private int _maxBossCount = 3;

    private float _elapsed;
    private int _bossCount;
    private int _lastBossTrigger;
    private bool _gameOver;

    public event System.Action<int> OnBossSpawn;
    public event System.Action OnGameOver;

    private void Awake()
    {
        if (_timerText == null)
            _timerText = GetComponent<TextMeshProUGUI>();

        UpdateDisplay(0);
    }

    private void Update()
    {
        if (_gameOver) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        _elapsed += Time.deltaTime;

        int totalSeconds = Mathf.FloorToInt(_elapsed);
        UpdateDisplay(totalSeconds);

        HandleBossSpawn(totalSeconds);
        HandleGameOver(totalSeconds);
    }

    private void UpdateDisplay(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void HandleBossSpawn(int totalSeconds)
    {
        if (_bossCount >= _maxBossCount) return;

        int currentTrigger = totalSeconds / Mathf.FloorToInt(_bossInterval);
        if (currentTrigger > _lastBossTrigger)
        {
            _lastBossTrigger = currentTrigger;
            _bossCount++;
            OnBossSpawn?.Invoke(_bossCount);
            // TODO: Spawn boss - sẽ cập nhật sau
            return;
        }
    }

    private void HandleGameOver(int totalSeconds)
    {
        if (totalSeconds < Mathf.FloorToInt(_gameDuration)) return;

        _gameOver = true;
        _timerText.text = "15:00";
        OnGameOver?.Invoke();
        // TODO: Xử lý kết thúc game (tính điểm, chuyển scene...)
        return;
    }
}
