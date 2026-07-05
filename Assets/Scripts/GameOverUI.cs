using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _survivalTimeText;

    private void OnEnable()
    {
        if (GameManager.Instance == null) return;

        float time = GameManager.Instance.SurvivalTime;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        _survivalTimeText.text = $"You survived for {minutes:D2}:{seconds:D2}";
    }
}
