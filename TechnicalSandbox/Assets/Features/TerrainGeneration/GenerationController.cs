using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationController : MonoBehaviour
{
    public Transform player;

    public TerrainChunk terrainChunkPrefab;
    public int chunkSize = 8;
    public int chunkResolution = 64;

    Dictionary<Vector3Int, TerrainChunk> chunksVisited;

    // Start is called before the first frame update
    void Start()
    {
        chunksVisited = new Dictionary<Vector3Int, TerrainChunk>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            var playerChunkId = Vector3Int.FloorToInt(player.position / chunkSize);
            UpdateChunksAroundPlayer(playerChunkId);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.forward);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.back);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.left);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.right);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.up);
            UpdateChunksAroundPlayer(playerChunkId + Vector3Int.down);


            if(chunksVisited.ContainsKey(playerChunkId))
            {
                int strength = 0;


                if (Input.GetMouseButton(0))
                {
                    strength = 1;
                }
                else if(Input.GetMouseButton(1))
                {
                    strength = -1;
                }

                if(strength != 0)
                {
                    TerrainChunk chunk;
                    if(chunksVisited.TryGetValue(playerChunkId, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.left, out  chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.right, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.up, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.down, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.forward, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }

                    if (chunksVisited.TryGetValue(playerChunkId + Vector3Int.back, out chunk))
                    {
                        chunk.UpdateVolume(strength, 1, player.position);
                    }
                }
            }    
            
        }

        
    }

    private void UpdateChunksAroundPlayer(Vector3Int playerChunkId)
    {
        if (!chunksVisited.ContainsKey(playerChunkId))
        {
            GenerateChunk(playerChunkId);
        }

        var playerCoordInChunk = player.position - playerChunkId * chunkSize;
        var playerCoordFromCenterOfChunk = ((playerCoordInChunk - Vector3.one * chunkSize / 2.0f) * 2) / chunkSize;

        var nearNeighbours = Vector3Int.RoundToInt(playerCoordFromCenterOfChunk);

        if (nearNeighbours != Vector3Int.zero)
        {
            if (nearNeighbours.x != 0)
            {
                if (!chunksVisited.ContainsKey(playerChunkId))
                {
                    GenerateChunk(playerChunkId + Vector3Int.right * nearNeighbours.x);
                }
            }

            if (nearNeighbours.y != 0)
            {
                if (!chunksVisited.ContainsKey(playerChunkId))
                {
                    GenerateChunk(playerChunkId + Vector3Int.up * nearNeighbours.y);
                }
            }

            if (nearNeighbours.z != 0)
            {
                if (!chunksVisited.ContainsKey(playerChunkId))
                {
                    GenerateChunk(playerChunkId + Vector3Int.forward * nearNeighbours.z);
                }
            }
        }
    }

    void GenerateChunk(Vector3Int chunkId)
    {
        var terrainChunk = Instantiate(terrainChunkPrefab, transform);
        chunksVisited.Add(chunkId, terrainChunk);

        GameObject gameObject = terrainChunk.gameObject; 
        gameObject.name = "CHUNK: " + chunkId.ToString();
        gameObject.transform.parent = transform;

        float cubeSize = (float)chunkSize / chunkResolution;

        Vector3 chunkBasePosition = (chunkId * chunkSize);
        Vector3 chunkOffset = (Vector3)chunkId * cubeSize;
        gameObject.transform.position = chunkBasePosition - chunkOffset;

        terrainChunk.size = chunkSize;
        terrainChunk.resolution = chunkResolution;

        terrainChunk.Init();
    }

    private void OnDrawGizmos()
    {
        if (chunksVisited != null)
        {
            Gizmos.color = Color.green;
            foreach (var chunk in chunksVisited)
            {
                Gizmos.DrawWireCube(chunk.Key * chunkSize + Vector3.one * chunkSize * 0.5f, Vector3.one * chunkSize);
            }
        }
    }
}
