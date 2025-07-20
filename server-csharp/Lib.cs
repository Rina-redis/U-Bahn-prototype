using SpacetimeDB;
using System;
using System.Collections.Generic;

public static partial class Module
{
    /////////////////////////////////////////////////////////////
    /// Tables

    [Table(Name = "train", Public = true)]
    public partial struct Train
    {
        [PrimaryKey]
        public string id;
        public bool started;
        public string from_station_id;
        public string to_station_id;
        public uint money;
        public List<string> player_ids;
    }

    [Table(Name = "station", Public = true)]
    public partial struct Station
    {
        [PrimaryKey]
        public string mvg_id;
        public string game_id;
        public uint money;
    }

    [Table(Name = "item", Public = true)]
    public partial struct Item
    {
        [PrimaryKey]
        public uint id;
        public uint quantity;
    }

    [Table(Name = "player", Public = true)]
    public partial struct Player
    {
        [PrimaryKey]
        public Identity identity;

        [Unique]
        public string player_id;
        public uint money;
        public string class_name;
        public List<Item> items;
        public List<Item> consumables;
        public uint armor_id;
        public uint weapon_id;
    }

    /////////////////////////////////////////////////////////////
    /// REDUCERS - INIT / CONNECT / LOGIN

    [Reducer(ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        Log.Info($"[Init] Initializing...");
    }

    [Reducer(ReducerKind.ClientConnected)]
    public static void Connect(ReducerContext ctx)
    {
        Log.Info($"[Connect] Client connected: {ctx.Sender}");
    }

    [Reducer(ReducerKind.ClientDisconnected)]
    public static void Disconnect(ReducerContext ctx)
    {
        Log.Info($"[Disconnect] Client disconnected: {ctx.Sender}");
    }

    /////////////////////////////////////////////////////////////
    /// PLAYER CREATION

    [Reducer]
    public static void Login(ReducerContext ctx, string name)
    {
        Log.Info($"[CreatePlayer] Creating player for {ctx.Sender} with name: {name}");

        var exists = ctx.Db.player.player_id.Find(name);
        if (exists != null)
            throw new Exception("[CreatePlayer] Player already exists");

        var newPlayer = new Player
        {
            identity = ctx.Sender,
            player_id = name,
            money = 0,
            class_name = "",
            items = new List<Item>(),
            consumables = new List<Item>(),
            armor_id = 0,
            weapon_id = 0,
        };
        ctx.Db.player.Insert(newPlayer);
    }

    /////////////////////////////////////////////////////////////
    /// ITEM MANAGEMENT

    [Reducer]
    public static void AddItem(ReducerContext ctx, uint _id)
    {
        Log.Info($"[AddItem] Adding item with ID {_id}");

        var entity = ctx.Db.item.id.Find(_id);
        if (entity.HasValue)
        {
            var updated = entity.Value;
            updated.quantity++;
            ctx.Db.item.id.Update(updated);
            Log.Info($"[AddItem] Increased quantity of item {_id}. Current quantity: {updated.quantity}");
        }
        else
        {
            var newItem = new Item { id = _id, quantity = 1 };
            ctx.Db.item.Insert(newItem);
            Log.Info($"[AddItem] Inserted new item with ID {_id}");
        }
    }

    [Reducer]
    public static void DeleteItem(ReducerContext ctx, uint _id)
    {
        Log.Info($"[DeleteItem] Deleting item with ID {_id}");

        var entity = ctx.Db.item.id.Find(_id) ?? throw new Exception($"[DeleteItem] Item {_id} does not exist!");

        if (entity.quantity > 1)
        {
            var updated = entity;
            updated.quantity--;
            ctx.Db.item.id.Update(updated);
            Log.Info($"[DeleteItem] Decreased quantity of item {_id}. Current quantity: {updated.quantity}");
        }
        else
        {
            ctx.Db.item.id.Delete(entity.id);
            Log.Info($"[DeleteItem] Item {_id} fully removed!");
        }
    }

    /////////////////////////////////////////////////////////////
    /// PLAYER UPDATES

    [Reducer]
    public static void EquipWeapon(ReducerContext ctx, uint itemId, string player_id)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.weapon_id = itemId;
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void EquipArmor(ReducerContext ctx, uint itemId, string player_id)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.armor_id = itemId;
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void AddConsumableToPlayer(ReducerContext ctx, uint itemId)
    {
        var player = ctx.Db.player.identity.Find(ctx.Sender) ?? throw new Exception("[AddConsumableToPlayer] Player not found");
        if (player.consumables == null)
            player.consumables = new List<Item>();

        player.consumables.Add(new Item { id = itemId, quantity = 1 });
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void SetConsumablesToPlayer(ReducerContext ctx, string player_id, List<Item> consumables)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.consumables = consumables;
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void SetInventoryToPlayer(ReducerContext ctx, string player_id, List<Item> items)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.items = items;
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void SetPlayerClass(ReducerContext ctx, string player_id, string player_class)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.class_name = player_class;
        ctx.Db.player.identity.Update(player);
    }

    [Reducer]
    public static void SetPlayerMoney(ReducerContext ctx, string player_id, uint player_money)
    {
        var player = ctx.Db.player.player_id.Find(player_id)?? throw new Exception("[EquipWeapon] Player not found");
        player.money = player_money;
        ctx.Db.player.identity.Update(player);
    }

    /////////////////////////////////////////////////////////////
    /// STATION & TRAIN

    [Reducer]
    public static void EnterTheTrain(ReducerContext ctx, string _id, string _from_station_id, string _to_station_id, string _player_id, uint _money)
    {
        var existingTrain = ctx.Db.train.id.Find(_id);

        if (existingTrain.HasValue)
        {
            var train = existingTrain.Value;
            if (!train.player_ids.Contains(_player_id))
            {
                train.player_ids.Add(_player_id);
                ctx.Db.train.id.Update(train);
            }
        }
        else
        {
            var newTrain = new Train
            {
                id = _id,
                from_station_id = _from_station_id,
                to_station_id = _to_station_id,
                money = _money,
                started = false,
                player_ids = new List<string> { _player_id }
            };

            ctx.Db.train.Insert(newTrain);
        }
    }

    [Reducer]
    public static void LeaveTheTrain(ReducerContext ctx, string _id, string _player_id)
    {
        var trainResult = ctx.Db.train.id.Find(_id);

        if (!trainResult.HasValue)
            return;

        var train = trainResult.Value;

        if (train.player_ids.Contains(_player_id))
        {
            train.player_ids.Remove(_player_id);

            if (train.player_ids.Count == 0)
            {
                ctx.Db.train.id.Delete(train.id);
            }
            else
            {
                ctx.Db.train.id.Update(train);
            }
        }
    }

    [Reducer]
    public static void DeleteTrain(ReducerContext ctx, string _id)
    {
        var train = ctx.Db.train.id.Find(_id) ?? throw new Exception($"[DeleteTrain] Train {_id} does not exist!");
        ctx.Db.train.id.Delete(train.id);
    }

    [Reducer]
    public static void AddStation(ReducerContext ctx, string _id, string _name)
    {
        var station = new Station { mvg_id = _id, game_id = _name, money = 0 };
        ctx.Db.station.Insert(station);
    }

    [Reducer]
    public static void DeleteStation(ReducerContext ctx, string _id)
    {
        var station = ctx.Db.station.mvg_id.Find(_id) ?? throw new Exception($"[DeleteStation] Station {_id} does not exist!");
        ctx.Db.station.mvg_id.Delete(station.mvg_id);
    }
}
