using UnityEngine;

public class SpellEffect : Effect
{
    public ActiveSpell spell;

    public override void ApplyEffect(PlayerCombatSystem player, Transform start)
    {
        spell.Activate(player, start, Vector2.zero); // Can go wrong, if wrong spells used.
    }
}
