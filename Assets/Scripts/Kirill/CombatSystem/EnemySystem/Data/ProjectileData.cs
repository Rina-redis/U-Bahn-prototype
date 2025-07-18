using System.Collections.Generic;
using UnityEngine;
public enum TargetType
{
    DIRECTION,
    DESTINATION_POINT,
    CURRENT_TARGET

}
public enum HitType
{
    FIRST,
    TARGET,
    ALL,
    NOBODY
}
[CreateAssetMenu(fileName = "ShootingData_NAME_Projectile", menuName = "Combat/Shooting/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    public TargetType targetType;
    public HitType hitType;
    public float speed;
    public List<Effect> effects;
}
