using UnityEngine;

public class RightArmMove : MonoBehaviour
{
    public Transform player;      // The player transform
    public Transform pickaxe;     // The pickaxe transform
    public float rotationSpeed = 10f; // Speed at which the arm moves to follow the pickaxe
    public Vector3 positionOffset = new Vector3(0.5f, 0.5f, 0); // Offset from player position

    void Update()
    {
       RotateToPickaxe();
    }
    void RotateToPickaxe (){

        Vector3 directionToPickaxe = pickaxe.position - player.position;

        float angleToPickaxe = (Mathf.Atan2(directionToPickaxe.y, directionToPickaxe.x) * Mathf.Rad2Deg) + 90f;

        if (Input.GetMouseButton(0)) // While holding left click
        {
            transform.SetParent(pickaxe);
            transform.position = pickaxe.TransformPoint(new Vector2(-0.065f, 0f)); // Local offset
            Quaternion targetRotation = Quaternion.Euler(0, 0, angleToPickaxe);
            transform.rotation = Quaternion.Slerp(targetRotation, transform.rotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.SetParent(player);
            transform.position = player.TransformPoint(new Vector2(0.1875f, -0.2f));
            transform.rotation = player.rotation;
        }
    }
}