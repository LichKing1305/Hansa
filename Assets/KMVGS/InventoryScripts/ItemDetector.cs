using UnityEngine;
public class ItemDetector : MonoBehaviour
{
    public float pickupRange = 5f;
    public LayerMask itemLayer;
    public KeyCode pickupKey = KeyCode.E;
    
    private void Update()
    {
        // Create ray from camera through mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickupRange, itemLayer))
        {
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                // Show UI prompt
                if (Input.GetKeyDown(pickupKey))
                {
                    pickup.TryPickup();
                }
            }
        }
    }
}