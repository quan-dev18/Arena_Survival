using UnityEngine;
using System.Collections.Generic;

public class AugmentManager : MonoBehaviour
{
    [Header("Augments")]
    [SerializeField] private AugmentData[] _allAugments;
    [SerializeField] private int _optionsPerLevel = 3;

    [Header("References")]
    [SerializeField] private AugmentSelectionUI _selectionUI;

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private PlayerAttack _playerAttack;
    private PlayerExperience _playerExperience;

    // Cac static percent bonus ap dung toan cuc boi augment
    public static float DamagePercent { get; private set; } = 0f;
    public static float AttackSpeedPercent { get; private set; } = 0f;
    public static float HealRegenPercent { get; private set; } = 0f;

    // Base values
    private float _baseMoveSpeed;
    private float _baseMaxHealth;
    private float _baseCollectionRadius;

    // Accumulated percent bonuses (non-static)
    private float _moveSpeedPercent;
    private float _maxHealthPercent;
    private float _collectionRadiusPercent;

    public bool IsSelecting { get; private set; }

    private void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        _playerAttack = FindAnyObjectByType<PlayerAttack>();
        _playerExperience = FindAnyObjectByType<PlayerExperience>();

        if (_playerController != null)
            _baseMoveSpeed = _playerController.MoveSpeed;
        if (_playerHealth != null)
            _baseMaxHealth = _playerHealth.MaxHealth;
        if (_playerExperience != null)
            _baseCollectionRadius = _playerExperience.CollectionRadius;

        if (_playerExperience != null)
            _playerExperience.OnLevelUp += OnPlayerLevelUp;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        if (_playerExperience != null)
            _playerExperience.OnLevelUp -= OnPlayerLevelUp;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= OnGameOver;
    }

    private void OnPlayerLevelUp(int level)
    {
        IsSelecting = true;
        Time.timeScale = 0f;
        AugmentData[] options = GetRandomOptions(_optionsPerLevel);
        _selectionUI.Show(options, OnAugmentSelected);
    }

    private void OnAugmentSelected(AugmentData augment)
    {
        IsSelecting = false;
        _selectionUI.Hide();

        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
            return;

        ApplyAugment(augment);
        Time.timeScale = 1f;
    }

    private void OnGameOver()
    {
        IsSelecting = false;
        _selectionUI.Hide();
    }

    // Lay N augment ngau nhien theo weight tu danh sach
    private AugmentData[] GetRandomOptions(int count)
    {
        if (_allAugments == null || _allAugments.Length == 0)
            return new AugmentData[count];

        List<AugmentData> pool = new List<AugmentData>(_allAugments);
        AugmentData[] result = new AugmentData[count];

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (var a in pool)
                totalWeight += a.spawnWeight;

            float rand = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            int idx = 0;
            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].spawnWeight;
                if (rand < cumulative)
                {
                    idx = j;
                    break;
                }
            }

            result[i] = pool[idx];
            pool.RemoveAt(idx);
        }

        return result;
    }

    private void ApplyAugment(AugmentData augment)
    {
        switch (augment.type)
        {
            case AugmentData.AugmentType.IncreaseMoveSpeed:
                _moveSpeedPercent += augment.value;
                _playerController.SetMoveSpeed(_baseMoveSpeed * (1 + _moveSpeedPercent / 100f));
                break;

            case AugmentData.AugmentType.IncreaseMaxHealth:
                _maxHealthPercent += augment.value;
                _playerHealth.SetMaxHealth(_baseMaxHealth * (1 + _maxHealthPercent / 100f));
                break;

            case AugmentData.AugmentType.Heal:
                _playerHealth.Heal(_playerHealth.MaxHealth * augment.value / 100f);
                break;

            case AugmentData.AugmentType.IncreaseDamage:
                DamagePercent += augment.value;
                break;

            case AugmentData.AugmentType.IncreaseFireRate:
                AttackSpeedPercent += augment.value;
                break;

            case AugmentData.AugmentType.IncreaseBulletCount:
                _playerAttack.AddBulletBonus(Mathf.RoundToInt(augment.value));
                break;

            case AugmentData.AugmentType.IncreaseCollectionRadius:
                _collectionRadiusPercent += augment.value;
                _playerExperience.SetCollectionRadius(_baseCollectionRadius * (1 + _collectionRadiusPercent / 100f));
                break;

            case AugmentData.AugmentType.HealRegen:
                HealRegenPercent = Mathf.Min(5f, HealRegenPercent + augment.value);
                break;
        }
    }
}
