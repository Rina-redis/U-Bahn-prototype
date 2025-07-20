using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInventoryController : MonoBehaviour
{
    private PlayerInventory inventory;

    [SerializeField] private UIInventoryStash stash;
    [SerializeField] public UIItemSlot UIarmor;
    [SerializeField] public UIItemSlot UIweapon;
    [SerializeField] public UIConsumableSlots UIConsumable;
    [SerializeField] private TMP_Dropdown dropdown;

    private UIInventorySlector selector;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        inventory = PlayerInventory.instance;
        inventory.InventoryChanged.AddListener(Rebuild);
        selector = GetComponent<UIInventorySlector>();

    }

    public void SetInventory(PlayerInventory pInv)
    {
        inventory = pInv;
    }

    public void RebuildWeapon()
    {
        stash.ShowWeaponOptions(inventory.GetInventory());
    }
    public void RebuildArmor()
    {
        stash.ShowArmorOptions(inventory.GetInventory());
    }

    public void Rebuild()
    {
        switch (selector.state)
        {
            case UIInventorySlector.SelectorState.SelectingArmor:
                stash.ShowArmorOptions(inventory.GetInventory().Where(item => item.GetSample().itemClass == inventory.GetClass() || item.GetSample().itemClass == PlayerInventory.PlayerClass.None).ToList());
                break;
            case UIInventorySlector.SelectorState.SelectingWeapon:
                stash.ShowWeaponOptions(inventory.GetInventory().Where(item => item.GetSample().itemClass == inventory.GetClass() || item.GetSample().itemClass == PlayerInventory.PlayerClass.None).ToList());
                break;
            case UIInventorySlector.SelectorState.SelectingConsumable:
                stash.ShowConsumableOptions(inventory.GetInventory().Where(item => item.GetSample().itemClass == inventory.GetClass() || item.GetSample().itemClass == PlayerInventory.PlayerClass.None).ToList());
                break;
            default:
                stash.Rebuild(inventory.GetInventory());
                break;
        }
        UIarmor.RefreshSlot(inventory.GetCurrentArmor());
        UIweapon.RefreshSlot(inventory.GetCurrentWeapon());
        UIConsumable.Rebuild(inventory.GetActiveConsumables());
        dropdown.value = (int)inventory.GetClass() - 1;
        dropdown.RefreshShownValue();
    }

    public void ChangeClass(int newClass){
        inventory.ChangeClass((PlayerInventory.PlayerClass)(newClass + 1));
    }
    

}
