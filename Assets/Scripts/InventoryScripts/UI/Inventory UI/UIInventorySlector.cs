using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Collections;

public class UIInventorySlector : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{

    [SerializeField] TMP_Text selectorText;
    [SerializeField] UIItemSlot selectedSlot;
    [SerializeField] UIInventorySlot selectedInventorySlot;
    [SerializeField] UIDescriptionWindow descriotionPanel;
    public SelectorState state = SelectorState.NoSelection;

    private GameObject player;
    private PlayerInventory inventory;

    private UIInventoryController controller;

    public float longPressThreshold = 0.6f;

    private Coroutine _holdRoutine;

    public void Awake()
    {
        player = GameObject.Find("Player");
        inventory = player.GetComponent<PlayerInventory>();
        controller = GetComponent<UIInventoryController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UIInventoryController uiinvcontroller;
        if (!TryGetComponentFromList<UIInventoryController>(eventData.hovered, out uiinvcontroller))
        {
            return;
        }

        UIConsumableSlots conSlots;

        bool isOverConsumblePanel = TryGetComponentFromList<UIConsumableSlots>(eventData.hovered, out conSlots);

        bool isOverInventoryStash = TryGetComponentFromList<UIInventoryStash>(eventData.hovered, out UIInventoryStash stash);


        // StringBuilder str = new StringBuilder();
        // str.Append("Clicked on me, here is the stack:\n");
        foreach (GameObject go in eventData.hovered)
        {
            //str.Append(go.name + "\n");
            switch (state)
            {
                case SelectorState.NoSelection:
                    UIItemSlot itemSlot;

                    if (go.TryGetComponent<UIItemSlot>(out itemSlot))
                    {
                        //Debug.Log("check " + slot);
                        if (itemSlot == controller.UIweapon)
                        {
                            state = SelectorState.SelectingWeapon;
                            selectorText.text = "Selecting Weapon:";
                            selectedSlot = itemSlot;
                            controller.Rebuild();
                        }
                        else if (itemSlot == controller.UIarmor)
                        {
                            state = SelectorState.SelectingArmor;
                            selectorText.text = "Selecting Armor:";
                            selectedSlot = itemSlot;
                            controller.Rebuild();
                        }
                        else
                        {
                            Debug.LogError("Shit, what have you done. How did you select this slot");
                        }
                        //StringBuilder str = new StringBuilder();
                        // str.Append("Clicked on me, here is the stack:\n");
                        return;
                    }

                    if (isOverConsumblePanel)
                    {
                        UIInventorySlot conSlot;

                        if (go.TryGetComponent<UIInventorySlot>(out conSlot))
                        {
                            state = SelectorState.SelectingConsumable;
                            selectorText.text = "Selecting Consumable:";
                            selectedInventorySlot = conSlot;
                            controller.Rebuild();
                            return;
                        }
                    }

                    break;
                case SelectorState.SelectingArmor:
                case SelectorState.SelectingWeapon:

                    UIInventorySlot slot;

                    if (go.TryGetComponent<UIInventorySlot>(out slot)&&isOverInventoryStash)
                    {
                        if (slot.realSlot.GetSample() is ScriptableWeapon && state == SelectorState.SelectingWeapon)
                        {
                            state = SelectorState.NoSelection;
                            selectedSlot = null;
                            inventory.SwapOutCurrentWeapon(slot.realSlot.GetSample());
                            selectorText.text = "Inventory: ";
                            //controller.Rebuild();
                            return;

                        }
                        if (slot.realSlot.GetSample() is ScriptableArmor && state == SelectorState.SelectingArmor)
                        {
                            state = SelectorState.NoSelection;
                            selectedSlot = null;
                            inventory.SwapOutCurrentArmor(slot.realSlot.GetSample());
                            selectorText.text = "Inventory: ";

                            //controller.Rebuild();
                            return;
                        }
                        Debug.LogError("Smthing went wrong during swap out. Maybe a wrong item. Bro just check some good vibe");
                        state = SelectorState.NoSelection;
                        selectedSlot = null;
                        selectorText.text = "Inventory: ";

                        return;
                    }
                    break;
                case SelectorState.SelectingConsumable:

                    UIInventorySlot selSlot;

                    if (go.TryGetComponent<UIInventorySlot>(out selSlot)&&isOverInventoryStash)
                    {
                        int indexOfSlot = controller.UIConsumable.GetIndexOfSlot(selectedInventorySlot);
                        if (indexOfSlot < 0 || indexOfSlot >= inventory.GetMaxActiveConsumables())
                        {
                            Debug.LogError("Bro... if you see this... I am already dead. Take care");
                        }
                        inventory.AddToActiveCosumables(selSlot.realSlot, indexOfSlot);
                        selectorText.text = "Inventory: ";
                        selectedInventorySlot = null;
                        state = SelectorState.NoSelection;
                        controller.Rebuild();
                        return;
                    }
                    break;
            }
        }
        state = SelectorState.NoSelection;
        selectedSlot = null;
        selectedInventorySlot = null;
        try
        {
            controller.Rebuild();
            selectorText.text = "Inventory: ";
        }
        catch (NullReferenceException e)
        {
            Debug.Log("Please stop this nonsence " + e + " I dont know this person");
        }
        //Debug.Log(str.ToString());
        
    }

    public bool TryGetComponentFromList<T>(List<GameObject> list, out T found) where T:Component {
        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go != null && go.TryGetComponent(out found))
                return true;
        }
        found = null;
        return false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (state != SelectorState.NoSelection)
        {
            return;
        }
        UIInventorySlot uislot;
        if (TryGetComponentFromList<UIInventorySlot>(eventData.hovered, out uislot))
        {
            Cancel();
            _holdRoutine = StartCoroutine(HoldCheck(uislot.realSlot.GetSample()));
        }
        UIShopSlot uishopslot;
        if (TryGetComponentFromList<UIShopSlot>(eventData.hovered, out uishopslot))
        {
            Cancel();
            _holdRoutine = StartCoroutine(HoldCheck(ItemManager.instance.getItem(uishopslot.itemId)));
        }


    }

    IEnumerator HoldCheck(ScriptableItemBase item)
    {
        yield return new WaitForSecondsRealtime(longPressThreshold);
        UIMasterController.instance.ShowDescription(item);
    }

    public void OnPointerUp(PointerEventData data) => Cancel();
    public void OnPointerExit(PointerEventData data) => Cancel();

    private void Cancel()
    {
        if (_holdRoutine != null)
        {
            StopCoroutine(_holdRoutine);
        }
        _holdRoutine = null;
    }


    public enum SelectorState
    {
        NoSelection,
        SelectingArmor,
        SelectingWeapon,
        SelectingConsumable
    }

}
