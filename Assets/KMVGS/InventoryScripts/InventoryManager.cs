using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public System.Action OnInventoryChanged;
    [SerializeField] private int inventorySize = 36; // Default Minecraft inventory size
    public InventorySlot[] slots;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Optional - keeps inventory between scenes
        
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        slots = new InventorySlot[inventorySize];
        for (int i = 0; i < inventorySize; i++)
        {
            slots[i] = new InventorySlot();
        }
    }
    
public bool AddItem(ItemData itemToAdd, int amount = 1)
{
    if (itemToAdd == null)
    {
        Debug.LogError("Tried to add null item!");
        return false;
    }

    bool addedAny = false;
    int originalAmount = amount;

    // First try to stack with existing items
    for (int i = 0; i < slots.Length; i++)
    {
        if (!slots[i].IsEmpty && 
            slots[i].itemData == itemToAdd && 
            !slots[i].IsFull)
        {
            int remaining = slots[i].AddToStack(amount);
            addedAny = addedAny || (remaining != amount);
            amount = remaining;
            
            if (amount <= 0) 
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
    }
    
    // Then try empty slots
    for (int i = 0; i < slots.Length; i++)
    {
        if (slots[i].IsEmpty)
        {
            int addAmount = Mathf.Min(amount, itemToAdd.maxStackSize);
            slots[i].AssignItem(itemToAdd, addAmount);
            amount -= addAmount;
            addedAny = true;
            
            if (amount <= 0)
            {
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
    }
    
    // Partial add success
    if (addedAny)
    {
        Debug.LogWarning($"Added {originalAmount - amount} of {itemToAdd.itemName}, {amount} didn't fit");
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    // Complete failure
    Debug.LogWarning($"Inventory full! Couldn't add {amount} {itemToAdd.itemName}");
    return false;
}
public int GetRemainingSpaceForItem(ItemData item)
{
    int space = 0;
    foreach (var slot in slots)
    {
        if (slot.IsEmpty)
        {
            space += item.maxStackSize;
        }
        else if (slot.itemData == item)
        {
            space += item.maxStackSize - slot.stackSize;
        }
    }
    return space;
}

public bool CanAddItem(ItemData item, int amount = 1)
{
    return GetRemainingSpaceForItem(item) >= amount;
}

public bool RemoveItem(ItemData itemToRemove, int amount = 1)
{
    int totalFound = 0;
    
    // First count how many we have
    foreach (var slot in slots)
    {
        if (!slot.IsEmpty && slot.itemData == itemToRemove)
        {
            totalFound += slot.stackSize;
        }
    }
    
    if (totalFound < amount) return false;
    
    // Now actually remove them
    for (int i = 0; i < slots.Length; i++)
    {
        if (!slots[i].IsEmpty && slots[i].itemData == itemToRemove)
        {
            int amountToRemove = Mathf.Min(amount, slots[i].stackSize);
            slots[i].stackSize -= amountToRemove;
            amount -= amountToRemove;
            
            if (slots[i].stackSize <= 0)
            {
                slots[i].Clear();
            }
            
            if (amount <= 0) return true;
        }
    }
    
    return false;
}

public bool HasItem(ItemData item, int amount = 1)
{
    int total = 0;
    foreach (var slot in slots)
    {
        if (!slot.IsEmpty && slot.itemData == item)
        {
            total += slot.stackSize;
            if (total >= amount) return true;
        }
    }
    return false;
}
}