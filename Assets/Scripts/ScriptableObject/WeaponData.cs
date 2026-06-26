using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [Tooltip("Tag trong ObjectPool")]
    public string bulletTag;   
    public float fireRate;
    public float detectRange;
    [Tooltip("Bao nhieu tia dan dc ban ra")]
    public int bulletCount = 1;  
    [Tooltip("Khoang cach tu tia 1-2")]
    public float spreadAngle = 0f;
}