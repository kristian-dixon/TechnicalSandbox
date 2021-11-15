using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class GreedyCell
{
    public bool filled;
    public bool assessed;

    public GreedyCell(bool isFilled)
    {
        filled = isFilled;
        assessed = false;
    }
}


public class GreedyFromTexture : MonoBehaviour
{
    public Texture2D referenceTexture;

    GreedyCell[,] greedyMap;

    MeshFilter filter;
    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        if (!referenceTexture)
        {
            return;
        }

        InitMap();
        GenerateGeometry();
    }

    private void InitMap()
    {
        greedyMap = new GreedyCell[referenceTexture.width, referenceTexture.height];
        var pixels = referenceTexture.GetPixels();
        for (int x = 0; x < referenceTexture.width; x++)
        {
            for (int y = 0; y < referenceTexture.height; y++)
            {
                greedyMap[x, y] = new GreedyCell(pixels[x + referenceTexture.width * y].r > 0.5f);
            }
        }
    }

    async void GenerateGeometry()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        RectInt rect = new RectInt(0,0,0,0);
        bool expanding = false;
        for (int y = 0; y < greedyMap.GetLength(1); y++)
        {
            for(int x = 0; x < greedyMap.GetLength(0); x++)
            {

                if (!greedyMap[x,y].assessed && greedyMap[x,y].filled)
                {
                    await Task.Delay(100);

                    //Start filling quad
                    GenerateQuad(x, y);
                    greedyMap[x, y].assessed = true;
                }
            }
        }
    }

    void GenerateQuad(int startX, int startY)
    {
        RectInt rect = new RectInt(startX, startY, 1, 1);

        //Find quad width
        for (int x = startX + 1; x < greedyMap.GetLength(1); x++)
        {
            if (greedyMap[x, startY].assessed || !greedyMap[x, startY].filled)
            {
                break;
            }
            else
            {
                greedyMap[x, startY].assessed = true;
                rect.width++;
            }
        }

        //Find blockage below
        for (int y = startY + 1; y < greedyMap.GetLength(1); y++)
        {
            bool escape = false;
            for (int x = startX; x < startX + rect.width; x++)
            {
                if (greedyMap[x, y].assessed || !greedyMap[x, y].filled)
                {
                    escape = true;
                    break;
                }
            }

            if(escape)
            {
                break;
            }

            for (int x = startX; x < startX + rect.width; x++)
            {
                greedyMap[x, y].assessed = true;
            }

            rect.height++;
        }

        CreateQuad(rect);
    }


    List<RectInt> quads = new List<RectInt>();
    private void CreateQuad(RectInt quad)
    {
        if (quads == null) quads = new List<RectInt>();
        quads.Add(quad);
    }

    public int test = 0;

    private void OnDrawGizmos()
    {
        if(quads != null)
        {
            for(int i = 0; i < quads.Count && i < test; i++)
            {
                var quad = quads[i];
                Gizmos.DrawCube(new Vector3(quad.center.x, quad.center.y), new Vector3(quad.width, quad.height));
            }
        }
        /*
        if(greedyMap != null)
        {
            for(int y = 0; y < greedyMap.GetLength(1); y++)
            {
                for (int x = 0; x < greedyMap.GetLength(1); x++)
                {
                    if(greedyMap[x,y].assessed)
                    {
                        Gizmos.color = Color.green;
                    }
                    else
                    {
                        Gizmos.color = Color.red;

                    }

                    Gizmos.DrawWireCube(new Vector3(x, y), new Vector3(1,1,1));

                }
            }
        }*/
    }
}
