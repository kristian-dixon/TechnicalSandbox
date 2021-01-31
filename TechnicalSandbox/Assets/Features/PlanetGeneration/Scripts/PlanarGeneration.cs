using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Based on: https://web.archive.org/web/20160309182309/http://freespace.virgin.net/hugo.elias/models/m_landsp.htm

public class PlanarGeneration : MonoBehaviour
{
    public float startingHeight = 2.5f;
    public float heightChange = 0.1f;

    public int maxIteration = 1000;

    Mesh mesh;
    Vector3[] originalVerticies;
    Vector3[] newVerticies;
    float[] heights;

    Vector3 plane;


    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVerticies = mesh.vertices;
        newVerticies = new Vector3[originalVerticies.Length];
        heights = new float[originalVerticies.Length];
        for(int i = 0; i < heights.Length; i++)
        {
            heights[i] = startingHeight;
        }

        StartCoroutine(GeneratePlanet());
    }

    Vector3 RandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * Random.Range(-0.5f, 0.5f);
    }

    IEnumerator GeneratePlanet()
    {
        int iter = 0;
        while (iter < maxIteration)
        {
            var planarDirection = RandomDirection();
            float weight = Random.Range(0, 2) * 2 - 1;
            Debug.Log(weight);

            for (int i = 0; i < originalVerticies.Length; i++)
            {
                float sign = Vector3.Dot(planarDirection, originalVerticies[i] - planarDirection);

                /*if (i == 0)
                    Debug.Log(sign);
                    */
                if (sign < 0)
                {
                    heights[i] -= heightChange * weight;
                }
                else
                {
                    heights[i] += heightChange * weight;
                }

                if (iter % 20 == 0)
                    newVerticies[i] = originalVerticies[i] * heights[i];
            }

            if (iter % 20 == 0)
            {
                mesh.vertices = newVerticies;
                mesh.RecalculateNormals();
            }
            yield return null;
        }

       

        
    }

    
}
