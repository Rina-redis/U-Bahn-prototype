using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData_NAME_RangedAttack", menuName = "Combat/Enemy/Ranged Attack Data")]
public class EnemyRangedAttackData : ScriptableObject
{
    public GameObject _projectile;
    public float damage;
}
