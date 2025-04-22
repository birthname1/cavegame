using Unity.VisualScripting;
using UnityEngine;

public class ItemHold : MonoBehaviour
{
    public UnityEngine.GameObject player;
    float maxVal = 0.5f; // Maximum value for clamping mouse position
    public Vector3 mousePos;
    public float pointAngle;
    public float spriteOffset = -45f;
    public Item item;
    public void Hold() 
    {
        gameObject.SetActive(true); // Ensure the item is active when holding

        pointAngle = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg) + spriteOffset;

        float clampedX = Mathf.Clamp(mousePos.x, -maxVal, maxVal);
        float clampedY = Mathf.Clamp(mousePos.y, -maxVal, maxVal);

        transform.SetPositionAndRotation(new Vector3
        (Mathf.Lerp(transform.position.x, clampedX + player.transform.position.x, 1f),
        Mathf.Lerp(transform.position.y, clampedY + player.transform.position.y, 2f), 
        0f), Quaternion.Euler(0, 0, pointAngle));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = UnityEngine.GameObject.Find("player");
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.transform.position;
        mousePos.z = 0f;
        Hold();
    }
}
