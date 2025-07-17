using UnityEngine;

public class ProjectileSpell : ActiveSpell
{
    [SerializeField] private ProjectileSpellData data;

    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end)
    {
        ProjectileSpellExecutor executor = player.gameObject.AddComponent<ProjectileSpellExecutor>();
        if (data.targetType == TargetType.CURRENT_TARGET)
        {
            executor.Initialize(data, start, player.GetCurrentTargetSelected().transform, player);
        }
        else
        {
            executor.Initialize(data, start, end, player);
        }
    }
}
