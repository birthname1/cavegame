using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using System.ComponentModel;
using Unity.VisualScripting;
using TMPro;
using UnityEditor;
using UnityEditor.SearchService;

public class InventoryBehavior : MonoBehaviour
{
    public Transform inventoryBar;
    public Transform toolbar;
    public GameObject slot;
    public List<PickaxeData> pickaxes = new List<PickaxeData>();
    public List<GameObject> inventorySlots = new List<GameObject>();
    public List<Item> inventoryItems = new List<Item>();
    public List<GameObject> toolbarSlots = new List<GameObject>();
    public List<Item> toolbarItems = new List<Item>();
    public Item pickaxe;
    public Item sword;
    public int inventorySlotCount;
    public int toolbarSlotCount;
    public GameObject selectionBox;
    public GameObject selectionBoxInstance;
    public int selectedIndex = 0;
    public GameObject player;
    public GameObject heldItem;
    public GameObject heldItemPrefab;
    public int identity = 0;
    public Item selectedItem;
    public float globalDropCooldown = 0.05f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("player");
        AddItem(GenerateRandomPickaxe());
    }

    // Update is called once per frame
    void Update()
    {
        SelectionMove();
        if (globalDropCooldown > 0f) {
            globalDropCooldown -= Time.deltaTime;
        }
    }
    public float WeightedRandom(float min, float max, int power = 3)
    {
        float t = Mathf.Pow(Random.value, power);
        return Mathf.Lerp(min, max, t);
    }
    public Item GenerateRandomPickaxe() 
    {
        identity++;

        PickaxeData newPickaxe = ScriptableObject.CreateInstance<PickaxeData>();
        float minSize = 0.75f;
        float maxSize = 3.0f;
        float scale = WeightedRandom(minSize, maxSize);
        newPickaxe.size = scale;
        
        Item newItem = Instantiate(pickaxe);
        newItem.identity = identity;
        newItem.itemName += " " + newItem.identity.ToString();

        newPickaxe.identity = identity;

        pickaxes.Add(newPickaxe);
        newItem.pickaxeData = newPickaxe;
        return newItem;
    }
    public void UpdateIdentities()
    {
        // Update inventory identities
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            inventorySlots[i].GetComponent<DraggableSlot>().inventoryIdentity = i;
        }

        // Update toolbar identities
        for (int i = 0; i < toolbarItems.Count; i++)
        {
            toolbarSlots[i].GetComponent<DraggableSlot>().toolbarIdentity = i;
        }
    }
    public void SelectionMove(){
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Move Right
        {
            selectedIndex = (selectedIndex + 1) % toolbarSlots.Count;
            UpdateSelectionBoxPosition();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Move Left
        {
            selectedIndex = (selectedIndex - 1 + toolbarSlots.Count) % toolbarSlots.Count;
            UpdateSelectionBoxPosition();
        }
        selectedIndex = Mathf.Clamp(selectedIndex, 0, toolbarItems.Count - 1);
    }
    public void UpdateSelectionBoxPosition()
    {
        // Make sure toolbar identity values are up to date
        for (int i = 0; i < toolbarSlots.Count; i++)
        {
            toolbarSlots[i].GetComponent<DraggableSlot>().toolbarIdentity = i;
        }

        // No toolbar slots? Destroy selection and held item if they exist
        if (toolbar.childCount <= 0)
        {
            if (selectionBoxInstance != null)
            {
                Destroy(selectionBoxInstance);
                selectionBoxInstance = null;
            }

            if (heldItem != null)
            {
                Destroy(heldItem);
                heldItem = null;
            }

            return;
        }

        // Selection box does not exist yet — create it at slot 0
        if (selectionBoxInstance == null)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, toolbarSlots.Count - 1);
            selectionBoxInstance = Instantiate(selectionBox, toolbarSlots[selectedIndex].transform);
            InstantiateHeldItem();
            return;
        }

        // Selection box is currently stuck in inventory — move it back to toolbar
        if (selectionBoxInstance.transform.parent == inventoryBar)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, toolbarSlots.Count - 1);
            selectionBoxInstance.transform.SetParent(toolbarSlots[selectedIndex].transform, false);
            InstantiateHeldItem();
            return;
        }

        // Regular case: keep it updated to selected index
        selectedIndex = Mathf.Clamp(selectedIndex, 0, toolbarSlots.Count - 1);
        selectionBoxInstance.transform.SetParent(toolbarSlots[selectedIndex].transform, false);
        InstantiateHeldItem();
    }
    public void AddItem(Item newItem) {
        
        if (newItem == null) return; 
        
        if (newItem.isPickaxe) 
        {
            newItem.amount = 1;
            int index = newItem.identity - 1;
            if (index >= 0 && index < pickaxes.Count)
            {
                newItem.pickaxeData = pickaxes[index];
            }
            inventoryItems.Add(newItem);
            CreateInventorySlots();
            return;
        }
        
        bool stacked = false;

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (newItem.itemName == inventoryItems[i].itemName && inventoryItems[i].stackable)
            {
                inventoryItems[i].amount += 1;
                stacked = true;
                break; 
            }
        }

        for (int i = 0; i < toolbarItems.Count; i++)
        {
            if (newItem.itemName == toolbarItems[i].itemName && toolbarItems[i].stackable)
            {
                toolbarItems[i].amount += 1;
                //inventorySlots[i].SlotCounter();
                stacked = true;
                break; // Exit loop after stacking
            }
        }

        if (!stacked) // If no stackable match was found, add as a new item
        {
            newItem.amount = 1;
            inventoryItems.Add(newItem);
            CreateInventorySlots();
        }   
    }
    public void CreateInventorySlots()
    {
        inventorySlotCount++;
        GameObject newSlot = Instantiate(slot, inventoryBar); // Instantiate a new slot prefab
        inventorySlots.Add(newSlot); 
    }

    public void InstantiateHeldItem() {
        selectedItem = toolbarItems[selectedIndex];

        if (selectedItem == null || selectedItem.itemPrefab == null) 
        {
            Destroy(heldItem);
            heldItem = null;
            return;
        }
        if (heldItem == null) {
            heldItem = Instantiate(selectedItem.itemPrefab, player.transform.position, Quaternion.identity);
            heldItemPrefab = selectedItem.itemPrefab;

            if (selectedItem.isPickaxe) 
            {
                float scale = selectedItem.pickaxeData.size;
                heldItem.transform.localScale = new Vector3(scale, scale, 1f);
            }
            
        }
        else if (heldItemPrefab != selectedItem.itemPrefab 
        || (selectedItem.isPickaxe && heldItem.transform.localScale.x != selectedItem.pickaxeData.size)) { 
            Destroy(heldItem);
            heldItem = null;
            heldItem = Instantiate(selectedItem.itemPrefab, player.transform.position, Quaternion.identity);

            if (selectedItem.isPickaxe) 
            {
                float scale = selectedItem.pickaxeData.size;
                heldItem.transform.localScale = new Vector3(scale, scale, 1f);
                Debug.Log($"Setting scale to {scale} for pickaxe {selectedItem.identity}");
            }

            heldItemPrefab = selectedItem.itemPrefab;
        }
    }
}
