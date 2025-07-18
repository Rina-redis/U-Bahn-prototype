using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "Scriptable Objects/StatusEffect")]
public class StatusEffect : Effect
{
    public UnitParams paramToEffect; // All speeds are effects multiplex (1.0f for 100%), all other just linear values.
    public float value;
    public bool hasDuration;
    public float duration;
    public bool hasEffectRadius;
    public float effectRadius;
    public bool isPermanent;

    public override void ApplyEffect(PlayerCombatSystem player, Transform start)
    {
        var executor = start.gameObject.AddComponent<StatusEffectExecutor>();
        executor.Init(this);
    }
}
