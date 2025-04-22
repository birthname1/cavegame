using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI; // Added for the Sprite type

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int amount;
    public GameObject itemPrefab = null;
    public bool stackable;
    public GameObject dropItem;
    public bool dropped = false;
    public bool isSmeltable;
    public Item smeltItem;
    public bool isRecipeItem;
    public bool isPickaxe;
    public PickaxeData pickaxeData = null;
    public int identity;
    public Item gem = null;
}

