using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CanvasGroup inventoryCanvasGroup;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            bool isVisible = inventoryCanvasGroup.alpha > 0;
            inventoryCanvasGroup.alpha = isVisible ? 0 : 1; // Toggle visibility
            inventoryCanvasGroup.interactable = !isVisible; // Enable/disable interactions
            inventoryCanvasGroup.blocksRaycasts = !isVisible; // Enable/disable raycasting for clicks
        }
    }
}
