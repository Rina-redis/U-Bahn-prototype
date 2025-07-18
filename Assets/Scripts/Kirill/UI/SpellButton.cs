using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isPreparingSpell = false;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image icon;
    [SerializeField] private PlayerCombatSystem player;

    private ActiveSpell spell;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPreparingSpell = true;
        Debug.Log("Spell is being prepared...");
        // TODO: Visual Adjustment of spell preparation
    }

    void Awake()
    {
        //icon.sprite = spell.SpellData.icon;
    }

    public void SetSpell(Spell spell)
    {
        Debug.Log("I AM SETTING SPELL: " + spell);
        if (spell is ActiveSpell activeSpell)
        {
            this.spell = activeSpell;
            icon.sprite = activeSpell.SpellData.icon;
        }
    }

    void FixedUpdate()
    {
        cooldownOverlay.fillAmount = 1f - spell.Reload.ReloadPercentage;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPreparingSpell) return;

        Debug.Log(spell);
        Debug.Log(player);

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y));

        spell.Activate(player, player.gameObject.transform, worldMousePosition);

        Debug.Log("Spell cast toward: " + worldMousePosition);

        isPreparingSpell = false;
    }
}
