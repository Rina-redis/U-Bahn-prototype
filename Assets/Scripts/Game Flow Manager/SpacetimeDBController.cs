using System.Threading.Tasks;
using SpacetimeDB.Types;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class SpacetimeDBController : MonoBehaviour
{
    public static SpacetimeDBController instance;
    public bool isInit { get; private set; } = false;
    public string Nickname { get; private set; }
    DbManager dbService;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);

    }

    public void initDB(string nickname)
    {
        dbService = new DbManager(nickname);
        isInit = true;
    }

    private void DbNotInit()
    {
        if (!isInit)
        {
            Debug.LogError("Db is not init. Stop calling it");
        }
    }

    public async Task<PlayerInventory.PlayerClass> GetPlayerClass()
    {
        if (!isInit)
        {
            DbNotInit();
        }
        string pc = await dbService.get_player_class();
        switch (pc)
        {
            case "Warrior":
                return PlayerInventory.PlayerClass.Warrior;
            case "Ranger":
                return PlayerInventory.PlayerClass.Ranger;
            case "Ingeneer":
                return PlayerInventory.PlayerClass.Ingeniur;
            default:
                return PlayerInventory.PlayerClass.Warrior;
        }
    }

    public async void SetPlayersClass(PlayerInventory.PlayerClass pc)
    {
        if (!isInit)
        {
            Debug.LogError("Db is not init. Stop calling it");
        }        switch (pc)
        {
            case PlayerInventory.PlayerClass.Warrior:
                await dbService.set_player_class("Warrior");
                break;
            case PlayerInventory.PlayerClass.Ranger:
                await dbService.set_player_class("Ranger");
                break;
            case PlayerInventory.PlayerClass.Ingeniur:
                await dbService.set_player_class("Ingeneer");
                break;
            default:
                await dbService.set_player_class("Warrior");
                break;
        }
    }

    public async Task<int> GetPlayerMoney()
    {
        if (!isInit)
        {
            Debug.LogError("Db is not init. Stop calling it");
        }
        return (int)await dbService.get_player_money();
    }

    public async void SetPlayerMoney(int pm)
    {
        if (!isInit)
        {
            DbNotInit();
        }
        await dbService.set_player_money((uint)pm);
    }

    public async Task<ScriptableWeapon> GetCurrentWeapon()
    {
        if (!isInit)
        {
            DbNotInit();
        }
        int id = (int)await dbService.get_weapon_id();
        ScriptableItemBase weapon;
        if (id == 0)
        {
            weapon = ItemManager.instance.emptyWeapon;
        }
        else
        {
            weapon = ItemManager.instance.getItem(id);
            if (weapon == null || weapon is not ScriptableWeapon)
            {
                Debug.LogError("Some shit in db weapon slot");
                return ItemManager.instance.emptyWeapon;
            }
        }
        return (ScriptableWeapon)weapon;
    }
    public async void SetCurrentWeapon(ScriptableWeapon weapon)
    {
        if (!isInit)
        {
            DbNotInit();
        }        await dbService.set_weapon((uint)weapon.id);
    }

    public async Task<ScriptableArmor> GetCurrentArmor()
    {
        if (!isInit)
        {
            DbNotInit();
        }
        int id = (int)await dbService.get_armor_id();
        ScriptableItemBase armor;
        if (id == 0)
        {
            armor = ItemManager.instance.emptyArmor;
        }
        else
        {
            armor = ItemManager.instance.getItem(id);
            if (armor == null || armor is not ScriptableArmor)
            {
                Debug.LogError("Some shit in db weapon slot");
                return ItemManager.instance.emptyArmor;
            }
        }
        return (ScriptableArmor)armor;
    }
    public async void SetCurrentArmor(ScriptableArmor armor)
    {
        if (!isInit)
        {
            DbNotInit();
        }        await dbService.set_armor((uint)armor.id);
    }

    public async Task<List<InventorySlot>> GetActiveConsumables()
    {
        if (!isInit)
        {
            DbNotInit();
        }        var consumablesDb = await dbService.get_consumables();
        List<InventorySlot> slots = new List<InventorySlot>();
        foreach (Item item in consumablesDb)
        {
            ScriptableItemBase consumableItem;
            if (item.Id == 0)
            {
                consumableItem = ItemManager.instance.emptyItem;
            }
            else
            {
                consumableItem = ItemManager.instance.getItem((int)item.Id);
                if (consumableItem == null || consumableItem is not ScriptableConsumable)
                {
                    Debug.LogError("Some shit in db in one of consumable slots");
                    return new List<InventorySlot>();
                }
            }
            InventorySlot slot = new InventorySlot(consumableItem);
            for (int i = 0; i < item.Quantity; ++i)
            {
                slot.AddItem(consumableItem);
            }
            slots.Add(slot);
        }
        return slots;
    }

    public async void SetActiveConsumables(List<InventorySlot> slots)
    {
        if (!isInit)
        {
            DbNotInit();
        }        List<Item> items = new List<Item>();
        foreach (InventorySlot slot in slots)
        {
            Item item = new Item();
            item.Id = (uint)slot.GetSample().id;
            item.Quantity = (uint)slot.Count;
            items.Add(item);
        }
        await dbService.set_consumables(items);
    }

    public async Task<List<InventorySlot>> GetInventory()
    {
        if (!isInit)
        {
            DbNotInit();
        }        var inventoryDb = await dbService.get_inventory();
        List<InventorySlot> slots = new List<InventorySlot>();
        foreach (Item item in inventoryDb)
        {
            ScriptableItemBase scriptableItem;
            if (item.Id == 0)
            {
                scriptableItem = ItemManager.instance.emptyItem;
            }
            else
            {
                scriptableItem = ItemManager.instance.getItem((int)item.Id);
                if (scriptableItem == null)
                {
                    Debug.LogError("Some shit in db in one of inventory slots");
                    return new List<InventorySlot>();
                }
            }
            InventorySlot slot = new InventorySlot(scriptableItem);
            for (int i = 0; i < item.Quantity; ++i)
            {
                slot.AddItem(scriptableItem);
            }
            slots.Add(slot);
        }
        return slots;
    }

    public async void SetInventory(List<InventorySlot> slots)
    {
        if (!isInit)
        {
            DbNotInit();
        }        List<Item> items = new List<Item>();
        foreach (InventorySlot slot in slots)
        {
            Item item = new Item();
            item.Id = (uint)slot.GetSample().id;
            item.Quantity = (uint)slot.Count;
            items.Add(item);
        }
        await dbService.set_inventory(items);
    }
}
