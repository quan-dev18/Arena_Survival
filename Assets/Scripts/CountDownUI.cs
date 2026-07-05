using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _root;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private Animator _animator;

    [Header("Animator")]
    [Tooltip("Name of the Animator state to replay on each countdown tick (must match the state name in the Animator Controller).")]
    [SerializeField] private string _countdownStateName = "Countdown";

    private Coroutine _routine;

    private void Awake()
    {
        if (_root != null)
            _root.SetActive(false);
    }

    private void Start()
    {
        // Use Start (not OnEnable) so this runs after every Awake() in the
        // scene has finished, guaranteeing GameManager.Instance is already set.
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("CountdownUI: GameManager.Instance is null, cannot subscribe.");
            return;
        }

        GameManager.Instance.OnStateChanged += HandleStateChanged;

        // GameManager may have already fired OnStateChanged(CountdownToStart)
        // in its own Start() before this Start() ran (Unity doesn't guarantee
        // Start() order between different scripts). Sync manually so we never
        // miss the very first state change.
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.CountdownToStart)
        {
            StartCountdown(GameManager.Instance.CountdownTime);
        }
        else
        {
            StopCountdown();
        }
    }

    private void StartCountdown(float duration)
    {
        if (_root != null)
            _root.SetActive(true);

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(CountdownRoutine(duration));
    }

    private void StopCountdown()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (_root != null)
            _root.SetActive(false);
    }

    private IEnumerator CountdownRoutine(float duration)
    {
        int seconds = Mathf.Max(1, Mathf.CeilToInt(duration));

        for (int i = seconds; i > 0; i--)
        {
            if (_countdownText != null)
                _countdownText.text = i.ToString();

            if (_animator != null && !string.IsNullOrEmpty(_countdownStateName))
                _animator.Play(_countdownStateName, 0, 0f); // force restart the clip from frame 0 every tick

            yield return new WaitForSeconds(1f);
        }

        _routine = null;
    }
}