using UnityEngine;
using System;
using System.Collections.Generic;

public class AttackingRangedBehaviourController : CombatBehaviourController
{
    [SerializeField] private EnemyRangedAttackData data;

    private Transform target;
    private PlayerMock chaisedPlayer;
    private float curAttackCooldown = 0f;

    List<UnitType> unitTypes = new List<UnitType>();

    public void SetTarget(PlayerMock player)
    {
        chaisedPlayer = player;
        target = chaisedPlayer.gameObject.transform;
        player.onDieEvent += OnPlayerKilled;

        if (unitTypes.Count == 0)
            unitTypes.Add(UnitType.PLAYER);
    }

    //---// Behaviour //---//
    public override void OnStartBehaviour()
    {
        if (target == null || chaisedPlayer == null)
        {
            Debug.LogError("I dont have a target to attack");
            return;
        }
    }

    public override void OnFixedUpdateBehave()
    {
        curAttackCooldown -= Time.fixedDeltaTime * Controller.AttackSpeed;

        if (curAttackCooldown <= 0)
        {
            Attack();
        }
    }

    private void Attack()
    {
        var projectile = Instantiate(data._projectile);
        projectile.transform.position = transform.position;

        projectile.GetComponent<Projectile>().SetTarget((target.position - transform.position).normalized, Controller, CalculateDamage(data.damage), unitTypes);
        curAttackCooldown = 1f;
    }

    private void OnPlayerKilled(System.Object obj, EventArgs e)
    {
        LoseTarget();
    }

    private void LoseTarget()
    {
        chaisedPlayer.onDieEvent -= OnPlayerKilled;
        chaisedPlayer = null;
        target = null;
        Controller.RangedLoseTarget();
    }

    public override void OnEndBehaviour()
    {
        chaisedPlayer = null;
        target = null;
        curAttackCooldown = 1f;
    }

    private float CalculateDamage(float damage) {
        return damage * ((100f + damage) / 100f);
    }
}
