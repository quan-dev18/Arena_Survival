using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private GameObject _gamePauseUI;

    [Header("Timer")]
    [SerializeField] private TimerUI _timerUI;

    [Header("Settings")]
    [SerializeField] private float _gameOverUIDelay = 3f;

    private WaitForSeconds _gameOverDelay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _gameOverDelay = new WaitForSeconds(_gameOverUIDelay);
    }

    private void Start()
    {
        if (_gameOverUI != null) _gameOverUI.SetActive(false);
        if (_gamePauseUI != null) _gamePauseUI.SetActive(false);
    }

    public void ShowPauseUI()
    {
        if (_gamePauseUI != null) _gamePauseUI.SetActive(true);
    }

    public void HidePauseUI()
    {
        if (_gamePauseUI != null) _gamePauseUI.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        if (_gameOverUI != null) StartCoroutine(ShowGameOverDelayed());
    }

    private IEnumerator ShowGameOverDelayed()
    {
        yield return _gameOverDelay;
        if (_gameOverUI != null) _gameOverUI.SetActive(true);
    }

    public void DisableTimer()
    {
        if (_timerUI != null) _timerUI.enabled = false;
    }
}
