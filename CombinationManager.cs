using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class CombinationManager : MonoBehaviour
{
    MushroomBreak[] mushrooms;
    ItemDrop[] droppedMushrooms;
    public Vector2 center;
    public Vector2 distance;
    public List<Item> droppedItems = new List<Item>();
    public Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();
    public List<CombineRecipe> combineRecipes;
    
    [Serializable]
    public class CombineRecipe
    {
        public Item resultItem; // Could also be a GameObject if you want to spawn a prefab
        public List<ItemAmount> requiredItems;

        public Dictionary<Item, int> GetRequiredItemsAsDictionary()
        {
            Dictionary<Item, int> dict = new Dictionary<Item, int>();
            foreach (ItemAmount pair in requiredItems)
            {
                if (pair.item != null)
                    dict[pair.item] = pair.amount;
            }
            return dict;
        }
    }
    [Serializable]
    public struct ItemAmount
    {
        public Item item;
        public int amount;
    }
    public GameObject CheckForMatchingRecipe(Dictionary<Item, int> ingredients)
    {
        foreach (CombineRecipe recipe in combineRecipes)
        {
            Dictionary<Item, int> required = recipe.GetRequiredItemsAsDictionary();

            if (DoesRecipeMatch(required, ingredients))
                return recipe.resultItem.dropItem;
        }
        return null; // No match
    }

    private bool DoesRecipeMatch(Dictionary<Item, int> recipe, Dictionary<Item, int> ingredients)
    {
        foreach (var pair in recipe)
        {
            if (!ingredients.ContainsKey(pair.Key) || ingredients[pair.Key] < pair.Value)
                return false;
        }
        return true;
    }
    public void Update()
    {
        mushrooms = FindObjectsByType<MushroomBreak>(FindObjectsSortMode.None);
        droppedMushrooms = FindObjectsByType<ItemDrop>(FindObjectsSortMode.None);

        GroupDroppedItems();
        SmeltItem();
        CombineItems();
    }
    void SmeltItem() {
        foreach (var mushroom in mushrooms)
        {
            if (!mushroom.CompareTag("fireMushroom")) continue;

            foreach (var item in droppedItems)
            {
                if (item.dropped && item.isSmeltable &&
                    Vector2.Distance(item.dropItem.transform.position, mushroom.transform.position) < 0.75f)
                {
                    item.dropped = false;

                    Destroy(item.dropItem);
                    Destroy(mushroom.gameObject);

                    Instantiate(item.smeltItem.dropItem, 
                        mushroom.transform.position + Vector3.up * 0.375f, 
                        Quaternion.identity);

                    break;
                }
            }
        }

        
        foreach (var droppedMushroom in droppedMushrooms)
        {
            if (!droppedMushroom.CompareTag("fireMushroom")) continue;

            foreach (var item in droppedItems)
            {
                if (item.dropped && item.isSmeltable &&
                    Vector2.Distance(item.dropItem.transform.position, droppedMushroom.transform.position) < 0.75f)
                {
                    item.dropped = false;

                    Destroy(item.dropItem);
                    Destroy(droppedMushroom.gameObject);

                    Instantiate(item.smeltItem.dropItem, 
                        droppedMushroom.transform.position + Vector3.up * 0.375f, 
                        Quaternion.identity);

                    break;
                }
            }
        }
    }
    void GroupDroppedItems() 
    {
        groupedItems.Clear();
        distance = Vector2.zero; 
        center = Vector2.zero;

        droppedItems = FindObjectsByType<Item>(FindObjectsSortMode.None)
        .Where(item => item.dropped && item.dropItem != null).ToList();
        
        if (droppedItems.Count == 0) return;
        foreach (var item in droppedItems)
        {
            if (!item.dropped || item.dropItem == null) continue;

            distance += (Vector2)item.dropItem.transform.position;
            
            if (groupedItems.ContainsKey(item))
                groupedItems[item]++;
            else
                groupedItems.Add(item, 1);
        }

        int validItemCount = groupedItems.Values.Sum();
        if (validItemCount > 0)
            center = distance / validItemCount;

        Debug.Log($"Grouped {groupedItems.Count} items, Center: {center}");
    }
    void CombineItems() {
        if (groupedItems.Count == 0) return;

    // Check mushrooms first
        foreach (var mushroom in mushrooms)
        {
            if (!mushroom.CompareTag("combiner")) continue;
            
            if (Vector2.Distance(center, mushroom.transform.position) < 0.75f)
            {
                GameObject result = CheckForMatchingRecipe(groupedItems);
                if (result != null)
                {
                    // Remove used items
                    foreach (var item in groupedItems.Keys.ToList())
                    {
                        if (item.dropItem != null)
                        {
                            item.dropped = false;
                            Destroy(item.dropItem);
                        }
                    }

                    // Create result
                    Vector3 spawnPos = mushroom.transform.position + Vector3.up * 0.375f;
                    Instantiate(result, spawnPos, Quaternion.identity);
                    
                    // Cleanup
                    Destroy(mushroom.gameObject);
                    groupedItems.Clear();
                    return;
                }
            }
        }

        foreach (var droppedMushroom in droppedMushrooms)
        {
            if (!droppedMushroom.CompareTag("combiner") || !droppedMushroom.refItem.dropped) continue;

            if (Vector2.Distance(center, droppedMushroom.transform.position) < 0.75f)
            {
                GameObject result = CheckForMatchingRecipe(groupedItems);
                if (result != null)
                {
                    // Remove used items
                    foreach (var item in groupedItems.Keys.ToList())
                    {
                        if (item.dropItem != null)
                        {
                            item.dropped = false;
                            Destroy(item.dropItem);
                        }
                    }

                    // Create result
                    Vector3 spawnPos = droppedMushroom.transform.position + Vector3.up * 0.375f;
                    Instantiate(result, spawnPos, Quaternion.identity);
                    
                    // Cleanup
                    Destroy(droppedMushroom.gameObject);
                    groupedItems.Clear();
                    return;
                }
            }
        }
    }
}
