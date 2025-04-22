using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Animations;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEditor.UIElements;


public class DraggableSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    //public InventoryManager inventory;
    public RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    public Camera mainCamera;
    public TextMeshProUGUI count;
    public TextMeshProUGUI itemName;
    public Item item;
    public Image image;
    public Image gemImage;
    public int inventoryIdentity;
    public int toolbarIdentity;
    public InventoryBehavior inventory;
    Transform originalParent;
    public float ableDropTimer;
    public GameObject player;
    public bool dropped = false;
    public float droppedTimer;
    public playerMovement playerfacing;
    public bool dragging;
    void Start() {
        
        mainCamera = FindFirstObjectByType<Camera>();
        //inventory = FindFirstObjectByType<InventoryManager>();
        inventory = FindFirstObjectByType<InventoryBehavior>();
        canvasGroup = inventory.GetComponent<CanvasGroup>();
        parentCanvas = inventory.GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        player = GameObject.Find("player");
        playerfacing = FindFirstObjectByType<playerMovement>();


        toolbarIdentity = -1;
        inventoryIdentity = inventory.inventorySlotCount - 1;
        item = inventory.inventoryItems[inventoryIdentity];
        image.sprite = inventory.inventoryItems[inventoryIdentity].icon;
        
        itemName.text = item.itemName.ToString();
        if (item.pickaxeData != null) {
            itemName.color = item.pickaxeData.GetColor(item.pickaxeData.RarityAssignment());
        }
        else itemName.color = Color.white;
        if (item.gem != null) {
            gemImage.enabled = true;
            gemImage.sprite = inventory.inventoryItems[inventoryIdentity].gem.icon;
        }  
        else gemImage.enabled = false;
    }
    public void Update()
    {
        if (item.amount > 1) {count.enabled = true; count.text = item.amount.ToString();}
        else count.enabled = false;

        DestroyToolbarSlot();
        DropItem();
    }

    public void DropItem() {
        if (inventory.globalDropCooldown > 0f) return;
        if (inventory.selectedIndex != toolbarIdentity) return;
        if (inventory.toolbar.childCount <= 0) return;
        if (item.amount <= 0) return;
        
        if (Input.GetKeyDown(KeyCode.Q) 
        && item == inventory.toolbarItems[inventory.selectedIndex]
        && inventory.toolbar.childCount > 0) {

            item.dropped = true;
            inventory.globalDropCooldown = 0.5f;
            
            GameObject droppedItem = Instantiate(item.dropItem, player.transform.position, Quaternion.identity);
            
            ItemDrop itemDrop = droppedItem.GetComponent<ItemDrop>();
            if (itemDrop != null) {
                itemDrop.refItem = Instantiate(item);
                Debug.Log($"Assigned refItem: {itemDrop.refItem.itemName}");
                
                if (item.isPickaxe) {
                    itemDrop.refItem.pickaxeData = inventory.pickaxes[item.identity - 1];
                }
                itemDrop.refItem.dropped = true;
            }

            if (playerfacing.facingRight) droppedItem.transform.position = new Vector2(
                Mathf.Lerp(player.transform.position.x, player.transform.position.x + 12.5f, 0.075f), player.transform.position.y);
            else droppedItem.transform.position = new Vector2(
                Mathf.Lerp(player.transform.position.x, player.transform.position.x - 12.5f, 0.075f), player.transform.position.y);
            item.amount--;
            if (item.amount <= 0) DestroyToolbarSlot();
        }
    }

    

    public void DestroyToolbarSlot() {
        if (item.amount <= 0) {
            inventory.selectedIndex--;

            inventory.toolbarItems.RemoveAt(toolbarIdentity);
            inventory.toolbarSlots.RemoveAt(toolbarIdentity);

            transform.SetParent(inventory.inventoryBar);
            Destroy(inventory.heldItem);
            inventory.heldItem = null;

            Destroy(gameObject);
            inventory.selectionBoxInstance = null;
            
            inventory.toolbarSlotCount--;
            inventory.UpdateSelectionBoxPosition();
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    { 
        dragging = true;
        originalParent = gameObject.transform.parent;    
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragging = true;
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
       // Restore slot visibility and re-enable raycasts
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, 
            eventData.position, 
            mainCamera, 
            out Vector2 localPoint
        );

        if (localPoint.y <= -70f && transform.parent == inventory.inventoryBar) {
            inventory.inventorySlotCount--;
            inventory.toolbarSlotCount++;

            transform.SetParent(inventory.toolbar);

            inventory.inventorySlots.RemoveAt(inventoryIdentity);
            inventory.inventoryItems.RemoveAt(inventoryIdentity);
            inventory.toolbarItems.Add(item);
            inventory.toolbarSlots.Add(gameObject);

            inventoryIdentity = -1;
            toolbarIdentity = inventory.toolbarItems.IndexOf(item);
            inventory.selectedIndex = toolbarIdentity;

            inventory.UpdateIdentities();
            inventory.UpdateSelectionBoxPosition();
        }
        else if (localPoint.y >= -70f && transform.parent == inventory.toolbar) {
            inventory.toolbarSlotCount--;
            inventory.inventorySlotCount++;
            
            transform.SetParent(inventory.inventoryBar);

            inventory.toolbarSlots.RemoveAt(toolbarIdentity);
            inventory.toolbarItems.RemoveAt(toolbarIdentity);
            inventory.inventoryItems.Add(item);
            inventory.inventorySlots.Add(gameObject);

            toolbarIdentity = -1;
            inventoryIdentity = inventory.inventoryItems.IndexOf(item);

            inventory.UpdateIdentities();
            inventory.UpdateSelectionBoxPosition();
        }
        else {
            transform.SetParent(originalParent);
        }
        rectTransform.anchoredPosition = Vector2.zero;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent as RectTransform);

        dragging = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!dragging) itemName.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!dragging) itemName.enabled = false;
    }
}

