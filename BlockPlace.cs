using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
public class BlockPlace : MonoBehaviour
{
    public Tilemap tilemap; // Assign your Tilemap in the Inspector
    public TileBase blockTile; // Assign your Tile asset in the Inspector
    public GameObject player; // Reference to the player (or center point)
    public float maxRadius = 5f;
    public Item item;
    public InventoryBehavior inventory; // Maximum placement distance

    private Camera mainCamera;
    public Vector3Int cellPosition;
    public Durability durability;
    public ProceduralGeneration proceduralGeneration;
    public DraggableSlot draggableSlot;

    void Start()
    {
        draggableSlot = FindFirstObjectByType<DraggableSlot>();
        player = GameObject.Find("player");
        mainCamera = Camera.main;
        tilemap = GameObject.Find("ground").GetComponent<Tilemap>();
        inventory = FindFirstObjectByType<InventoryBehavior>();
        durability = FindFirstObjectByType<Durability>();
        proceduralGeneration = FindFirstObjectByType<ProceduralGeneration>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !draggableSlot.dragging) // Left-click to place a tile
        {
            PlaceTile();
        }
    }

    void PlaceTile()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; // Keep on the correct plane

        // Convert world position to tilemap position
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // Check if within placement radius
        float distance = Vector3.Distance(player.transform.position, tilemap.GetCellCenterWorld(cellPosition));
        
        if (distance <= maxRadius && item.amount > 0)
        {
            if (tilemap.GetTile(cellPosition) != null) return; // Tile already exists
            item.amount -= 1;
            tilemap.SetTile(cellPosition, blockTile);

            durability.blockDurability[cellPosition] = durability.GetDurabilityForTile(tilemap.GetTile(cellPosition));
            proceduralGeneration.EditTile(cellPosition, true);
        }
        
    }
}