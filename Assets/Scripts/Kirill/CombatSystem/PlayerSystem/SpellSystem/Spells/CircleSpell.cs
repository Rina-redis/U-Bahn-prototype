using UnityEngine;

public class CircleSpell : ActiveSpell
{
    [SerializeField] private CircleSpellData data;

    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end) // end is not used
    {
        CircleSpellExecutor executor = player.gameObject.AddComponent<CircleSpellExecutor>();
        executor.Initialize(data, start, player);
    }
}
