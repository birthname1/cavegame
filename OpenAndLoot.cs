using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class OpenAndLoot : MonoBehaviour
{
    public Animator animator;
    public InventoryBehavior inventory;
    public PickaxeSwing pickaxe;
    public bool opened = false;
    public Item pickaxeItem;
    public Item goldBar;
    public Item ironBar;
    public GameObject droppedItem;
    public Item newPickaxeInst;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = FindFirstObjectByType<InventoryBehavior>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        pickaxe = FindFirstObjectByType<PickaxeSwing>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pickaxe") && pickaxe.swingCompletedBuffer > 0f && opened == false) {
            opened = true;

            animator.SetBool("open", true);
            droppedItem = Instantiate(GetLoot().dropItem, transform.position, Quaternion.identity);
            ItemDrop itemDrop = droppedItem.GetComponent<ItemDrop>();
            itemDrop.refItem = newPickaxeInst;
        }
    }

    public Item GetLoot() {
        float lootTable = Random.value;

        /*if (lootTable < 0.1f) {
            return goldBar;
        }
        else if (lootTable < 0.5f) {
            return ironBar;
        }
        else {*/
            newPickaxeInst = inventory.GenerateRandomPickaxe();
            return newPickaxeInst;
        //}
    }
}
