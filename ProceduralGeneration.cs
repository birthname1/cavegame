using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Data;
using Unity.VisualScripting;

public class ProceduralGeneration : MonoBehaviour
{
    public BiomeManager biomeManager;
    public GameObject tomb;
    public GameObject chest;
    public Tile markedTile;
    public int chunkSize = 16;
    public int viewDistance = 2; // How many chunks away to generate
    public string seed;
    public bool useRandomSeed;
    [Range(0, 100)]
    public int randomFillPercent;

    public Tilemap tilemap;
    public Tilemap background;
    public Tile wallTile;
    public Tile backgroundTile;
    public Tilemap foreground;
    public Tile grass;
    public Transform player;
    public Durability durability;
    public ChunkLoader chunkLoader; // Reference to the ChunkManager

    private Dictionary<Vector3Int, bool> editedTiles = new Dictionary<Vector3Int, bool>();
    private Dictionary<Vector2Int, int[,]> generatedChunks = new Dictionary<Vector2Int, int[,]>();
    private System.Random pseudoRandom;
    int worldSeed;
    Dictionary<GameObject, Vector2Int> mushrooms = new Dictionary<GameObject, Vector2Int>();

    public int offsetX;
    public int offsetY;
    

    BiomeType GetBiomeForChunk(Vector2Int chunkCoord) {

        float scale = 0.1f; // Smaller = larger biomes
        float noise = Mathf.PerlinNoise((chunkCoord.x + offsetX) * scale, (chunkCoord.y + offsetY) * scale);

        if (noise < 0.5f) return BiomeType.Combination;
        //&& noise < 0.5f...
        else if (chunkCoord.y < -2) return BiomeType.Trans;
        
        else return BiomeType.Fire;
    }

    void Start()
    {
        biomeManager = FindFirstObjectByType<BiomeManager>();
        durability = FindFirstObjectByType<Durability>();
        player = GameObject.Find("player").transform;
        chunkLoader = FindFirstObjectByType<ChunkLoader>(); // Find the ChunkManager in the scene

        if (useRandomSeed) {
            worldSeed = DateTime.Now.Ticks.GetHashCode();
            seed = worldSeed.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());
        UpdateChunksAroundPlayer();

        System.Random rng = new System.Random(worldSeed);
        offsetX = rng.Next(-100000, 100000);
        offsetY = rng.Next(-100000, 100000);
    }

    void Update()
    {
        UpdateChunksAroundPlayer();
    }

    void UpdateChunksAroundPlayer()
    {
        Vector2Int currentChunk = GetPlayerChunk();
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();


        for (int dx = -viewDistance; dx <= viewDistance; dx++)
        {
            for (int dy = -viewDistance; dy <= viewDistance; dy++)
            {
                Vector2Int chunkCoord = new Vector2Int(currentChunk.x + dx, currentChunk.y + dy);
                chunksToKeep.Add(chunkCoord);

                if (!generatedChunks.ContainsKey(chunkCoord))
                {
                    if (chunkLoader.TryLoadChunk(chunkCoord, out ChunkData loadedChunk))
                    {
                        DrawChunk(chunkCoord, loadedChunk.map); // Load the chunk from saved data
                        generatedChunks[chunkCoord] = loadedChunk.map;
                    }
                    else
                    {
                        GenerateChunk(chunkCoord); // Generate a new chunk if not saved
                    }
                }
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();

        foreach (var chunk in generatedChunks.Keys)
        {
            if (!chunksToKeep.Contains(chunk))
            {
                chunksToRemove.Add(chunk);
            }
        }

        foreach (var chunk in chunksToRemove)
        {
            UnloadChunk(chunk);
        }
    }

    void UnloadChunk(Vector2Int chunkCoord)
    {
        List<GameObject> toRemove = new List<GameObject>();
        if (!generatedChunks.ContainsKey(chunkCoord)) return;

        // Clear tiles from the tilemap
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(chunkCoord.x * chunkSize + x, chunkCoord.y * chunkSize + y, 0);
                tilemap.SetTile(tilePos, null);
                background.SetTile(tilePos, null);
                foreground.SetTile(tilePos, null);
                foreach (var pair in mushrooms)
                {
                    if (pair.Value == chunkCoord)
                    {
                        Destroy(pair.Key);
                        toRemove.Add(pair.Key);
                    }
                }
                foreach (var mushroom in toRemove) {
                    mushrooms.Remove(mushroom);
                }
            }
        }

        // Save the chunk and remove it from memory
        chunkLoader.SaveChunk(chunkCoord);
        generatedChunks.Remove(chunkCoord);
    }

    Vector2Int GetPlayerChunk()
    {
        int x = Mathf.FloorToInt(player.position.x / chunkSize);
        int y = Mathf.FloorToInt(player.position.y / chunkSize);
        return new Vector2Int(x, y);
    }
    void GenerateOres(int[,] map, Vector2Int chunkCoord)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                // Skip air tiles
                if (map[x, y] == 0) continue;

                // Use random noise to determine ore placement
                float oreChance = UnityEngine.Random.value;

                if (oreChance < 0.001f && chunkCoord.y >= -2) // 0.1% chance for Gold
                {
                    map[x, y] = 3;
                    //map[x + UnityEngine.Random.Range(-1, 1), y + UnityEngine.Random.Range(-1, 1)] = 3;
                    //map[x + UnityEngine.Random.Range(-1, 1), y + UnityEngine.Random.Range(-1, 1)] = 3; // Gold
                }

                else if (oreChance < 0.005f && chunkCoord.y < -2) // 0.5% chance for Iron
                {
                    map[x, y] = 2;
                    //map[x + UnityEngine.Random.Range(-1, 1), y + UnityEngine.Random.Range(-1, 1)] = 2;
                    //map[x + UnityEngine.Random.Range(-1, 1), y + UnityEngine.Random.Range(-1, 1)] = 2; // Iron
                }
                
            }
        }
    }

    void GenerateChunk(Vector2Int chunkCoord)
    {
        int[,] map = new int[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {

                if (x == 0 || y == 0 || x == chunkSize - 1 || y == chunkSize - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
            
        }

        // Optionally smooth the chunk
        for (int i = 0; i < 5; i++)
        {
            map = SmoothMap(map);
        }

        GenerateOres(map, chunkCoord);

        DrawChunk(chunkCoord, map);
        generatedChunks.Add(chunkCoord, map);

        Dictionary<Vector3Int, bool> chunkEdits = new();
        foreach (var kvp in editedTiles)
        {
            Vector3Int pos = kvp.Key;
            Vector2Int coord = new Vector2Int(
                Mathf.FloorToInt(pos.x / (float)chunkSize),
                Mathf.FloorToInt(pos.y / (float)chunkSize)
            );
            if (coord == chunkCoord)
            {
                chunkEdits[pos] = kvp.Value;
            }
        }

        ChunkData chunkData = new ChunkData { map = map, editedTiles = chunkEdits };
        chunkLoader.loadedChunks[chunkCoord] = chunkData;
        chunkLoader.loadedChunks[chunkCoord] = chunkData; // Add the chunk to the ChunkManager
    }

    int[,] SmoothMap(int[,] map)
    {
        int[,] newMap = new int[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int neighborWalls = GetSurroundingWallCount(map, x, y);
                if (neighborWalls > 4)
                    newMap[x, y] = 1;
                else if (neighborWalls < 4)
                    newMap[x, y] = 0;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        return newMap;
    }

    int GetSurroundingWallCount(int[,] map, int x, int y)
    {
        int wallCount = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx >= 0 && nx < chunkSize && ny >= 0 && ny < chunkSize)
                {
                    if (nx != x || ny != y)
                        wallCount += map[nx, ny];
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    void DrawChunk(Vector2Int chunkCoord, int[,] map)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int tilePos = new Vector3Int(chunkCoord.x * chunkSize + x, chunkCoord.y * chunkSize + y, 0);

                if (chunkLoader.loadedChunks.TryGetValue(chunkCoord, out ChunkData chunkData) &&
                    chunkData.editedTiles.TryGetValue(tilePos, out bool placed))
                {
                    tilemap.SetTile(tilePos, placed ? wallTile : null);
                }
                else
                {
                    switch (map[x, y])
                    {
                        case 0: // Air
                            tilemap.SetTile(tilePos, null);
                            break;
                        case 1: // Wall
                            tilemap.SetTile(tilePos, wallTile);
                            break;
                        case 2: // Iron
                            tilemap.SetTile(tilePos, biomeManager.GetOreTile("Iron"));
                            break;
                        case 3: // Gold
                            tilemap.SetTile(tilePos, biomeManager.GetOreTile("Gold"));
                            break;
                    }
                }
                BiomeType biomeType = GetBiomeForChunk(chunkCoord);
                BiomeData biome = biomeManager.GetBiomeData(biomeType);

                if (y + 1 < chunkSize && map[x, y] == 1 && map[x, y + 1] == 0)
                {
                    foreground.SetTile(tilePos, biome.grass);
                    if (UnityEngine.Random.value < 0.1f) {
                        GameObject mushroom = Instantiate(biome.mushrooms.mushroomPrefabs[UnityEngine.Random.Range(0, biome.mushrooms.mushroomPrefabs.Length)], 
                        new Vector3(Mathf.Clamp(tilePos.x + UnityEngine.Random.value, tilePos.x + 0.2f, tilePos.x + 0.8f), tilePos.y + 1f, 0f), Quaternion.identity);
                        mushrooms.Add(mushroom, chunkCoord);
                    }
                }
                else
                {
                    foreground.SetTile(tilePos, null);
                }
                
                background.SetTile(tilePos, backgroundTile);
            
            }
        }
        if (FindTombLoc(map) != null && UnityEngine.Random.value < 0.088f) {
            PlaceTomb(FindTombLoc(map), chunkCoord);
        }
        if (FindChestLoc(map) != null && UnityEngine.Random.value < 0.088f) {
            PlaceChest(FindChestLoc(map), chunkCoord);
        }
        durability.InitializeDurability();
    }

    public void EditTile(Vector3Int position, bool placed)
    {
        //Debug.Log("EditTile called: " + position + " | Placed: " + placed);
        
        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt(position.x / (float)chunkSize),
            Mathf.FloorToInt(position.y / (float)chunkSize)
        );

        if (chunkLoader.loadedChunks.TryGetValue(chunkCoord, out ChunkData chunkData))
        {
            // Convert the world position to local chunk coordinates
            int localX = position.x - chunkCoord.x * chunkSize;
            int localY = position.y - chunkCoord.y * chunkSize;

            // Update the map directly
            chunkData.map[localX, localY] = placed ? 1 : 0;

            chunkData.editedTiles[position] = placed;

            // Save the chunk
            chunkLoader.SaveChunk(chunkCoord);
        }

        // Update the tilemap
        tilemap.SetTile(position, placed ? wallTile : null);
    }

    void OnApplicationQuit()
    {
        foreach (var chunkCoord in chunkLoader.loadedChunks.Keys)
        {
            chunkLoader.SaveChunk(chunkCoord);
        }
        Debug.Log("All chunks saved on application quit.");
    }


    List<Vector3Int> FindTombLoc(int[,] map)
    {
        List<Vector3Int> validTiles = new List<Vector3Int>();

        for (int x = 1; x < chunkSize - 2; x++) // Avoid edges
        {
            for (int y = 1; y < chunkSize - 2; y++) // Avoid edges
            {
                if (map[x - 1, y] == 1 && map[x + 1, y] == 1 && map[x + 2, y] == 1
                && map[x, y + 1] == 1 && map[x + 1, y + 1] == 1
                && map[x, y - 1] == 1 && map[x + 1, y - 1] == 1)
                    validTiles.Add(new Vector3Int(x, y, 0));
            }
        }

        //Debug.Log($"Found {validTiles.Count} valid tiles for tomb placement.");
        return validTiles;
    }

    public void PlaceTomb(List<Vector3Int> validTiles, Vector2Int chunkCoord) {
        if (validTiles == null || validTiles.Count == 0) return;

        Vector3Int tombPos = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];

        Vector3Int worldTombPos = new Vector3Int(
        chunkCoord.x * chunkSize + tombPos.x,
        chunkCoord.y * chunkSize + tombPos.y,
        0
        );

        tilemap.SetTile(worldTombPos, null);
        tilemap.SetTile(worldTombPos + Vector3Int.right, null);

        Instantiate(tomb, new Vector3(worldTombPos.x + 1f, worldTombPos.y + 0.5f, 0f), Quaternion.identity);

    }
    List<Vector3Int> FindChestLoc(int[,] map)
    {
        List<Vector3Int> validTiles = new List<Vector3Int>();

        for (int x = 1; x < chunkSize - 2; x++) // Avoid edges
        {
            for (int y = 1; y < chunkSize - 2; y++) // Avoid edges
            {
                if (map[x - 1, y] == 0 && map[x + 1, y] == 0
                && map[x, y + 1] == 0 && map[x + 1, y + 1] == 0 && map[x - 1, y + 1] == 0
                && map[x, y - 1] == 1 && map[x + 1, y - 1] == 1 && map[x - 1, y - 1] == 1)
                    validTiles.Add(new Vector3Int(x, y, 0));
            }
        }

        //Debug.Log($"Found {validTiles.Count} valid tiles for chest placement.");
        return validTiles;
    }

    public void PlaceChest(List<Vector3Int> validTiles, Vector2Int chunkCoord) {
        if (validTiles == null || validTiles.Count == 0) return;

        Vector3Int chestPos = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];

        Vector3Int worldChestPos = new Vector3Int(
        chunkCoord.x * chunkSize + chestPos.x,
        chunkCoord.y * chunkSize + chestPos.y,
        0
        );

        Instantiate(chest, new Vector3(worldChestPos.x, worldChestPos.y + 0.5f, 0f), Quaternion.identity);

    }
}



