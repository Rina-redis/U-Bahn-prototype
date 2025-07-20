using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isReady = false;
    [SerializeField] private float locationRefreshTime;

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
        DontDestroyOnLoad(this);
        //LogIn("browski");
    }

    public void StartGame()
    {

    }

    public void StopGame()
    {

    }

    public void GetReady()
    {
        isReady = true;
        //.... mock shit
        PlayerInventory inventory = PlayerInventory.instance;
        NetworkManager.Instance.JoinGame(inventory.GetCurrentWeapon(), inventory.GetCurrentArmor(), PlayerInventory.instance.GetPlayerCombatSystemData(),new List<ScriptableConsumable>());
    }
    public void GetUnready()
    {
        isReady = false;
        //...
    }

    public void LogIn(string nickname)
    {
        GetComponent<SpacetimeDBController>().initDB(nickname);

    }
}
