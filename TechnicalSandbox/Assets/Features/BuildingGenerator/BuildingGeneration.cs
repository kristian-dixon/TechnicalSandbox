using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BuildingGeneration : MonoBehaviour
{

    MeshFilter filter;

    Mesh mesh;

    public int maxSegments = 5;

    public float minSegmentWidth = 1, minSegmentHeight = 1, minSegmentDepth = 1;
    public float maxSegmentWidth = 10, maxSegmentHeight = 10, maxSegmentDepth = 10;

    public float spireChance = 0.3f;
    public float expandChance = 0.2f;
    public float expandAmount = 2;

    List<Vector3> verticies;
    List<Vector2> uvs;

    List<int> indicies;

    // Start is called before the first frame update
    void Start()
    {
        filter = GetComponent<MeshFilter>();
        GenerateBuilding();
    }

    public void GenerateBuilding()
    {
        mesh = new Mesh();
        verticies = new List<Vector3>();
        uvs = new List<Vector2>();
        indicies = new List<int>();
        

        float floorHeight = 0;

        float segmentWidth = Random.Range(maxSegmentWidth - minSegmentWidth, maxSegmentWidth);
        float segmentHeight = Random.Range(minSegmentHeight, maxSegmentHeight);
        float segmentDepth = Random.Range(maxSegmentDepth - minSegmentDepth, maxSegmentDepth);

        int shouldSpire = Random.Range(0f, 1f) < spireChance ? 1 : 0;



        for (int i = 0; i < maxSegments + shouldSpire; i++)
        {
            var nearBottomLeft = new Vector3(-segmentWidth / 2, floorHeight, -segmentDepth / 2);
            var nearTopLeft = new Vector3(-segmentWidth / 2, floorHeight + segmentHeight, -segmentDepth / 2);
            var nearTopRight = new Vector3(segmentWidth / 2, floorHeight + segmentHeight, -segmentDepth / 2);
            var nearBottomRight = new Vector3(segmentWidth / 2, floorHeight, -segmentDepth / 2);
            var farBottomLeft = new Vector3(-segmentWidth / 2, floorHeight, segmentDepth / 2);
            var farTopLeft = new Vector3(-segmentWidth / 2, floorHeight + segmentHeight, segmentDepth / 2);
            var farTopRight = new Vector3(segmentWidth / 2, floorHeight + segmentHeight, segmentDepth / 2);
            var farBottomRight = new Vector3(segmentWidth / 2, floorHeight, segmentDepth / 2);

            MakeQuad(nearBottomLeft, nearTopLeft, nearTopRight, nearBottomRight);
            MakeQuad(farBottomLeft, farTopLeft, nearTopLeft, nearBottomLeft);
            MakeQuad(farBottomRight, farTopRight, farTopLeft, farBottomLeft);
            MakeQuad(nearBottomRight, nearTopRight, farTopRight, farBottomRight);

            float expansion = Random.Range(0f, 1f) < expandChance ? expandAmount : 0;

            floorHeight += segmentHeight + Random.Range(0f, 5f) + expansion;


            segmentWidth = Random.Range(segmentWidth - minSegmentWidth, segmentWidth + expansion);
            segmentHeight = Random.Range(minSegmentHeight, ( maxSegmentHeight));
            segmentDepth = Random.Range(segmentDepth - minSegmentDepth, segmentDepth + expansion);

            if(i == maxSegments - 1 && shouldSpire == 0)// - 1 && shouldSpire == 0)
            {
                segmentWidth = 0;
                segmentHeight = 0;
                segmentDepth = 0;
            }
            else if(i == maxSegments - 1)
            {
                segmentWidth = 0.2f;
                segmentHeight = Random.Range(0.5f, 10f);
                segmentDepth = 0.2f;

            }

            if(i == maxSegments)
            {
                segmentWidth = 0;
                segmentHeight = 0;
                segmentDepth = 0;
            }

            //Connect each floor
            nearBottomLeft = nearTopLeft;
            nearBottomRight = nearTopRight;
            farBottomLeft = farTopLeft;
            farBottomRight = farTopRight;

            nearTopLeft = new Vector3(-segmentWidth / 2, floorHeight, -segmentDepth / 2);
            nearTopRight = new Vector3(segmentWidth / 2, floorHeight, -segmentDepth / 2);
            farTopLeft = new Vector3(-segmentWidth / 2, floorHeight, segmentDepth / 2);
            farTopRight = new Vector3(segmentWidth / 2, floorHeight, segmentDepth / 2);


            MakeQuad(nearBottomLeft, nearTopLeft, nearTopRight, nearBottomRight);
            MakeQuad(farBottomLeft, farTopLeft, nearTopLeft, nearBottomLeft);
            MakeQuad(farBottomRight, farTopRight, farTopLeft, farBottomLeft);
            MakeQuad(nearBottomRight, nearTopRight, farTopRight, farBottomRight);

        }


        mesh.vertices = verticies.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = indicies.ToArray();
        mesh.RecalculateNormals();
        filter.mesh = mesh;
    }

    void MakeQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
    {
        verticies.Add(bottomLeft);
        verticies.Add(topLeft);
        verticies.Add(topRight);
        verticies.Add(bottomRight);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        indicies.Add(verticies.Count - 4);
        indicies.Add(verticies.Count - 3);
        indicies.Add(verticies.Count - 2);

        indicies.Add(verticies.Count - 2);
        indicies.Add(verticies.Count - 1);
        indicies.Add(verticies.Count - 4);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.G))
        {
            GenerateBuilding();
        }
    }
}
