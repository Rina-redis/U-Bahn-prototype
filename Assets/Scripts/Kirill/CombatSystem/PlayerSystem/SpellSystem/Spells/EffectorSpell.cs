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
                statusEffect.ApplyEfect(start);
            }
        }
        // TODO: if apply effects on others, like areas, etc.
    }
}
