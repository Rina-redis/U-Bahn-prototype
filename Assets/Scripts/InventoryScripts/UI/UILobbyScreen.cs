using System.Text;
using LightScrollSnap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyScreen : MonoBehaviour
{
    [SerializeField] UIInventoryController uiInventory;
    [SerializeField] UIShopManager uiShop;
    [SerializeField] TMP_Text playerNickname;
    [SerializeField] TMP_Text playerMoney;
    [SerializeField] Button readyButton;
    [SerializeField] ScrollSnap scrollSnap;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        LeaveStation();
    }
    private bool shopEnabled;
    void Start()
    {
        Rebuild();
        PlayerInventory.instance.MoneyChanged.AddListener(UpdateMoney);
    }

    public void Rebuild()
    {
        if (WorldMapController.instance.isOnStation)
        {
            if (!shopEnabled)
            {
                shopEnabled = true;
                ArriveAtStation(WorldMapController.instance.currentStation);
            }
            uiShop?.Rebuild(WorldMapController.instance.currentStation);
        }
        else
        {
            if (shopEnabled)
            {
                shopEnabled = false;
                LeaveStation();
            }
        }
        
        playerNickname.text = PlayerInventory.instance.GetNickname();
        StringBuilder str = new StringBuilder();
        playerMoney.text = str.Append("Player`s Money: ").Append(PlayerInventory.instance.GetMoney()).Append(" $").ToString();
        uiInventory.Rebuild();
    }

    public void UpdateMoney()
    {uiShop.gameObject.SetActive(true);
        StringBuilder str = new StringBuilder();
        playerMoney.text = str.Append("Player`s Money: ").Append(PlayerInventory.instance.GetMoney()).Append(" $").ToString();
    }

    public void ArriveAtStation(Station station)
    {
        uiShop.gameObject.SetActive(true);
        readyButton.gameObject.SetActive(true);
        scrollSnap.SmoothScrollToItem(uiShop.gameObject.transform.GetSiblingIndex(), scrollSnap.smoothScrollDuration);
    }

    public void LeaveStation()
    {
        uiShop.gameObject.SetActive(false);
        readyButton.gameObject.SetActive(false);
    }
}
