using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileData data;
    private Transform target;
    private Vector2 flyingDirection;
    private bool isFlying = false;
    private UnitController attacker;
    private float damage;
    private List<UnitType> targetTypes;

    float distanceTravelled;
    public void SetTarget(Transform target, UnitController attacker, float damage, List<UnitType> targetTypes)
    {
        if (data.targetType != TargetType.CURRENT_TARGET)
        {
            Debug.LogError("Projectile has wrong target type, or the wrong function was called");
            return;
        }

        this.target = target;
        this.attacker = attacker;
        this.damage = damage;
        this.targetTypes = targetTypes;
        UpdateFlyingDirection(target.position);

        isFlying = true;
    }

    public void SetTarget(Vector2 positionOrDirection, UnitController attacker, float damage, List<UnitType> targetTypes)
    {
        if (data.targetType == TargetType.CURRENT_TARGET)
        {
            Debug.LogError("Projectile has wrong target type, or the wrong function was called");
            return;
        }
        if (data.targetType == TargetType.DIRECTION)
        {
            flyingDirection = positionOrDirection;
        }
        else
        {
            UpdateFlyingDirection(positionOrDirection);
        }
        this.attacker = attacker;
        this.damage = damage;
        this.targetTypes = targetTypes;
        isFlying = true;
    }

    void FixedUpdate()
    {
        if (isFlying)
        {
            if (data.targetType == TargetType.CURRENT_TARGET)
            {
                UpdateFlyingDirection(target.position);
            }
            transform.up = new Vector3(flyingDirection.x, flyingDirection.y, 0);
            transform.Translate(Vector2.up * data.speed * Time.fixedDeltaTime);
            distanceTravelled += (Vector2.up * data.speed * Time.fixedDeltaTime).magnitude;

            //TODO: If certain point was achieved, 
            if (data.targetType == TargetType.DESTINATION_POINT)
            {
                if ((transform.position - target.transform.position).magnitude <= 0.01f)
                {
                    OnEndFlight();
                }
            }
        }
    }

    void UpdateFlyingDirection(Vector2 position)
    {
        flyingDirection = (position - (Vector2)transform.position).normalized;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Projectule sensed someone: " + collision.gameObject);
        if (data.hitType == HitType.NOBODY)
            return;
        collision.gameObject.TryGetComponent<UnitController>(out UnitController unit);
        if (unit != null)
        {
            Debug.Log("I can attack someone, but will I do it?");
            if (!targetTypes.Contains(unit.UnitData.unitType))
                return;
            if (data.hitType == HitType.TARGET && collision.transform != target)
                return;

            Debug.Log("I am attacking");
            //ApplyEffects(unit.transform);
            unit.Hurt(damage, attacker);
            
            if (!(data.hitType == HitType.ALL))
                OnEndFlight();
        }
    }

    private void ApplyEffects(Transform targ)
    {
        foreach (Effect effect in data.effects)
        {
            effect.ApplyEffect(attacker as PlayerCombatSystem, targ); // !!! This code is very bad. attack can be not Player and target can be null.
        }
    }

    private void OnEndFlight()
    {
        ApplyEffects(target);
        Destroy(gameObject);
    }
}
