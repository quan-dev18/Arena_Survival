using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private OptionUI[] _options;

    private System.Action<AugmentData> _onSelected;

    private void Awake()
    {
        _panel.SetActive(false);
    }

    // Hien thi panel voi danh sach augment de chon
    public void Show(AugmentData[] augments, System.Action<AugmentData> onSelected)
    {
        _onSelected = onSelected;
        _panel.SetActive(true);

        for (int i = 0; i < _options.Length; i++)
        {
            if (i < augments.Length && augments[i] != null)
            {
                _options[i].Setup(augments[i], OnOptionClicked);
                _options[i].gameObject.SetActive(true);
            }
            else
            {
                _options[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnOptionClicked(AugmentData augment)
    {
        _panel.SetActive(false);
        _onSelected?.Invoke(augment);
    }

    [System.Serializable]
    private class OptionUI
    {
        public GameObject gameObject;
        public Button button;
        public RawImage background;
        public TextMeshProUGUI text;

        private AugmentData _data;

        public void Setup(AugmentData data, System.Action<AugmentData> callback)
        {
            _data = data;
            gameObject.SetActive(true);
            text.text = $"{data.augmentName}";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => callback(_data));
        }
    }
}
