using UnityEngine;

public abstract class ActiveSpell : Spell
{
    public Reload Reload { get => reload; }
    [SerializeField] private Reload reload;
    public void Activate(PlayerCombatSystem player, Transform start, Vector2 end)
    {
        if (!reload.IsReady())
            return;
        Execute(player, start, end);
        reload.SpellWasUsed();
    }

    protected abstract void Execute(PlayerCombatSystem player, Transform start, Vector2 end);

}
