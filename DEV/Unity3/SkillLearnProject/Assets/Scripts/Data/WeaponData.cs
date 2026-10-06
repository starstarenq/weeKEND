using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItemData", menuName = "ScriptableObjects/WeaponItemData", order = 1)]
public class WeaponItemData : ScriptableObject
{
    [Header("Weapon Identity")]
    public string Name;
    public string Korean_Name;

    [Header("Base Damage")]
    public int Base_Damage_Min;
    public int Base_Damage_Max;

    [Header("Cooldown (Seconds)")]
    public float Cooldown_Min;
    public float Cooldown_Max;

    [Header("Amount")]
    public int Amount_Min;
    public int Amount_Max;
}
