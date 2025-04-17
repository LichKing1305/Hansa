using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Text countText;
    
    private InventorySlot currentSlot;
    
    public void UpdateSlot(InventorySlot slot)
    {
        currentSlot = slot;
        icon.sprite = slot.itemData?.icon;
        icon.gameObject.SetActive(slot.itemData != null);
        countText.text = slot.stackSize > 1 ? slot.stackSize.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle click actions (equip/use/drop)
    }
}