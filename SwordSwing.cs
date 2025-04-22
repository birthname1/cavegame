using System;
using System.Security.Authentication;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SwordSwing : MonoBehaviour
{     
    public UnityEngine.GameObject player;        // Player transform for positioning
    public AnimationCurve rotationSwingCurve;
    // Total duration of the swing
    private float swingTime = 0f;
    
    public float swingDuration = 0.1f;     // Time tracker for swing
    public bool swingStart = false;  // Check if the pickaxe is swinging
    public Vector3 mousePos;
    public float maxVal = 0.5f;
    public float pointAngle;
    public float spriteOffset = -45f;
    public float swingCompletedBuffer = 0f;
    public bool swingCompleted = false;
    public Item item;
    bool isSwinging = false;

    void Swinging() {
        float startX = (mousePos.x > 0) ? 0.5f : -0.5f;
        float endX = (mousePos.x > 0) ? -0.5f : 0.5f;
        float startY = (mousePos.y > 0) ? 0.5f : -0.5f;
        float endY = (mousePos.y > 0) ? -0.5f : 0.5f;
        
        pointAngle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg) + spriteOffset;

        float clampedX = Mathf.Clamp(mousePos.x, -maxVal, maxVal);
        float clampedY = Mathf.Clamp(mousePos.y, -maxVal, maxVal);

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
            if (swingTime >= 1f)
            {
                swingTime = 0f;                                       
                isSwinging = false;
            }
        }
        else {
            transform.SetPositionAndRotation(new Vector3(Mathf.Lerp(transform.position.x, clampedX + player.transform.position.x, 1f),
            Mathf.Lerp(transform.position.y, clampedY + player.transform.position.y, 2f), 0f), Quaternion.Euler(0, 0, pointAngle));
        }
    }
    void Start()
    {
        player = GameObject.Find("player");
    }
    void Update()
    { 
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;
        mousePos.z = 0f;
        Swinging();
    }  
}