using System.Collections.Generic;
using SpacetimeDB.Types;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    private GpsMvgManager gpsLocation;
    private bool? isMoving = null;
    private bool eventTriggered = false;
    private string fromStationId, toStationId;
    private void OnEnable()
    {
        gpsLocation = FindFirstObjectByType<GpsMvgManager>();
        if (gpsLocation != null)
        {
            gpsLocation.OnIsMovingChanged += OnMovingChanged;
        }
        else
        {
            Debug.LogError("GameManager: GpsMvgManager not found in scene!");
        }
    }

    private void OnDisable()
    {
        if (gpsLocation != null)
        {
            gpsLocation.OnIsMovingChanged -= OnMovingChanged;
        }
    }

    private void OnMovingChanged(bool moving, GpsMvgManager.Station from, GpsMvgManager.Station to)
    {
        isMoving = moving;
        eventTriggered = true;
        Debug.Log($"Listener: isMoving = {moving}, from = {from?.stationName}, to = {to?.stationName}");
        fromStationId = from.stationGlobalID;
        toStationId = to.stationGlobalID;
    }

    private async void Start()
    {
        var dbService = new DbManager("blablabla");
        uint armorId = await dbService.get_armor_id();
        Debug.Log($"Armor ID: {armorId}");
        await dbService.set_player_class("Tank");
        string player_class = await dbService.get_player_class();
        Debug.Log($"Player class: {player_class}");

        Item sample_item = new Item();
        sample_item.Id = 12;
        sample_item.Quantity = 2;

        Item sample_item2 = new Item();
        sample_item2.Id = 222;
        sample_item2.Quantity = 9;

        List<Item> inventory = new List<Item>();
        inventory.Add(sample_item);
        inventory.Add(sample_item2);
        await dbService.set_inventory(inventory);
        List<Item> return_inventory = await dbService.get_inventory();
        foreach (Item item in return_inventory)
        {
            Debug.Log($"Player inventory: {item.Id}, {item.Quantity}");
        }

        await dbService.set_player_money(100);
        uint money = await dbService.get_player_money();
        Debug.Log($"Moeny: {money}");
    }

    private void Update()
    {
        if (eventTriggered && isMoving.HasValue)
        {
            if (isMoving.Value)
            {
                Debug.Log("GAME STARTED");
                //DbManager.enterTheTrain(from.stationGlobalID, to.stationGlobalID);
            }
            else
            {
                Debug.Log("GAME STOPPED");
                //DbManager.leaveTheTrain(from.stationGlobalID, to.stationGlobalID);
            }
            // create a new train in db if not exists
            // half of the players should be in the stop area
            eventTriggered = false;
        }
    }
}
