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

    // Cac static modifier duoc ap dung toan cuc boi augment
    public static float DamageMultiplier { get; private set; } = 1f;
    public static float FireRateMultiplier { get; private set; } = 1f;

    private void Start()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        _playerAttack = FindAnyObjectByType<PlayerAttack>();
        _playerExperience = FindAnyObjectByType<PlayerExperience>();

        if (_playerExperience != null)
            _playerExperience.OnLevelUp += OnPlayerLevelUp;
    }

    private void OnDestroy()
    {
        if (_playerExperience != null)
            _playerExperience.OnLevelUp -= OnPlayerLevelUp;
    }

    private void OnPlayerLevelUp(int level)
    {
        Time.timeScale = 0f;
        AugmentData[] options = GetRandomOptions(_optionsPerLevel);
        _selectionUI.Show(options, OnAugmentSelected);
    }

    private void OnAugmentSelected(AugmentData augment)
    {
        ApplyAugment(augment);
        Time.timeScale = 1f;
    }

    // Lay N augment ngau nhien tu danh sach
    private AugmentData[] GetRandomOptions(int count)
    {
        if (_allAugments == null || _allAugments.Length == 0)
            return new AugmentData[count];

        // Copy danh sach de shuffle
        List<AugmentData> pool = new List<AugmentData>(_allAugments);
        AugmentData[] result = new AugmentData[count];

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
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
                _playerController.AddMoveSpeed(augment.value);
                break;

            case AugmentData.AugmentType.IncreaseMaxHealth:
                _playerHealth.AddMaxHealth(augment.value);
                break;

            case AugmentData.AugmentType.Heal:
                _playerHealth.Heal(augment.value);
                break;

            case AugmentData.AugmentType.IncreaseDamage:
                DamageMultiplier += augment.value;
                break;

            case AugmentData.AugmentType.IncreaseFireRate:
                FireRateMultiplier = Mathf.Max(0.1f, FireRateMultiplier - augment.value);
                break;

            case AugmentData.AugmentType.IncreaseBulletCount:
                _playerAttack.AddBulletBonus(Mathf.RoundToInt(augment.value));
                break;

            case AugmentData.AugmentType.IncreaseCollectionRadius:
                _playerExperience.AddCollectionRadius(augment.value);
                break;
        }
    }
}
