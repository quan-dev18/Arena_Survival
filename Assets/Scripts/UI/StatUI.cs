using UnityEngine;
using TMPro;

public class StatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statText;

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private PlayerAttack _playerAttack;
    private PlayerExperience _playerExperience;

    private void Awake()
    {
        _playerController = FindAnyObjectByType<PlayerController>();
        _playerHealth = FindAnyObjectByType<PlayerHealth>();
        _playerAttack = FindAnyObjectByType<PlayerAttack>();
        _playerExperience = FindAnyObjectByType<PlayerExperience>();
    }

    private void Update()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        float hp = _playerHealth != null ? _playerHealth.CurrentHealth : 0;
        float maxHp = _playerHealth != null ? _playerHealth.MaxHealth : 0;
        float speed = _playerController != null ? _playerController.MoveSpeed : 0;
        int level = _playerExperience != null ? _playerExperience.CurrentLevel : 0;
        float xp = _playerExperience != null ? _playerExperience.CurrentXP : 0;
        float xpToNext = _playerExperience != null ? _playerExperience.XPToNextLevel : 0;
        float collectionRadius = _playerExperience != null ? _playerExperience.CollectionRadius : 0;

        WeaponData weapon = _playerAttack != null ? _playerAttack.CurrentWeapon : null;
        int bulletBonus = _playerAttack != null ? _playerAttack.BulletBonus : 0;
        string weaponName = weapon != null ? weapon.weaponName : "None";
        float fireRate = weapon != null ? weapon.fireRate : 0;
        int bulletCount = weapon != null ? weapon.bulletCount + bulletBonus : 0;
        float detectRange = weapon != null ? weapon.detectRange : 0;
        float spread = weapon != null ? weapon.spreadAngle : 0;

        float dmgBonus = AugmentManager.DamagePercent;
        float atkSpeedBonus = AugmentManager.AttackSpeedPercent;
        float healRegen = AugmentManager.HealRegenPercent;

        statText.text = $"HP: {hp:F0}/{maxHp:F0}\n" +
                        $"Speed: {speed:F1}\n" +
                        $"Level: {level}  XP: {xp:F0}/{xpToNext:F0}\n" +
                        $"Collection Radius: {collectionRadius:F1}\n" +
                        $"Weapon: {weaponName}\n" +
                        $"Fire Rate: {fireRate:F2}s\n" +
                        $"Bullets: {bulletCount}\n" +
                        $"Range: {detectRange:F1}\n" +
                        $"Spread: {spread:F1}\n" +
                        $"Dmg Bonus: {dmgBonus:F0}%\n" +
                        $"Atk Speed Bonus: {atkSpeedBonus:F0}%\n" +
                        $"Heal Regen: {healRegen:F1}%/s";
    }
}
