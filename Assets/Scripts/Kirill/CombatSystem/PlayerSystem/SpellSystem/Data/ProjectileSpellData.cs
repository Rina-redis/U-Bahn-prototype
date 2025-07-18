using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileSpellData", menuName = "Scriptable Objects/ProjectileSpellData")]
public class ProjectileSpellData : SpellData
{
    public TargetType targetType; // Has to be the same with the Projectile!
    public List<UnitType> targetTypes;

    public float flightSpeed;
    public float maxDistance;
    public GameObject _projectile;

    public float executionTime;
    public float executionDelay;
    public int executionAmount;
    public float damageProExecution;
}
