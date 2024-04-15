using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData")]
public class WeaponSO : ScriptableObject
{
    public ResourceType weaponType;
    public float weaponShootPause = 0.5f;
    public float attackDistance = 10;
    public float damage = 1;
}
