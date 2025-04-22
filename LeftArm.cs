using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class LeftStick : MonoBehaviour
{
    public Transform player;
    public Transform pickaxe;
    public InventoryBehavior inventory;
    void Start()
    {
        player = GameObject.Find("player").transform;
        inventory = FindFirstObjectByType<InventoryBehavior>();
    }
    void GoToPick() {
        if (pickaxe != null) {
            transform.SetParent(pickaxe);
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position;
            if (Input.GetMouseButton(0)) // While holding left click
                {
                
                if (mousePos.y/mousePos.x > 0) { 
                    transform.position = pickaxe.TransformPoint(new Vector2(0f, -0.125f)); // Local offset
                    transform.rotation = pickaxe.rotation * Quaternion.Euler(0, 0, 0);
                }
                else if (mousePos.y/mousePos.x == 0) {

                }
                else if (mousePos.y/mousePos.x < 0)
                {
                    transform.position = pickaxe.TransformPoint(new Vector2(-0.125f, 0.067f)); // Local offset
                    transform.rotation = pickaxe.rotation * Quaternion.Euler(0, 0, -90f);
                }
                
                
            }
        }
        else
        {
        transform.SetParent(player);
        Vector3 offset = new Vector3(0.28f, -0.18f, 0f);
        transform.position = Vector2.Lerp(transform.position, player.position + offset, Time.deltaTime * 100f); // Adjust speed
        transform.rotation = Quaternion.Lerp(transform.rotation, player.rotation, Time.deltaTime * 100f); // Adjust speed
        }
    }
    void Update()
    {
        if (inventory.heldItem != null && inventory.heldItem.CompareTag("Pickaxe"))
        {
            pickaxe = inventory.heldItem.transform;
        }
        else
        {
            pickaxe = null;
        }
        GoToPick();
    }
}  
