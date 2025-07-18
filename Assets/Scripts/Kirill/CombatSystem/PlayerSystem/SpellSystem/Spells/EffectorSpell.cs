using UnityEngine;

public class EffectorSpell : ActiveSpell
{
    [SerializeField] private EffectorSpellData data;
    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end)
    {
        if (data.type == EffectorSpellType.SELF)
        {
            foreach (StatusEffect statusEffect in data.statusEffects)
            {
                statusEffect.ApplyEffect(start);
            }
        }
        // TODO: if apply effects on others, like areas, etc.
        else if (data.type == EffectorSpellType.TARGET)
        {
            foreach (StatusEffect statusEffect in data.statusEffects)
            {
                var target = player.GetCurrentTargetSelected();
                if (target == null)
                    return;
                statusEffect.ApplyEffect(target.transform);
            }
        }
    }
}
