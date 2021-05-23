using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlanetaryQuadTree
{
    public int recursionLimit;
    public PlanetaryNode rootNode;
    Vector3 right;
    Vector3 up;

    public PlanetaryQuadTree(int recursionLimit, float planetRadius, Vector3 right, Vector3 up)
    {
        this.right = right;
        this.up = up;

        this.recursionLimit = recursionLimit;

        rootNode = new PlanetaryNode(Vector3.Cross(up, right) * planetRadius, planetRadius, 0, right, up, planetRadius);
    }


    /// <param name="playerPosition"> Player position is in local space :) </param>
    public bool UpdatePlanetDetail(Vector3 playerPosition)
    {
        return rootNode.UpdatePlanetDetail(playerPosition, 0, recursionLimit);
    }

    public void TriangulateTree(List<Vector3> verts, List<int> tris)
    {
        rootNode.TriangulateNode(verts, tris);
    }
}

[System.Serializable]
public class PlanetaryNode 
{
    public PlanetaryNode[,] nodes;
    Vector3 center;
    public float scale;
    Vector3 right;
    Vector3 up;

    float planetRadius;

    public PlanetaryNode(Vector3 center, float scale, int depth, Vector3 right, Vector3 up, float planetRadius)
    {
        nodes = null;
        this.center = center;
        this.scale = scale;
        this.right = right;
        this.up = up;
        this.planetRadius = planetRadius;
    }

    //Returns true if the mesh needs rebuilding.
    public bool UpdatePlanetDetail(Vector3 playerPosition, int depth, int depthLimit)
    {
        if (depth == depthLimit)
        {
            Debug.Log("DepthLimitHit");
            return false;
        }

        if(Vector3.SqrMagnitude(center - playerPosition) < scale * scale)
        {
            bool retVal = false;
            if(nodes == null)
            {
                //Create Nodes
                nodes = new PlanetaryNode[2, 2];
                nodes[0, 0] = new PlanetaryNode(center + (-right + -up) * scale / 2f, scale / 4f, depth + 1, right, up, planetRadius);
                nodes[1, 0] = new PlanetaryNode(center + (right + -up) * scale / 2f, scale / 4f, depth + 1, right, up, planetRadius);
                nodes[0, 1] = new PlanetaryNode(center + (-right + up) * scale / 2f, scale / 4f, depth + 1, right, up, planetRadius);
                nodes[1, 1] = new PlanetaryNode(center + (right + up) * scale / 2f, scale / 4f, depth + 1, right, up, planetRadius);

                retVal = true; //Because new quads are being added
            }

            //Check if nodes need updating
            foreach(var node in nodes)
            {
                retVal |= node.UpdatePlanetDetail(playerPosition, depth + 1, depthLimit);
            }
            return retVal;
        }
        else
        {
            if(nodes != null)
            {
                nodes = null;
                return true;
            }
            return false;
        }
    }

    public void TriangulateNode(List<Vector3> vertices, List<int> indices)
    {
        if (nodes == null)
        {
            Vector3 bl = center + (-right + -up) * scale / 1f;
            Vector3 br = center + (right + -up) * scale / 1f;
            Vector3 tl = center + (-right + up) * scale / 1f;
            Vector3 tr = center + (right + up) * scale / 1f;

            MakeSphereizedQuad(bl, tl, tr, br, vertices, indices);
        }
        else
        {
            foreach (var node in nodes)
            {
                node.TriangulateNode(vertices, indices);
            }
        }
    }

    void MakeSphereizedQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, List<Vector3> vertices, List<int> indices)
    {
        var radius = planetRadius;
        bottomLeft = bottomLeft.normalized * radius;
        bottomRight = bottomRight.normalized * radius;
        topLeft = topLeft.normalized * radius;
        topRight = topRight.normalized * radius;

        vertices.Add(bottomLeft);
        vertices.Add(topLeft);
        vertices.Add(topRight);
        vertices.Add(bottomRight);


        indices.Add(vertices.Count - 4);
        indices.Add(vertices.Count - 3);
        indices.Add(vertices.Count - 2);

        indices.Add(vertices.Count - 2);
        indices.Add(vertices.Count - 1);
        indices.Add(vertices.Count - 4);
    }
}


public class CubeBasedSphere : MonoBehaviour
{

    public PlanetaryQuadTree frontTree;
    public PlanetaryQuadTree leftTree;
    public PlanetaryQuadTree rightTree;
    public PlanetaryQuadTree backTree;
    public PlanetaryQuadTree topTree;
    public PlanetaryQuadTree bottomTree;

    public Transform player;

    public int subdivisions = 5;
    public float radius = 5;

    Mesh mesh;
    List<Vector3> vertices;
    List<int> indices;

    bool isRunning = false;

    MeshFilter filter;

    private void OnValidate()
    {
        if (isRunning)
        {
            
            //Update mesh
            UpdateMesh();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        frontTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.right, Vector3.up);
        backTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.left, Vector3.up);
        rightTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.forward, Vector3.up);
        leftTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.back, Vector3.up);
        bottomTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.left, Vector3.forward);
        topTree = new PlanetaryQuadTree(subdivisions, radius, Vector3.right, Vector3.forward);


        filter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        filter.mesh = mesh;
        UpdateMesh();

        isRunning = true;
    }

    void UpdateMesh()
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        //tree.TriangulateTree(vertices, indices);

        frontTree.TriangulateTree(vertices, indices);
        backTree.TriangulateTree(vertices, indices); 
        rightTree.TriangulateTree(vertices, indices); 
        leftTree.TriangulateTree(vertices, indices); 
        bottomTree.TriangulateTree(vertices, indices); 
        topTree.TriangulateTree(vertices, indices); 
        
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();

        Debug.Log(vertices.Count);
        Debug.Log(indices.Count);

    }

    private void Update()
    {
        if (player)
        {
            bool dirtyMesh = false;

            dirtyMesh |= frontTree.UpdatePlanetDetail(player.position);
            dirtyMesh |= backTree.UpdatePlanetDetail(player.position);
            dirtyMesh |= leftTree.UpdatePlanetDetail(player.position);
            dirtyMesh |= rightTree.UpdatePlanetDetail(player.position);
            dirtyMesh |= topTree.UpdatePlanetDetail(player.position);
            dirtyMesh |= bottomTree.UpdatePlanetDetail(player.position);



            if (dirtyMesh) 
            {
                UpdateMesh();
            }
        }
    }

}
