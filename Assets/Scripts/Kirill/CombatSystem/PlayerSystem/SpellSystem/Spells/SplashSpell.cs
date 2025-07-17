using UnityEngine;

public class SplashSpell : ActiveSpell
{
    [SerializeField] private SplashSpellData data;
    [SerializeField] private GameObject _splashSpellExecutor;

    public override SpellData SpellData => data;

    protected override void Execute(PlayerCombatSystem player, Transform start, Vector2 end)
    {
        GameObject go = Instantiate(_splashSpellExecutor);
        go.transform.parent = transform;
        go.transform.localPosition = Vector2.zero;
        go.transform.up = (end - (Vector2)start.position).normalized;

        SplashSpellExecutor executor = go.GetComponent<SplashSpellExecutor>();
        executor.Initialize(data, player, start, end);
    }
}
