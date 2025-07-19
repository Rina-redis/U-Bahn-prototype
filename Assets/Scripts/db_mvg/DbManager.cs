using UnityEngine;
using System;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Generic;

public class DbManager
{
    private string player_id;

    private bool IsReady = false;
    private Queue<Action> pendingActions = new Queue<Action>();

    public DbManager(string player_name)
    {
        player_id = player_name;
        Application.targetFrameRate = 60;

        var builder = DbConnection.Builder()
            .OnConnect(HandleConnect)
            .OnConnectError(HandleConnectError)
            .OnDisconnect(HandleDisconnect)
            .WithUri(SERVER_URL)
            .WithModuleName(MODULE_NAME);

        if (PlayerPrefs.HasKey(AuthToken.Token))
        {
            builder = builder.WithToken(AuthToken.Token);
        }

        Conn = builder.Build();
    }

    const string SERVER_URL = "http://127.0.0.1:3000";
    const string MODULE_NAME = "spacetime-db-service";

    public static Identity LocalIdentity { get; private set; }
    public static DbConnection Conn { get; private set; }

    void HandleConnect(DbConnection conn, Identity identity, string token)
    {
        Debug.Log("[GameManager] Connected to SpacetimeDB.");
        AuthToken.SaveToken(token);
        LocalIdentity = identity;

        Conn.SubscriptionBuilder()
            .OnApplied(HandleSubscriptionApplied)
            .SubscribeToAllTables();

        Conn.Reducers.Login(player_id);
    }

    private void HandleSubscriptionApplied(SubscriptionEventContext ctx)
    {
        Debug.Log("Subscription applied!");
        IsReady = true;

        while (pendingActions.Count > 0)
        {
            pendingActions.Dequeue().Invoke();
        }
    }

    void HandleConnectError(Exception ex)
    {
        Debug.LogError($"[GameManager] Connection error: {ex}");
    }

    void HandleDisconnect(DbConnection conn, Exception ex)
    {
        Debug.LogWarning("[GameManager] Disconnected.");
        if (ex != null)
        {
            Debug.LogException(ex);
        }
    }

    public void RunWhenReady(Action action)
    {
        if (IsReady) action();
        else pendingActions.Enqueue(action);
    }

    ////////////////////////////////////////////////////////
    /// TWOJE METODY (wszystkie przez RunWhenReady)

    public uint get_armor_id()
    {
        uint armor_id = 0;
        RunWhenReady(() =>
        {
            var player = Conn.Db.Player.PlayerId.Find(player_id);
            armor_id = player.ArmorId;
        });
        return armor_id;
    }

    public void set_armor(uint armor_id)
    {
        RunWhenReady(() =>
        {
            Conn.Reducers.EquipArmor(armor_id, player_id);
        });
    }

    public uint get_weapon_id()
    {
        uint weapon_id = 0;
        RunWhenReady(() =>
        {
            var player = Conn.Db.Player.PlayerId.Find(player_id);
            weapon_id = player.ArmorId;
        });
        return weapon_id;
    }

    public void set_weapon(uint weapon_id)
    {
        RunWhenReady(() =>
        {
            Conn.Reducers.EquipWeapon(weapon_id, player_id);
        });
    }

    public List<Item> get_inventory()
    {
        List<Item> inventory = new List<Item>();
        RunWhenReady(() =>
        {
            var player = Conn.Db.Player.PlayerId.Find(player_id);
            inventory = player.Items;
        });
        return inventory;

    }

    public void set_inventory(List<Item> items)
    {
        RunWhenReady(() =>
        {
            Debug.Log($"[DbManager] (TODO) Set inventory with {items?.Count ?? 0} items");
            // Implementacja zależna od logiki reducerów
        });
    }

    public void get_consumables()
    {
        RunWhenReady(() =>
        {
            var player = Conn.Db.Player.PlayerId.Find(player_id);
            Debug.Log($"[DbManager] Consumables: {player?.Consumables?.Count ?? 0} items");
        });
    }

    public void set_consumables(List<Item> items)
    {
        RunWhenReady(() =>
        {
            Debug.Log($"[DbManager] (TODO) Set consumables with {items?.Count ?? 0} items");
            // Implementacja zależna od logiki reducerów
        });
    }

    public void get_player_class()
    {
        RunWhenReady(() =>
        {
            var player = Conn.Db.Player.PlayerId.Find(player_id);
            Debug.Log($"[DbManager] Class: {player?.ClassName}");
        });
    }

    public void set_player_class(string class_name)
    {
        RunWhenReady(() =>
        {
            Debug.Log($"[DbManager] (TODO) Set class to {class_name}");
            // Można dodać reducer
        });
    }
}
