using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon = null;
    [TextArea] public string description;
    public int maxStackSize = 64;
    public GameObject worldPrefab; // For when the item is dropped in the world
    
    [Header("Item Type")]
    public bool isConsumable = false;
    public bool isEquippable = false;
    public bool isQuestItem = false;
}