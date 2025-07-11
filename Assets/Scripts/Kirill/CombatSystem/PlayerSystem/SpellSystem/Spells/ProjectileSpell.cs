using UnityEngine;


[CreateAssetMenu(fileName = "ProjectileSpell", menuName = "Scriptable Objects/ProjectileSpell")]
public class ProjectileSpell : ActiveSpell
{
    [SerializeField] private ProjectileSpellData data;
    protected override void Execute(PlayerMock playerMock, Transform start, Vector2 end)
    {
        ProjectileSpellExecutor executor = playerMock.gameObject.AddComponent<ProjectileSpellExecutor>();
        if (data.targetType == TargetType.CURRENT_TARGET)
        {
            executor.Initialize(data, start, playerMock.GetCurrentTargetSelected());
        }
        else
        {
            executor.Initialize(data, start, end);
        }
    }
}
