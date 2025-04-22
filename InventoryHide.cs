using UnityEngine;

public class InventoryHide : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CanvasGroup canvasGroup;
    private bool isVisible = true;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI();
        }
    }

    void ToggleUI()
    {
        isVisible = !isVisible;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
}
