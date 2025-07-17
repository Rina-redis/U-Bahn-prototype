using UnityEngine;

public enum UnitType
{
    PLAYER,
    ENEMY,
    VEHICLE
}
public enum UnitParams
{
    HEALTH,
    ARMOR,
    STRENGTH,
    SPEED,
    ATTACK_SPEED,
    ARMOR_PENETRATION
}
[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public UnitType unitType;
    public float health;
    public float armor;
    public float strength;
    public float speed;
    public float attackSpeed;
    public float armorPenetration;
}
