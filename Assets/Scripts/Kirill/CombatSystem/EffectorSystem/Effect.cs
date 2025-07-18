using UnityEngine;

[CreateAssetMenu(fileName = "Effect", menuName = "Scriptable Objects/Effect")]
public abstract class Effect : ScriptableObject
{
    public abstract void ApplyEffect(PlayerCombatSystem player, Transform start);
}
