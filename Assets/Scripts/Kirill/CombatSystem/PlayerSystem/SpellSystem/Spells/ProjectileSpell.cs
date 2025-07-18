using UnityEngine;

public class ProjectileSpell : ActiveSpell
{
    [SerializeField] private ProjectileSpellData data;
    [SerializeField] private GameObject _projectileSpellExecutor;

    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end)
    {
        GameObject go = Instantiate(_projectileSpellExecutor);
        go.transform.parent = transform;
        go.transform.localPosition = Vector2.zero;
        go.transform.up = (end - (Vector2)start.position).normalized;

        ProjectileSpellExecutor executor = go.GetComponent<ProjectileSpellExecutor>();

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
