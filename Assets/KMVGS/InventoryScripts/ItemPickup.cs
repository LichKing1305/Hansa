using UnityEngine;
using KMVGS.FinalCharacterController;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;
    public float pickupDistance = 5f;
    public GameObject pickupEffect;
    
    public void TryPickup()
    {
        float distance = Vector3.Distance(
            transform.position, 
            PlayerController.Instance.transform.position
        );
        
        if (distance <= pickupDistance)
        {
            if (InventoryManager.Instance.AddItem(itemData, amount))
            {
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }

    // Visual feedback when selected by cursor
    public void OnHoverStart()
    {
        // Add outline effect or scale up
        transform.localScale *= 1.1f;
    }

    public void OnHoverEnd()
    {
        // Remove visual effect
        transform.localScale /= 1.1f;
    }
}