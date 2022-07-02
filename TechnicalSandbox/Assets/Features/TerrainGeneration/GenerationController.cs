using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerationController : MonoBehaviour
{
    public Transform player;
    public int chunkSize = 8;
    public int chunkResolution = 64;

    HashSet<Vector3Int> chunksVisited;

    // Start is called before the first frame update
    void Start()
    {
        chunksVisited = new HashSet<Vector3Int>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            var playerChunkId = Vector3Int.FloorToInt(player.position / chunkSize);
            chunksVisited.Add(playerChunkId);


            var playerCoordInChunk = player.position - playerChunkId * chunkSize;
            var playerCoordFromCenterOfChunk = ((playerCoordInChunk - Vector3.one * chunkSize / 2.0f) * 2) / chunkSize;

            var nearNeighbours = Vector3Int.RoundToInt(playerCoordFromCenterOfChunk);
            
            if(nearNeighbours != Vector3Int.zero)
            {
                if(nearNeighbours.x != 0)
                {
                    chunksVisited.Add(playerChunkId + Vector3Int.right * nearNeighbours.x);
                }

                if (nearNeighbours.y != 0)
                {
                    chunksVisited.Add(playerChunkId + Vector3Int.up * nearNeighbours.y);
                }

                if (nearNeighbours.z != 0)
                {
                    chunksVisited.Add(playerChunkId + Vector3Int.forward * nearNeighbours.z);
                }
            }
            
        }
    }

    private void OnDrawGizmos()
    {
        if (chunksVisited != null)
        {
            Gizmos.color = Color.green;
            foreach (var chunk in chunksVisited)
            {
                Gizmos.DrawWireCube(chunk * chunkSize + Vector3.one * chunkSize * 0.5f, Vector3.one * chunkSize);
            }
        }
    }
}
