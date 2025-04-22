using System;
using System.Security.Authentication;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PickaxeSwing : MonoBehaviour
{     
    public GameObject player;        // Player transform for positioning
    public AnimationCurve rotationSwingCurve;
    public float swingDuration = 0.5f; // Total duration of the swing
    private float swingTime = 0f;      // Time tracker for swing
    public bool isSwinging = false;   // Check if the pickaxe is swinging
    public Vector3 mousePos;
    public float maxValx = 0.5f;
    public float maxValy = 0.75f;
    public Tilemap scene;
    public float pointAngle;
    public float spriteOffset = -45f;
    public float swingCompletedBuffer = 0f;
    public bool swingCompleted = false;
    public Item item;
    public int strength = 1;
    public float size;
    public InventoryBehavior inventory;
    public PickaxeData data;
        
    void Start()
    {
        inventory = FindFirstObjectByType<InventoryBehavior>();
        player = GameObject.Find("player");
        scene = FindFirstObjectByType<Tilemap>();

        item = inventory.toolbarItems[inventory.selectedIndex];
        data = inventory.pickaxes[item.identity - 1];
        float size = item.pickaxeData.size;
        transform.localScale = new Vector3(size, size, 1f);
    }
    void Update()
    { 
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;
        mousePos.z = 0f;
        Swinging();
    }  
    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Tilemap"))
        {
            Tilemap tilemap = other.GetComponent<Tilemap>();
            Durability generator = FindFirstObjectByType<Durability>();
            if (generator != null && swingCompleted == true)
            {
                Vector3Int tilePosition = tilemap.WorldToCell(transform.position);
                generator.DamageBlock(tilePosition, strength);
                swingCompleted = false;
            }
        }
    } 

    void Swinging() {
        float startX = (mousePos.x > 0) ? maxValx : -maxValx;
        float endX = (mousePos.x > 0) ? -maxValx : maxValx;
        float startY = (mousePos.y > 0) ? maxValy : -maxValy;
        float endY = (mousePos.y > 0) ? -maxValy : maxValy;
        
        pointAngle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg) + spriteOffset;

        float clampedX = Mathf.Clamp(mousePos.x, -maxValx, maxValx);
        float clampedY = Mathf.Clamp(mousePos.y, -maxValy, maxValy);

        // DO SOMETHING WITH ASIN AND ACOS TO OFFSET THE ANGLE

        if (Input.GetMouseButtonDown(0))
        {
            if (!isSwinging) {
                isSwinging = true;
            }
        }
        if (isSwinging)
        {
            swingTime += Time.deltaTime / swingDuration;
            
            float rotationCurveValue = rotationSwingCurve.Evaluate(swingTime);
            float xCurveValue = rotationSwingCurve.Evaluate(swingTime);

            if ((pointAngle > 60 + spriteOffset && pointAngle < 120 + spriteOffset) || (pointAngle > -120 + spriteOffset && pointAngle < -60 + spriteOffset)) {
                //y direction rotations
                transform.localRotation = (mousePos.y/mousePos.x > 0) 
            ? Quaternion.Euler(0f, 0f, Mathf.LerpAngle(pointAngle, pointAngle - 45f + spriteOffset, rotationCurveValue)) 
            : Quaternion.Euler(0f, 0f, Mathf.LerpAngle(pointAngle, pointAngle + 45f - spriteOffset, rotationCurveValue));
            }
            else {
                //x directions rotations
                transform.localRotation = (mousePos.y/mousePos.x > 0) 
            ? Quaternion.Euler(0f, 0f, Mathf.LerpAngle(pointAngle, pointAngle + 45f - spriteOffset, rotationCurveValue)) 
            : Quaternion.Euler(0f, 0f, Mathf.LerpAngle(pointAngle, pointAngle - 45f + spriteOffset, rotationCurveValue));
            }
            

            if ((pointAngle > 60 + spriteOffset && pointAngle < 120 + spriteOffset) || (pointAngle > -120 + spriteOffset && pointAngle < -60 + spriteOffset)) {
                //y direction movement
                transform.position = new Vector3(clampedX + player.transform.position.x, player.transform.position.y + Mathf.Lerp(startY, endY, xCurveValue), 0f);
            }
            else {
                //x direction movement
                transform.position = new Vector3(player.transform.position.x + Mathf.Lerp(startX, endX, xCurveValue), clampedY + player.transform.position.y, 0f);
            }
            

        }
        else {
            transform.SetPositionAndRotation(new Vector3(Mathf.Lerp(transform.position.x, clampedX + player.transform.position.x, 1f),
            Mathf.Lerp(transform.position.y, clampedY + player.transform.position.y, 2f), 0f), Quaternion.Euler(0, 0, pointAngle));
        }
    
        if (swingTime >= 1f)
        {
            swingCompleted = true;
            swingCompletedBuffer += Time.deltaTime;
            isSwinging = false;
            swingTime = 0f;
        }
        if (swingCompletedBuffer >= 0.1f) {

            swingCompletedBuffer = 0f;
        }
    }
    /*
    public Rarity RarityAssignment() {
        if (size > 1.8f) {
            return Rarity.Epic;
        }
        else if (size > 1.5f) {
            return Rarity.Rare;
        }
        else if (size > 1.0f) {
            return Rarity.Uncommon;
        }
        else {
            return Rarity.Common;
        }
    }
     */
}