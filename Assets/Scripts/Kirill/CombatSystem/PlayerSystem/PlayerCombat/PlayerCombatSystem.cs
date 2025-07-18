using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombatSystem : UnitController
{
    [SerializeField] private PlayerCombatSystemData data;
    public override UnitData UnitData => data;

    private ScriptableArmor armorEq;
    private ScriptableWeapon weaponEq;
    private List<ScriptableConsumable> consumables;
    private GameCombatManager gameCombatManager;

    private List<PlayerCombatSystem> nearestPlayers = new List<PlayerCombatSystem>();
    private List<EnemyCombatBehaviourSystem> nearestEnemies = new List<EnemyCombatBehaviourSystem>();
    private List<VehicleCombatBehaviourSystem> nearestVehicles = new List<VehicleCombatBehaviourSystem>();

    private UnitController target;
    private float curAttackCooldown = 0f;
    private bool isSetUp = false;

    List<UnitType> unitTypesEnemy;
    List<UnitType> unitTypesVehicle;
    List<UnitType> unitTypesEnemyAndVehicle;

    void Awake()
    {
        gameCombatManager = GameObject.Find("GameCombatManager").GetComponent<GameCombatManager>();

        unitTypesEnemy = new List<UnitType>
        {
            UnitType.ENEMY
        };
        unitTypesVehicle = new List<UnitType>
        {
            UnitType.VEHICLE
        };
        unitTypesEnemyAndVehicle = new List<UnitType>
        {
            UnitType.ENEMY,
            UnitType.VEHICLE
        };

        base.Init();
    }

    public void Init(ScriptableArmor armorEq, ScriptableWeapon weaponEq, List<ScriptableConsumable> consumables)
    {
        this.armorEq = armorEq;
        this.weaponEq = weaponEq;
        this.consumables = new List<ScriptableConsumable>(consumables);

        if (!didAwake)
        {
            Awake();
        }

        ApplyUnitDataStats(armorEq.unitData);
        ApplyUnitDataStats(weaponEq.unitData);

        var spellArmorGO = Instantiate(armorEq.spell);
        spellArmorGO.transform.parent = transform;
        spellArmorGO.transform.localPosition = Vector2.zero;
        Spell spellArmor = spellArmorGO.GetComponent<Spell>();

        var spellWeaponGO = Instantiate(weaponEq.spell);
        spellWeaponGO.transform.parent = transform;
        spellWeaponGO.transform.localPosition = Vector2.zero;
        Spell spellWeapon = spellWeaponGO.GetComponent<Spell>();

        gameCombatManager.SetSpells(spellArmor, spellWeapon);

        curAttackCooldown = weaponEq.cooldown;

        //TODO Handle Consumables

        //
        isSetUp = true;
    }

    void OnBecameVisible()
    {

        /*float distance = (transform.position - target.position).magnitude;
        if (data.detectionRange < distance)
        {
            LoseTarget();
        }
        else if (data.attackRange < distance)
        {
            Chaise();
        }
        else
        {
            if (curAttackCooldown <= 0)
            {
                Attack();
            }
        }*/
    }

    void FixedUpdate()
    {
        if (!isSetUp)
            return;
        curAttackCooldown -= Time.fixedDeltaTime * AttackSpeed;
        if (curAttackCooldown <= 0)
            TryAttack();

    }

    private void TryAttack()
    {
        switch (data.playerClass)
        {
            case PlayerClass.WARRIOR:
                if (nearestEnemies.Count == 0)
                    return;
                if (target == null || (target.transform.position - transform.position).magnitude > weaponEq.range)
                {
                    target = FindNearestUnit(nearestEnemies);
                    AttackMelee();
                }
                else
                {
                    AttackMelee();
                }
                break;
            case PlayerClass.RANGER: // Fon now just attack vehicles, but has to be added additional logic, if some enemies are really close
                if (nearestVehicles.Count == 0)
                    return;
                if (target == null || (target.transform.position - transform.position).magnitude > weaponEq.range)
                {
                    target = FindNearestUnit(nearestVehicles);
                    AttackRanged();
                }
                else
                {
                    AttackRanged();
                }
                break;
            case PlayerClass.INGENIEUR: // The same with Ranger, but also adjust for mothers arm
                if (nearestVehicles.Count == 0)
                    return;
                if (target == null || (target.transform.position - transform.position).magnitude > weaponEq.range)
                {
                    target = FindNearestUnit(nearestVehicles);
                    AttackRanged();
                }
                else
                {
                    AttackRanged();
                }
                break;
        }
    }

    private void AttackMelee()
    {
        if (weaponEq.canDealSplashDamage)
        {
            // TODO: Add delay and visual interpretation of hit. For noe just do it instantly
            Collider2D[] hits = Physics2D.OverlapCircleAll((Vector2)transform.position, weaponEq.range);
            Vector2 direction = (target.transform.position - transform.position).normalized;
            foreach (var hit in hits)
            {
                Vector3 toTarget = hit.transform.position - transform.position;
                toTarget.z = 0;

                float angle = Vector3.Angle(direction, toTarget.normalized);
                if (angle <= weaponEq.fov / 2f)
                {
                    if (hit.TryGetComponent(out EnemyCombatBehaviourSystem enemy))
                    {
                        enemy.Hurt(CalculateDamage(weaponEq.damage), this);
                    }
                }
            }
        }
        else
        {
            // TODO: Add delay and visual interpretation of hit. For noe just do it instantly
            target.Hurt(CalculateDamage(weaponEq.damage), this);
        }
        curAttackCooldown = weaponEq.cooldown;
    }
    private void AttackRanged()
    {
        var projectile = Instantiate(weaponEq.projectile);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().SetTarget(target.transform, this, CalculateDamage(weaponEq.damage), unitTypesEnemyAndVehicle);
        // Later on make logic for whict type of enemies to attack

        curAttackCooldown = weaponEq.cooldown;
    }
    void OnTriggerEnter2D(Collider2D collision) // Can get wrong if the were in the radius from start on?
    {
        var unit = collision.gameObject.GetComponent<UnitController>();
        if (unit != null)
        {
            unit.OnDieEvent += UnitInRangeDied;
            if (unit is PlayerCombatSystem otherPlayer)
            {
                nearestPlayers.Add(otherPlayer);
                // TODO: For mothers arm and other interaction with players
            }
            if (unit is EnemyCombatBehaviourSystem enemy)
            {
                nearestEnemies.Add(enemy);
            }
            if (unit is VehicleCombatBehaviourSystem vehicle)
            {
                nearestVehicles.Add(vehicle);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        var unit = collision.gameObject.GetComponent<UnitController>();
        if (unit != null)
        {
            RemoveUnitFromNearestList(unit);
        }
    }

    private void UnitInRangeDied(UnitController unit)
    {
        RemoveUnitFromNearestList(unit);
    }

    private void RemoveUnitFromNearestList(UnitController unit)
    {
        unit.OnDieEvent -= UnitInRangeDied;
        if (unit is PlayerCombatSystem otherPlayer)
        {
            nearestPlayers.Remove(otherPlayer);
            // TODO: For mothers arm and other interaction with players
        }
        if (unit is EnemyCombatBehaviourSystem enemy)
        {
            nearestEnemies.Remove(enemy);
        }
        if (unit is VehicleCombatBehaviourSystem vehicle)
        {
            nearestVehicles.Remove(vehicle);
        }
    }

    private UnitController FindNearestUnit<T>(List<T> units) where T : UnitController
    {
        float curDistance = float.MaxValue;
        UnitController choseEnemy = null;
        foreach (var unit in units)
        {
            float distance = (unit.transform.position - transform.position).magnitude;
            if (distance < curDistance)
            {
                curDistance = distance;
                choseEnemy = unit;
            }
        }
        if (choseEnemy == null)
        {
            return null;
        }
        return choseEnemy;
    }
    protected override void Die()
    {
        throw new System.NotImplementedException();
    }

    private float CalculateDamage(float damage)
    {
        return damage * ((100f + damage) / 100f);
    }

    public UnitController GetCurrentTargetSelected()
    {
        return target;
    }
}
