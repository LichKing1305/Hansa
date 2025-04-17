using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private int totalSlots = 36;
    
    private void Start()
    {
        // Create all inventory slots
        for (int i = 0; i < totalSlots; i++)
        {
            Instantiate(slotPrefab, inventoryGrid);
        }
        
        // Link to inventory updates
        InventoryManager.Instance.OnInventoryChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        // Refresh all slots
        for (int i = 0; i < totalSlots; i++)
        {
            var slot = inventoryGrid.GetChild(i).GetComponent<InventorySlotUI>();
            slot.UpdateSlot(InventoryManager.Instance.slots[i]);
        }
    }
}