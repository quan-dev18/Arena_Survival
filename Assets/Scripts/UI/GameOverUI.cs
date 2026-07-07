using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _survivalTimeText;

    private void OnEnable()
    {
        if (_survivalTimeText == null) return;

        float survivalTime = GameManager.Instance.SurvivalTime;
        if (survivalTime >= 900f)
        {
            _survivalTimeText.text = "Perfect Survival!";
        }
        else
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60);
            int seconds = Mathf.FloorToInt(survivalTime % 60);
            _survivalTimeText.text = "Survival time: " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
