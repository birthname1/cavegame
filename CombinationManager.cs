using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;

public class CombinationManager : MonoBehaviour
{
    MushroomBreak[] mushrooms;
    ItemDrop[] droppedMushrooms;
    public Vector2 center;
    public Vector2 distance;
    public List<GameObject> droppedItems = new List<GameObject>();
    public Dictionary<GameObject, List<GameObject>> groupedItems = new Dictionary<GameObject, List<GameObject>>();
    public List<CombineRecipe> combineRecipes;
    public DraggableSlot slotdrag;
    float clusterThreshold = 0.75f;
    
    [Serializable]
    public class CombineRecipe
    {
        public Item resultItem; // Could also be a GameObject if you want to spawn a prefab
        public List<ItemAmount> requiredItems;

        public Dictionary<GameObject, int> GetRequiredItemsAsDictionary()
        {
            Dictionary<GameObject, int> dict = new Dictionary<GameObject, int>();
            foreach (ItemAmount pair in requiredItems)
            {
                if (pair.item != null)
                    dict[pair.item] = pair.amount.Count;
            }
            return dict;
        }
    }
    [Serializable]
    public struct ItemAmount
    {
        public GameObject item;
        public List<GameObject> amount;
    }
    public GameObject CheckForMatchingRecipe(Dictionary<GameObject, List<GameObject>> ingredients)
    {
        foreach (CombineRecipe recipe in combineRecipes)
        {
            Dictionary<GameObject, int> required = recipe.GetRequiredItemsAsDictionary();

            if (DoesRecipeMatch(required, ingredients))
                return recipe.resultItem.dropItem;
        }
        return null; // No match
    }

    private bool DoesRecipeMatch(Dictionary<GameObject, int> recipe, Dictionary<GameObject, List<GameObject>> ingredients)
    {
        foreach (var pair in recipe)
        {
            if (!ingredients.ContainsKey(pair.Key) || ingredients[pair.Key].Count < pair.Value)
                return false;
        }
        return true;
    }
    public void Update()
    {
        mushrooms = FindObjectsByType<MushroomBreak>(FindObjectsSortMode.None);
        droppedMushrooms = FindObjectsByType<ItemDrop>(FindObjectsSortMode.None);

        GroupClusters(droppedItems, clusterThreshold);
        SmeltItem();
        CombineItems();
    }
    public void Start()
    {
        slotdrag = FindFirstObjectByType<DraggableSlot>();
    }
    void SmeltItem() {
        var allMushrooms = mushrooms.Select(m => m.gameObject)
            .Concat(droppedMushrooms.Select(m => m.gameObject))
            .ToList();

        foreach (var mushroom in allMushrooms)
        {
            if (!mushroom.CompareTag("fireMushroom")) continue;

            foreach (var item in droppedItems)
            {
                if (item == null) continue;
                Item refItem = item.GetComponent<ItemDrop>().refItem;
                if (refItem == null || !refItem.dropped || !refItem.isSmeltable) continue;

                if (Vector2.Distance(item.transform.position, mushroom.transform.position) < 0.75f)
                {
                    refItem.dropped = false;

                    Destroy(item);
                    Destroy(mushroom.gameObject);

                    Instantiate(refItem.smeltItem.dropItem, 
                        mushroom.transform.position + Vector3.up * 0.375f, 
                        Quaternion.identity);

                    break;
                }
            }
        }
    }
    public List<List<GameObject>> GroupClusters(List<GameObject> droppedItems, float clusterThreshold)
    {
        List<List<GameObject>> clusters = new List<List<GameObject>>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        foreach (var item in droppedItems)
        {
            if (item == null || visited.Contains(item)) continue;

            // Create a new cluster
            List<GameObject> cluster = new List<GameObject>();
            Queue<GameObject> queue = new Queue<GameObject>();
            queue.Enqueue(item);

            while (queue.Count > 0)
            {
                var currentItem = queue.Dequeue();
                if (visited.Contains(currentItem)) continue;

                visited.Add(currentItem);
                cluster.Add(currentItem);

                // Check for nearby items
                foreach (var otherItem in droppedItems)
                {
                    if (otherItem == null || visited.Contains(otherItem)) continue;

                    float distance = Vector2.Distance(currentItem.transform.position, otherItem.transform.position);
                    if (distance <= clusterThreshold)
                    {
                        queue.Enqueue(otherItem);
                    }
                }
            }

            // Add the cluster to the list of clusters
            if (cluster.Count > 0)
            {
                clusters.Add(cluster);
            }
        }
        return clusters;
    }
    void CombineItems()
    {
        if (droppedItems.Count == 0 || droppedMushrooms.Length == 0) return;

    // Group dropped mushrooms into clusters
        var allMushrooms = mushrooms.Select(m => m.gameObject)
            .Concat(droppedMushrooms.Select(m => m.gameObject))
            .ToList();

        List<List<GameObject>> mushroomClusters = GroupClusters(allMushrooms, clusterThreshold);

        foreach (var mushroomCluster in mushroomClusters)
        {
            // Calculate the center of the mushroom cluster
            Vector2 clusterCenter = Vector2.zero;
            foreach (var mushroom in mushroomCluster)
            {
                clusterCenter += (Vector2)mushroom.transform.position;
            }
            clusterCenter /= mushroomCluster.Count;

            foreach (var mushroom in mushroomCluster)
            {
                if (!mushroom.CompareTag("combiner")) continue;

                // Filter dropped items to include only those near the current mushroom
                List<GameObject> nearbyItems = droppedItems
                    .Where(item => item != null && Vector2.Distance(item.transform.position, mushroom.transform.position) < clusterThreshold)
                    .ToList();

                if (nearbyItems.Count == 0) continue;

                // Group nearby items into clusters
                List<List<GameObject>> itemClusters = GroupClusters(nearbyItems, clusterThreshold);

                foreach (var itemCluster in itemClusters)
                {
                    // Check for matching recipes
                    Dictionary<GameObject, List<GameObject>> clusterGroupedItems = new Dictionary<GameObject, List<GameObject>>();
                    foreach (var item in itemCluster)
                    {
                        Item refItem = item.GetComponent<ItemDrop>().refItem;
                        if (refItem == null) continue;

                        if (clusterGroupedItems.ContainsKey(refItem.dropItem))
                        {
                            clusterGroupedItems[refItem.dropItem].Add(item);
                        }
                        else
                        {
                            clusterGroupedItems[refItem.dropItem] = new List<GameObject> { item };
                        }
                    }

                    GameObject result = CheckForMatchingRecipe(clusterGroupedItems);
                    if (result != null)
                    {
                        // Remove used items
                        foreach (var kvp in clusterGroupedItems)
                        {
                            foreach (var item in kvp.Value)
                            {
                                Item refItem = item.GetComponent<ItemDrop>().refItem;
                                if (!refItem.isPickaxe) {
                                    Destroy(item);
                                    droppedItems.Remove(item);
                                }
                            }
                        }

                        // Create the result item at the cluster center
                        Vector3 spawnPos = mushroom.transform.position + Vector3.up * 0.375f;
                        Instantiate(result, spawnPos, Quaternion.identity);

                        // Cleanup
                        Destroy(mushroom);
                        groupedItems.Clear();
                        break;
                    }
                }
            }
        }
    }
}
