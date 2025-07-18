using UnityEngine;

public class CircleSpell : ActiveSpell
{
    [SerializeField] private CircleSpellData data;
    [SerializeField] private GameObject _circleSpellExecutor;

    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end) // end is not used
    {
        GameObject go = Instantiate(_circleSpellExecutor);
        go.transform.parent = transform;
        go.transform.localPosition = Vector2.zero;
        go.transform.up = (end - (Vector2)start.position).normalized;

        CircleSpellExecutor executor = go.GetComponent<CircleSpellExecutor>();
        executor.Initialize(data, start, player);
    }
}
