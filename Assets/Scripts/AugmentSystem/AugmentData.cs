using UnityEngine;

[CreateAssetMenu(fileName = "NewAugment", menuName = "Augment/AugmentData")]
public class AugmentData : ScriptableObject
{
    [Header("Info")]
    public string augmentName;

    [Header("Effect")]
    [Tooltip("Loai chi so can tang")]
    public AugmentType type;
    [Tooltip("Nhap % (vd: 10 = +10%). IncreaseBulletCount la so luong dan (flat). HealRegen la %/giay")]
    public float value;

    [Header("Spawn Rate")]
    [Range(0f, 100f)] public float spawnWeight = 1f;

    public enum AugmentType
    {
        IncreaseMoveSpeed,  
        IncreaseMaxHealth,
        Heal,
        IncreaseDamage,
        IncreaseFireRate,
        IncreaseBulletCount,
        IncreaseCollectionRadius,
        HealRegen,
    }
}
