using UnityEngine;

[CreateAssetMenu(fileName = "NewAugment", menuName = "Augment/AugmentData")]
public class AugmentData : ScriptableObject
{
    [Header("Info")]
    public string augmentName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Effect")]
    public AugmentType type;
    public float value;

    public enum AugmentType
    {
        IncreaseMoveSpeed,
        IncreaseMaxHealth,
        Heal,
        IncreaseDamage,
        IncreaseFireRate,
        IncreaseBulletCount,
        IncreaseCollectionRadius,
    }
}
