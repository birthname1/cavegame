using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ChunkData
{
    public int[,] map;
    public Dictionary<Vector3Int, bool> editedTiles = new(); // Store edited tiles
}

public class ChunkLoader : MonoBehaviour
{
    public Dictionary<Vector2Int, ChunkData> loadedChunks = new();

    private string SavePath => Path.Combine(Application.persistentDataPath, "chunks");

    void Awake()
    {
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);
    }

    public bool TryLoadChunk(Vector2Int coord, out ChunkData chunk)
    {
        string path = Path.Combine(SavePath, $"{coord.x}_{coord.y}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ChunkDataWrapper wrapper = JsonUtility.FromJson<ChunkDataWrapper>(json);
            chunk = wrapper.ToChunkData();

            loadedChunks[coord] = chunk;
            return true;
        }
        chunk = null;
        return false;
    }

    public void SaveChunk(Vector2Int coord)
    {
        if (!loadedChunks.ContainsKey(coord)) return;

        // Get the current chunk data
        ChunkData chunkData = loadedChunks[coord];

        // Wrap the chunk data for serialization
        ChunkDataWrapper wrapper = new ChunkDataWrapper(chunkData);

        // Serialize the wrapper to JSON
        string json = JsonUtility.ToJson(wrapper);

        // Define the path to save the chunk
        string path = Path.Combine(SavePath, $"{coord.x}_{coord.y}.json");

        // Write the JSON data to the file
        File.WriteAllText(path, json);
    }

    public void UnloadChunk(Vector2Int coord)
    {
        SaveChunk(coord);
        loadedChunks.Remove(coord);
    }
    void OnApplicationQuit()
    {
        // Loop through all the loaded chunks and save them before quitting
        foreach (var chunkCoord in loadedChunks.Keys)
        {
            SaveChunk(chunkCoord);
        }
    }
}
[Serializable]
public class ChunkDataWrapper
{
    public int[] flatMap;
    public int width;
    public int height;
    public List<EditedTile> editedTiles;

    public ChunkDataWrapper(ChunkData data)
    {
        width = data.map.GetLength(0);
        height = data.map.GetLength(1);
        flatMap = new int[width * height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                flatMap[y * width + x] = data.map[x, y];

        editedTiles = new List<EditedTile>();
        foreach (var kvp in data.editedTiles)
        {
            editedTiles.Add(new EditedTile { position = kvp.Key, placed = kvp.Value });
        }
    }

    public ChunkData ToChunkData()
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = flatMap[y * width + x];

        var editedTilesDict = new Dictionary<Vector3Int, bool>();
        foreach (var editedTile in editedTiles)
        {
            editedTilesDict[editedTile.position] = editedTile.placed;
        }

        return new ChunkData { map = map, editedTiles = editedTilesDict };
    }
}


[Serializable]
public class EditedTile
{
    public Vector3Int position;
    public bool placed;
}

