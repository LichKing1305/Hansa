using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int stackSize;
    
    public bool IsEmpty => itemData == null;
    public bool IsFull => !IsEmpty && stackSize >= itemData.maxStackSize;
    
    public void Clear()
    {
        itemData = null;
        stackSize = 0;
    }
    
    public void AssignItem(ItemData data, int amount)
    {
        if (data == null)
        {
            Clear();
            return;
        }
        itemData = data;
        stackSize = amount;
    }
    
    public int AddToStack(int amount)
    {
        int remainingSpace = itemData.maxStackSize - stackSize;
        int amountToAdd = Mathf.Min(remainingSpace, amount);
        stackSize += amountToAdd;
        return amount - amountToAdd;
    }
}