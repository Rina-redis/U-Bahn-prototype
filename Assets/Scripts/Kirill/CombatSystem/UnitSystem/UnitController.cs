using System;
using Fusion;
using UnityEngine;

public abstract class UnitController : NetworkBehaviour
{
    public abstract UnitData UnitData { get; }
    public event Action<UnitController> OnDieEvent;

    [Networked]
    private float health { get; set; }
    [Networked]
    private float armor { get; set; }
    [Networked]
    private float strength { get; set; }
    [Networked]
    private float speed { get; set; }
    [Networked]
    private float attackSpeed { get; set; }
    [Networked]
    private float armorPenetration { get; set; }

    public float Health { get => health; }
    public float Armor { get => armor; }
    public float Strength { get => strength; }
    public float Speed { get => speed * (speedMultiplex <= 0.1 ? 0.1f : speedMultiplex); }
    public float AttackSpeed { get => attackSpeed * (attackSpeedMultiplex <= 0.1 ? 0.1f : attackSpeedMultiplex); }
    public float ArmorPenetration { get => armorPenetration >= 90f ? 90f : armorPenetration; }

    private float speedMultiplex;
    private float attackSpeedMultiplex;
    protected void Init()
    {
        health = 0;
        armor = 0;
        strength = 0;
        speed = 0;
        attackSpeed = 0;
        armorPenetration = 0;

        speedMultiplex = 1f;
        attackSpeedMultiplex = 1f;

        ApplyUnitDataStats(UnitData);
    }

    protected void ApplyUnitDataStats(UnitData unitData)
    {
        Debug.Log("attack speed in giver UnitData: " + unitData.attackSpeed);
        Debug.Log("attack speed multiplex: " + speedMultiplex);
        Debug.Log("attack speed just: " + attackSpeed);
        Debug.Log("attack speed calculated: " + AttackSpeed);

        health += unitData.health;
        armor += unitData.armor;
        strength += unitData.strength;
        speed += unitData.speed;
        attackSpeed += unitData.attackSpeed;
        armorPenetration += unitData.armorPenetration;

        Debug.Log("Attack Speed after adding just: " + attackSpeed);
        Debug.Log("Current attack speed:" + AttackSpeed);
    }

    public void ApplyStatusEffect(StatusEffect statusEffect)
    {
        // Applying effect
        switch (statusEffect.paramToEffect)
        {
            case UnitParams.HEALTH:
                health += statusEffect.value;
                if (health <= 0)
                {
                    health = 0;
                    Die();
                }
                break;
            case UnitParams.ARMOR:
                armor += statusEffect.value;
                break;
            case UnitParams.ARMOR_PENETRATION:
                armorPenetration += statusEffect.value;
                break;
            case UnitParams.STRENGTH:
                strength += statusEffect.value;
                break;
            case UnitParams.SPEED:
                speedMultiplex += statusEffect.value;
                break;
            case UnitParams.ATTACK_SPEED:
                attackSpeedMultiplex += statusEffect.value;
                break;
        }
    }

    public void RemoveStatusEffect(StatusEffect statusEffect)
    {
        Debug.Log("I remove effect: " + statusEffect);
        // Reverting application of an effect
        switch (statusEffect.paramToEffect)
        {
            case UnitParams.HEALTH:
                health -= statusEffect.value;
                if (health <= 0)
                {
                    health = 0;
                    Die();
                }
                break;
            case UnitParams.ARMOR:
                armor -= statusEffect.value;
                break;
            case UnitParams.ARMOR_PENETRATION:
                armorPenetration -= statusEffect.value;
                break;
            case UnitParams.STRENGTH:
                strength -= statusEffect.value;
                break;
            case UnitParams.SPEED:
                speedMultiplex -= statusEffect.value;
                break;
            case UnitParams.ATTACK_SPEED:
                attackSpeedMultiplex -= statusEffect.value;
                break;
        }
    }

    public virtual void Hurt(float damage, UnitController attacker)
    {
        Debug.Log("Unit was hurt " + gameObject + " Damage: " + damage + " penetration: " + attacker.ArmorPenetration);
        Debug.Log("Health before: " + Health);
        health -= damage * (100f / (100f + this.armor - attacker.ArmorPenetration));
        Debug.Log("Health after: " + Health);
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    protected abstract void Die(); // In all Fällen OnDieEvent shooten

    protected void TriggerDeathEvent()
    {
        OnDieEvent?.Invoke(this);
    }
}
