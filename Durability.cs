using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Durability : MonoBehaviour
{
    public Tilemap tilemap; 
    public Tilemap foreground;// Reference to your tilemap
    public Dictionary<Vector3Int, int> blockDurability = new Dictionary<Vector3Int, int>();
    public GameObject dropStone;
    public GameObject dropIron;
    public GameObject dropGold;
    public GameObject mushroom;
    public ProceduralGeneration proceduralGeneration;
    void Start()
    {
        tilemap = GameObject.Find("ground").GetComponent<Tilemap>();
        foreground = GameObject.Find("foreground").GetComponent<Tilemap>();
        proceduralGeneration = FindFirstObjectByType<ProceduralGeneration>();
        InitializeDurability();
        
    }

    public void InitializeDurability()
    {
        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                // Set default durability based on tile type
                blockDurability[position] = GetDurabilityForTile(tilemap.GetTile(position));
            }
        }
    }

    public int GetDurabilityForTile(TileBase tile)
    {
        if (tile.name.Contains("iron")) return 5;
        if (tile.name.Contains("gold")) return 6;

        return 2; // Default durability
    }

    public void DamageBlock(Vector3Int position, int strength)
    {
        if (blockDurability.ContainsKey(position))
        {
            blockDurability[position] -= strength;

            if (blockDurability[position] <= 0)
            {
                DestroyBlock(position);
            }
        }
    }

    void DestroyBlock(Vector3Int position)
    {
        if (tilemap.GetTile(position).name.Contains("iron")) {
            Instantiate(dropIron, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f), Quaternion.Euler(0, 0, 0));
        }
        else if (tilemap.GetTile(position).name.Contains("gold")) {
            Instantiate(dropGold, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f), Quaternion.Euler(0, 0, 0));
        }
        else Instantiate(dropStone, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f), Quaternion.Euler(0, 0, 0));

        tilemap.SetTile(position, null);
        foreground.SetTile(position, null);

        blockDurability.Remove(position); // Remove from durability tracking
        proceduralGeneration.EditTile(position, false);
    }
}