using System.Collections;
using UnityEngine;

public class CircleSpellExecutor : MonoBehaviour
{
    private CircleSpellData data;
    private Transform castTransform;
    private PlayerCombatSystem player;

    public void Initialize(CircleSpellData spellData, Transform castTransform, PlayerCombatSystem player)
    {
        data = spellData;
        this.player = player;
        this.castTransform = castTransform;
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
        Collider[] hits = Physics.OverlapSphere(castTransform.position, data.radius, LayerMask.GetMask("Enemy"));

        foreach (Collider hit in hits)
        {
            Debug.Log("I hit an enemy with circle: " + hit.gameObject);
            if (hit.TryGetComponent(out UnitController unit))
            {
                if (unit is PlayerCombatSystem)
                    return;
                Debug.Log("I am hitting an enemy");
                unit.Hurt(CalculateDamage(data.damageProExecution), player);
            }
        }
    }

    private float CalculateDamage(float damage)
    {
        return damage * ((100f + damage) / 100f);
    }
}
