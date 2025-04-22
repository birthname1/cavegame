using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ItemDrop : MonoBehaviour
{
    public CombinationManager combo;
    public GameObject player;
    public float destroyDistance = 0.2f;
    public Item refItem;
    public InventoryBehavior inventory;
    public float dropTimer = 0f;
    TombManager[] tombs;
    public List<Item> droppedItems = new List<Item>();
    public Dictionary<Item, int> groupedItems = new Dictionary<Item, int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("player");
        inventory = FindFirstObjectByType<InventoryBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        PickupItem();
        TombSacrifice();
    }
    void TombSacrifice() {
        tombs = FindObjectsByType<TombManager>(FindObjectsSortMode.None);

        for (int i = 0; i < tombs.Count(); i++) {
            if (refItem.dropped 
            && Vector2.Distance(transform.position, tombs[i].transform.position) < 1f
            && refItem.isRecipeItem && !tombs[i].bossActive) 
            {
                refItem.dropped = false;

                Destroy(gameObject);
                if (tombs[i].heldItems.ContainsKey(refItem))
                tombs[i].heldItems[refItem]++;

                else tombs[i].heldItems.Add(refItem, 1);
            }
        }
    }
    
    void PickupItem() {
        
        if (!refItem.dropped && Mathf.Abs(player.transform.position.x - transform.position.x) <= 1f 
        && Mathf.Abs(player.transform.position.y - transform.position.y) <= 1f) {
            ItemMovement();
        }
        else if (refItem.dropped) {
            dropTimer += Time.deltaTime;
            if (dropTimer >= 1f 
            && Mathf.Abs(player.transform.position.x - transform.position.x) <= 1f 
            && Mathf.Abs(player.transform.position.y - transform.position.y) <= 1f) 
            { dropTimer = 0f; refItem.dropped = false; ItemMovement(); }
            else return;
        }
    }
    void ItemMovement() {
        transform.position = new Vector2(Mathf.Lerp(transform.position.x, player.transform.position.x, 0.1f), Mathf.Lerp(transform.position.y, player.transform.position.y, 0.1f));
        if (Vector2.Distance(transform.position, player.transform.position) <= destroyDistance)
        { 
            Destroy(gameObject);
            inventory.AddItem(refItem);
        }
    }
}

