using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory instance;
    [HideInInspector] public UnityEvent InventoryChanged;
    [HideInInspector] public UnityEvent EquipmentChanged;
    [HideInInspector] public UnityEvent MoneyChanged;

    [SerializeField] private int playerMoney = 0;


    private List<InventorySlot> inventoryStash = new List<InventorySlot>();
    [SerializeField] private ScriptableArmor currentArmor;
    [SerializeField] private ScriptableWeapon currentWeapon;
    [SerializeField] private int maxActiveConsumables;
    [SerializeField] private InventorySlot[] activeConsumables;

    [SerializeField] private PlayerClass currentClass = PlayerClass.Warrior;
    [SerializeField] private string nickname = "Roflopafl";

    [SerializeField] private PlayerCombatSystemData ingeneerData;
    [SerializeField] private PlayerCombatSystemData RangerData;
    [SerializeField] private PlayerCombatSystemData WarriorData;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        // DontDestroyOnLoad(this);
        if (maxActiveConsumables == 0)
        {
            maxActiveConsumables = 1;
        }
        activeConsumables = new InventorySlot[maxActiveConsumables];
    }

    void Start()
    {
        for (int i = 0; i < maxActiveConsumables; ++i)
        {
            if (activeConsumables[i] == null)
            {
                activeConsumables[i] = new InventorySlot(ItemManager.instance.emptyItem);
            }
        }
        if (currentArmor == null)
        {
            currentArmor = ItemManager.instance.emptyArmor;
        }
        if (currentWeapon == null)
        {
            currentWeapon = ItemManager.instance.emptyWeapon;
        }
        
        //LoadInventory();
        //EquipmentChanged.Invoke();
    }

    public PlayerInventory AddItem(ScriptableItemBase item)
    {
        foreach (InventorySlot slot in inventoryStash)
        {
            if (slot.GetSample().name == item.name && slot.GetSample().maxStackSize > slot.Count)
            {
                slot.AddItem(item);
                InventoryChanged.Invoke();
                return this;
            }
        }
        inventoryStash.Add(new InventorySlot(item).AddItem(item));
        SpacetimeDBController.instance.SetInventory(inventoryStash);
        InventoryChanged.Invoke();
        return this;
    }

    private async void LoadInventory()
    {

        var db = SpacetimeDBController.instance;
        currentArmor = await db.GetCurrentArmor();
        currentWeapon = await db.GetCurrentWeapon();
        var consumables = await db.GetActiveConsumables();
        for (int i = 0; i < activeConsumables.Length&&i<consumables.Count; ++i)
        {
            activeConsumables[i] = consumables[i];
        }
        inventoryStash = await db.GetInventory();
        playerMoney = await db.GetPlayerMoney();
        ChangeClass(await db.GetPlayerClass());
        // Loading inventory from Server
        InventoryChanged.Invoke();
    }

    public PlayerInventory RemoveItem(ScriptableItemBase item)
    {
        foreach (InventorySlot slot in inventoryStash)
        {
            if (slot.GetSample().name == item.name)
            {
                slot.RemoveItem();
                if (slot.Count == 0)
                {
                    inventoryStash.Remove(slot);
                }
                SpacetimeDBController.instance.SetInventory(inventoryStash);
                InventoryChanged.Invoke();
                return this;
            }
        }
        Debug.LogError("Removing nonexisitng item");
        return this;
    }

    public PlayerInventory RemoveSlot(InventorySlot slot)
    {
        inventoryStash.Remove(slot);
        SpacetimeDBController.instance.SetInventory(inventoryStash);

        return this;
    }

    public bool Contains(ScriptableItemBase item)
    {
        foreach (InventorySlot slot in inventoryStash)
        {
            if (slot.GetSample().name == item.name)
            {
                return true;
            }
        }
        return false;
    }

    public void SwapOutCurrentArmor(ScriptableItemBase armor)
    {
        if (currentArmor is not ScriptableArmor)
        {
            Debug.LogError("Bro is trying to wear not currentArmor and dies from cringe");
            return;
        }
        if (currentArmor != ItemManager.instance.emptyArmor)
        {
            AddItem(currentArmor);
        }
        RemoveItem(armor);
        currentArmor = (ScriptableArmor)armor;
        SpacetimeDBController.instance.SetCurrentArmor(currentArmor);
        InventoryChanged.Invoke();
        EquipmentChanged.Invoke();
    }

    public void SwapOutCurrentWeapon(ScriptableItemBase weapon)
    {
        if (weapon is not ScriptableWeapon)
        {
            Debug.LogError("Bro is trying to equip not a weapon and dies from cringe");
            return;
        }
        if (currentWeapon != ItemManager.instance.emptyWeapon)
        {
            AddItem(currentWeapon);
        }
        RemoveItem(weapon);
        currentWeapon = (ScriptableWeapon)weapon;
        SpacetimeDBController.instance.SetCurrentWeapon(currentWeapon);
        InventoryChanged.Invoke();
        EquipmentChanged.Invoke();
    }

    public PlayerInventory AddSlot(InventorySlot slot)
    {
        inventoryStash.Add(slot);
        SpacetimeDBController.instance.SetInventory(inventoryStash);
        return this;
    }

    public void AddToActiveCosumables(InventorySlot slot, int slotIndex)
    {
        if (slot.GetSample() is not ScriptableConsumable)
        {
            return;
        }
        if (activeConsumables[slotIndex].GetSample() != ItemManager.instance.emptyItem && activeConsumables[slotIndex].Count != 0)
        {
            inventoryStash.Add(activeConsumables[slotIndex]);
        }
        RemoveSlot(slot);
        activeConsumables[slotIndex] = slot;

        SpacetimeDBController.instance.SetActiveConsumables(activeConsumables.ToList());
        InventoryChanged.Invoke();
        EquipmentChanged.Invoke();
    }

    public List<InventorySlot> GetInventory()
    {
        return inventoryStash;
    }
    public override string ToString()
    {
        StringBuilder bld = new StringBuilder();
        bld.Append("Player has \n");
        foreach (InventorySlot slot in inventoryStash)
        {
            bld.Append(slot).Append("\n");
        }
        return bld.ToString();
    }

    public int GetAmountOf(int id)
    {
        int count = 0;
        foreach (InventorySlot slot in inventoryStash)
        {
            if (slot.GetSample().id == id)
            {
                count += slot.Count;
            }
        }
        return count;
    }


    public ScriptableArmor GetCurrentArmor()
    {
        return currentArmor;
    }

    public ScriptableWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public InventorySlot[] GetActiveConsumables()
    {
        return activeConsumables;
    }

    public int GetMaxActiveConsumables()
    {
        return maxActiveConsumables;
    }

    public string GetNickname()
    {
        return nickname;
    }

    public bool MoneySpend(int price)
    {
        if (price > playerMoney)
        {
            Debug.Log("Sorry Card Declined");
            return false;
        }
        playerMoney -= price;
        SpacetimeDBController.instance.SetPlayerMoney(playerMoney);
        MoneyChanged.Invoke();
        return true;
    }

    public void GainMoney(int gain)
    {
        playerMoney += gain;
        SpacetimeDBController.instance.SetPlayerMoney(playerMoney);
    }

    public int GetMoney()
    {
        return playerMoney;
    }

    public void ChangeClass(PlayerClass newClass)
    {
        if (newClass == PlayerClass.None)
        {
            Debug.LogError("Trying to assign None class");
        }
        if (currentArmor.itemClass != newClass && currentArmor != ItemManager.instance.emptyArmor)
        {
            AddItem(currentArmor);
            currentArmor = ItemManager.instance.emptyArmor;
            //EquipmentChanged.Invoke();
        }
        if (currentWeapon.itemClass != newClass && currentWeapon != ItemManager.instance.emptyWeapon)
        {
            AddItem(currentWeapon);
            currentWeapon = ItemManager.instance.emptyWeapon;
            //EquipmentChanged.Invoke();
        }
        for (int i = 0; i < activeConsumables.Length; ++i)
        {
            if (activeConsumables[i].GetSample().itemClass != newClass && activeConsumables[i].GetSample().itemClass != PlayerClass.None && activeConsumables[i].GetSample() != ItemManager.instance.emptyItem)
            {
                AddSlot(activeConsumables[i]);
                activeConsumables[i] = new InventorySlot(ItemManager.instance.emptyItem);
            }
            //EquipmentChanged.Invoke();
        }
        currentClass = newClass;
        SpacetimeDBController.instance.SetPlayersClass(currentClass);

        InventoryChanged.Invoke();

    }

    public List<InventorySlot> GetFilteredConsumable()
    {
        return activeConsumables.Where(item => item.GetSample() != ItemManager.instance.emptyItem).ToList();
    }

    public PlayerClass GetClass()
    {
        return currentClass;
    }

    public PlayerCombatSystemData GetPlayerCombatSystemData()
    {
        switch (currentClass)
        {
            case PlayerClass.Warrior:
                return WarriorData;
            case PlayerClass.Ranger:
                return RangerData;
            case PlayerClass.Ingeniur:
                return ingeneerData;
        }
        return null;
    }
    
    public enum PlayerClass
    {
        None = 0,
        Warrior = 1,
        Ranger = 2,
        Ingeniur = 3
    }
}
