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

    List<Vector3> verticies; 
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
        indicies = new List<int>();
        

        float floorHeight = 0;

        float segmentWidth = Random.Range(minSegmentWidth, maxSegmentWidth);
        float segmentHeight = Random.Range(minSegmentHeight, maxSegmentHeight);
        float segmentDepth = Random.Range(minSegmentDepth, maxSegmentDepth);

        //Draw face
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



        mesh.vertices = verticies.ToArray();
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
