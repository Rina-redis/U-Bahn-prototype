using UnityEngine;
using System;
using System.Threading.Tasks;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Generic;

public class DbManager
{
    private string player_id;

    private TaskCompletionSource<bool> connectionReady = new TaskCompletionSource<bool>();

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

    private void HandleConnect(DbConnection conn, Identity identity, string token)
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
        connectionReady.TrySetResult(true);
    }

    private void HandleConnectError(Exception ex)
    {
        Debug.LogError($"[GameManager] Connection error: {ex}");
    }

    private void HandleDisconnect(DbConnection conn, Exception ex)
    {
        Debug.LogWarning("[GameManager] Disconnected.");
        if (ex != null)
        {
            Debug.LogException(ex);
        }
    }

    private async Task WaitUntilReady()
    {
        await connectionReady.Task;
    }

    ////////////////////////////////////////////////////////
    /// TWOJE METODY ASYNC

    public async Task<uint> get_armor_id()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.ArmorId ?? 0;
    }

    public async Task set_armor(uint armor_id)
    {
        await WaitUntilReady();
        Conn.Reducers.EquipArmor(armor_id, player_id);
    }

    public async Task<uint> get_player_money()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.Money ?? 0;
    }

    public async Task set_player_money(uint _money)
    {
        await WaitUntilReady();
        Conn.Reducers.SetPlayerMoney(player_id, _money);
    }

    public async Task<uint> get_weapon_id()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.WeaponId ?? 0;
    }

    public async Task set_weapon(uint weapon_id)
    {
        await WaitUntilReady();
        Conn.Reducers.EquipWeapon(weapon_id, player_id);
    }

    public async Task<List<Item>> get_inventory()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.Items ?? new List<Item>();
    }

    public async Task set_inventory(List<Item> items)
    {
        await WaitUntilReady();
        Conn.Reducers.SetInventoryToPlayer(player_id, items);
    }

    public async Task<List<Item>> get_consumables()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.Consumables ?? new List<Item>();
    }

    public async Task set_consumables(List<Item> items)
    {
        await WaitUntilReady();
        Conn.Reducers.SetConsumablesToPlayer(player_id, items);
    }

    public async Task<string> get_player_class()
    {
        await WaitUntilReady();
        var player = Conn.Db.Player.PlayerId.Find(player_id);
        return player?.ClassName ?? "Unknown";
    }

    public async Task set_player_class(string class_name)
    {
        await WaitUntilReady();
        Conn.Reducers.SetPlayerClass(player_id, class_name);
    }
}
