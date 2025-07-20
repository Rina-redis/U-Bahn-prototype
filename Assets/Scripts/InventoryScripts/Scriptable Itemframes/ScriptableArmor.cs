using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableItem", menuName = "Scriptable Objects/ScriptableArmor")]

public class ScriptableArmor : ScriptableItemBase
{
    public float additionalHealth;
    public float armor;
    public Sprite PlayerSprite;
    
    public UnitData unitData;


    public void Use(GameObject player)
    {
        Debug.Log(player.name + " is equipped with " + name + " and it looks cool as " + armor);
    }
}
