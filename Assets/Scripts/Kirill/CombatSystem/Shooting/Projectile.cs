using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileData data;
    Transform target;
    Vector2 flyingDirection;
    bool isFlying = false;
    UnitController attacker;
    float damage;
    public void SetTarget(Transform target, UnitController attacker, float damage)
    {
        if (data.targetType != TargetType.CURRENT_TARGET)
        {
            Debug.LogError("Projectile has wrong target type, or the wrong function was called");
            return;
        }
        this.target = target;
        this.attacker = attacker;
        this.damage = damage;
        UpdateFlyingDirection(target.position);

        isFlying = true;
    }

    public void SetTarget(Vector2 positionOrDirection, UnitController attacker, float damage)
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
            if (attacker.UnitData.unitType == unit.UnitData.unitType)
                return;
            Debug.Log("I am attacking");
            unit.Hurt(damage, attacker);
            DestroyGO();
        }
    }

    private void DestroyGO()
    {
        Destroy(gameObject);
    }
}
