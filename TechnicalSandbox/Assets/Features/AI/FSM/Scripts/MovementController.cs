using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.AI.FSM
{
    public class MovementController: MonoBehaviour
    {
        //0 == wall
        //1 == empty
        public Texture2D mapTex;
        public GameObject wallPrefab;
        public GameObject floorPrefab;
        bool[,] map;

        private void Start()
        {
            GenerateMap();
        }

        void GenerateMap()
        {
            if(mapTex != null)
            {
                var pixels = mapTex.GetPixels();
                for(int x = 0; x < mapTex.width; x++)
                {
                    for(int y = 0; y < mapTex.height; y++)
                    {
                        if(pixels[x + y * mapTex.width] == Color.black)
                        {
                            Instantiate(wallPrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                        }
                    }
                }

                var floor = Instantiate(floorPrefab, new Vector3(mapTex.width / 2f, -1, mapTex.height / 2f), Quaternion.identity, transform);
                floor.transform.localScale = new Vector3(mapTex.width, 0, mapTex.height);
            }
        }

        public void GetRouteForTarget(Entity entity)
        {
        }
    }
}
