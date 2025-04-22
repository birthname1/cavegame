using UnityEngine;

public class RightStick : MonoBehaviour
{ // Assign pickaxe in Inspector
    public Transform player;  // Assign player in Inspector
    public Transform pickaxe;
    public InventoryBehavior inventory; // Assign pickaxe in Inspector

    void Start()
    {
        player = GameObject.Find("player").transform;
        inventory = FindFirstObjectByType<InventoryBehavior>();
    }
    void GoToPick() {
        if (pickaxe != null) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position;
            if (Input.GetMouseButton(0)) // While holding left click
                {
                transform.SetParent(pickaxe);
                if (mousePos.y/mousePos.x > 0) { 
                    transform.position = pickaxe.TransformPoint(new Vector2(-0.125f, -0.25f)); // Local offset
                    transform.rotation = pickaxe.rotation * Quaternion.Euler(0, 0, 0);
                }
                else if (mousePos.y/mousePos.x == 0) {

                }
                else if (mousePos.y/mousePos.x < 0)
                {
                    transform.position = pickaxe.TransformPoint(new Vector2(-0.245f, -0.115f)); // Local offset
                    transform.rotation = pickaxe.rotation * Quaternion.Euler(0, 0, -90f);
                }
                
                
            }
        }    
        else
        {
            transform.SetParent(player);
            Vector3 offset = new Vector3(-0.28f, -0.18f, 0f);
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

