using System.Collections;
using UnityEngine;

public class SplashSpellExecutor : MonoBehaviour
{
    private SplashSpellData data;
    private Transform castTransform;
    private Vector2 direction;
    private PlayerCombatSystem player;

    public void Initialize(SplashSpellData spellData, PlayerCombatSystem player, Transform castTransform, Vector2 castedPoint)
    {
        data = spellData;
        this.castTransform = castTransform;
        direction = castedPoint - (Vector2)castTransform.position;
        this.player = player;
        StartCoroutine(ExecuteSpell());
    }

    private IEnumerator ExecuteSpell()
    {
        yield return new WaitForSeconds(data.executionDelay);

        float interval = data.executionTime / data.executionAmount;

        for (int i = 0; i < data.executionAmount; i++)
        {
            DealDamage();
            yield return new WaitForSeconds(interval);
        }

        Destroy(this);
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(castTransform.position, data.radius);
        foreach (var hit in hits)
        {
            Vector3 toTarget = (hit.transform.position - castTransform.position);
            toTarget.z = 0;

            float angle = Vector3.Angle(direction, toTarget.normalized);
            if (angle <= data.fov / 2f)
            {
                Debug.Log("I hit the enemy with splash: " + hit.gameObject);
                if (hit.TryGetComponent(out UnitController unit))
                {
                    if (unit is PlayerCombatSystem)
                        return;
                    Debug.Log("I am hitting an enemy");
                    unit.Hurt(CalculateDamage(data.damageProExecution), player);
                }
            }
        }
    }

    private float CalculateDamage(float damage)
    {
        return damage * ((100f + damage) / 100f);
    }
}
