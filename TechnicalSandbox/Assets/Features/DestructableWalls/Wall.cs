using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //public List<Node> subNodes = new List<Node>();
    public bool isLeaf = true; // Leaf = false -> will be not triangulated
    public Bounds bounds;

    public Node[,,] subNodes = null;// = new Node[2,2,2];

    public Node()
    {
        //subNodes = new Node[2, 2, 2];
    }
}

public class Octtree
{
    Node root;
    Bounds rootBounds;

    int depthLimit;

    public Octtree(Bounds bounds, int depthLimit)
    {
        rootBounds = bounds;//bounds;
        this.depthLimit = depthLimit;
        root = new Node();
        root.bounds = rootBounds;
    }

    void Generate()
    {
        //Subdivide(root, rootBounds, 0);
    }

    public void TriangulateTree(List<Vector3> verts, List<int> triangles)
    {
        verts.Clear();
        triangles.Clear();

        RecursiveTriangulation(root, verts, triangles);
    }

    void RecursiveTriangulation(Node node, List<Vector3> verts, List<int> triangles)
    {
        if (node.isLeaf)
        {
            TriangulateNode(node, verts, triangles);
        }

        if (node.subNodes != null)
        {
            foreach (Node n in node.subNodes)
            {
                if (n != null)
                    RecursiveTriangulation(n, verts, triangles);
            }

            
        }
    }

    void TriangulateNode(Node node, List<Vector3> verts, List<int> triangles)
    {
        var center = node.bounds.center; 
        var size = node.bounds.extents; size.z = 0;
        Debug.Log( "before " + verts.Count);
        MakeQuad(verts, triangles,
                 center + Vector3.left * size.x + Vector3.down * size.y,
                 center + Vector3.left * size.x + Vector3.up * size.y,
                 center + Vector3.right * size.x + Vector3.up * size.y,
                 center + Vector3.right * size.x + Vector3.down * size.y);

        Debug.Log("after " + verts.Count);

    }

    void MakeQuad(List<Vector3> verts, List<int> triangles, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
    {
        verts.Add(bottomLeft);
        verts.Add(topLeft);
        verts.Add(topRight);
        verts.Add(bottomRight);

        triangles.Add(verts.Count - 4);
        triangles.Add(verts.Count - 3);
        triangles.Add(verts.Count - 2);

        triangles.Add(verts.Count - 2);
        triangles.Add(verts.Count - 1);
        triangles.Add(verts.Count - 4);
    }

    bool SubdivisionCheck(Bounds b)
    {
        /*if (b.Contains(new Vector3(0.2f, 0, 0)))
        {
            return true;
        }*/

        if (b.Contains(new Vector3(0.3f, -0.25f, 0)))
        {
            return true;
        }
        
        if (b.Contains(new Vector3(0.6f, 0.35f, 0)))
        {
            return true;
        }
        if (b.Contains(new Vector3(-0.1f, -0f, 0)))
        {
            return true;
        }
        if (b.Contains(new Vector3(0.1f, -0.45f, 0)))
        {
            return true;
        }

        return false;
    }

    public void RecalculateTree()
    {
        //Check that any triangles fall within the rootnode
        //If do - subdivide
        if (SubdivisionCheck(rootBounds))
        {
            Debug.Log("Initial boundry check success");
            Subdivide(root, rootBounds, 0);
        }
    } 

    //Returns true if verts are still present to stop leafification of parents
    bool Subdivide(Node currentNode, Bounds currentBounds, int depth)
    {
        Debug.Log("DEPTH " + depth);
        currentNode.bounds = currentBounds;

        if(depth == depthLimit) 
        {
            Debug.Log("Depth Limit Hit!");
            //To get in here the node must have triangles still intersecting it so it returns to tell it to not triangulate this spot later.
            currentNode.isLeaf = false; return true; 
        };

        //Check if subdivision already exists
        if(currentNode.subNodes != null)
        {
            Debug.Log("Subdiv data already exists!");
            bool canBeLeaf = true;
            //Check if each children intersect with any triangle
            for(int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    if (SubdivisionCheck(currentNode.subNodes[x, y, 0].bounds))
                    {
                        if (Subdivide(currentNode.subNodes[x, y, 0], currentNode.subNodes[x, y, 0].bounds, depth + 1))
                        {
                            canBeLeaf = false;
                        }
                    }
                }
            }

            if (canBeLeaf == false)
            {
                currentNode.isLeaf = false;
            }
            return !canBeLeaf;
        }
        else
        {
            currentNode.subNodes = new Node[2, 2, 1];
        }

        bool retVal = false;
        bool allTrue = true;
        {
            var extents = currentBounds.extents;
            extents.z = currentBounds.size.z;

            //Split into multiple bounds
            Bounds subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(0.5f, 0.5f, 0)), extents); 
            {
                var node = new Node();
                node.bounds = subBounds;
                if (SubdivisionCheck(subBounds))
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                else
                {
                    allTrue = false;
                }
                currentNode.subNodes[0, 0, 0] = node;

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-0.5f, 0.5f, 0));
            {
                var node = new Node();
                node.bounds = subBounds;

                if (SubdivisionCheck(subBounds))
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                else
                {
                    allTrue = false;
                }
                currentNode.subNodes[1, 0, 0] = node;

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(0.5f, -0.5f, 0));
            {
                var node = new Node();
                node.bounds = subBounds;

                if (SubdivisionCheck(subBounds))
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                else
                {
                    allTrue = false;
                }
                currentNode.subNodes[0, 1, 0] = node;
            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-0.5f, -0.5f, 0));
            {
                var node = new Node();
                node.bounds = subBounds;

                if (SubdivisionCheck(subBounds))
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                else
                {
                    allTrue = false;
                }

                currentNode.subNodes[1, 1, 0] = node;
            }

        }

        if(retVal)
        {
            currentNode.isLeaf = false;
        }

        return retVal;
        
        /*
        if (allTrue)
        {
            foreach (Node n in currentNode.subNodes)
            {
                if (n == null)
                {
                    return retVal;
                }

                if (!n.isLeaf)
                {
                    return retVal;
                }
            }

            currentNode.isLeaf = true;
        }
        return retVal;*/
    }

    public void DrawGizmos()
    {
        DrawRecursively(root);
    }

    //Returns true when bottommost child is true.
    bool DrawRecursively(Node node)
    {
        bool drawWithColour = false;
        if (node.subNodes != null)
        {
            foreach (Node n in node.subNodes)
            {
                if (n != null)
                    drawWithColour |= DrawRecursively(n);
            }

            if (drawWithColour == false)
            {
                drawWithColour = node.isLeaf;

            }
        }

        if (node.isLeaf)
        {
            Gizmos.color = node.isLeaf ? Color.green : Color.white;
            // if (node.isLeaf)
            //Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
            Gizmos.DrawCube(node.bounds.center, node.bounds.size);
        }
        return drawWithColour;
    }

    public void DrawRayQuery(Ray ray)
    {
        if (rootBounds.IntersectRay(ray))
        {
            var directionToOrigin = ray.origin - rootBounds.center;
            RayQuery(root, ray, directionToOrigin);
        }
    }

    bool RayQuery(Node node, Ray ray, Vector3 centerToRayOrigin)
    {
        if (node.isLeaf)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(node.bounds.center, node.bounds.size);
            return true;
        }

        int startX = centerToRayOrigin.x > 0 ? 1 : 0;
        int startY = centerToRayOrigin.y > 0 ? 1 : 0;
        int startZ = centerToRayOrigin.z > 0 ? 1 : 0;



        bool hit = false;
        for (int x = startX; x > -1 && x < 2; x += 1 - startX * 2)
        {
            for (int y = startY; y > -1 && y < 2; y += 1 - startY * 2)
            {
                for (int z = startZ; z > -1 && z < 2; z += 1 - startZ * 2)
                {
                    var n = node.subNodes[x, y, z];
                    if (n != null)
                    {
                        if (n.bounds.IntersectRay(ray))
                            hit |= RayQuery(n, ray, centerToRayOrigin);
                    }

                    if (hit) break;
                }
                if (hit) break;
            }
            if (hit) break;
        }

        if (hit)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
        }

        return hit;
    }

    List<Vector3> GetPoints(Node node)
    {
        var points = new List<Vector3>();


        if (node.isLeaf)
        {
            points.Add(node.bounds.center);
            return points;
        }

        foreach (Node n in node.subNodes)
        {
            if (n != null)
            {
                var childPoints = GetPoints(n);
                points.AddRange(childPoints);
            }
        }

        return points;
    }

    List<Color> GetPointColours(Node node)
    {
        var points = new List<Color>();


        if (node.isLeaf)
        {
            Color colour = new Color(1, 1, 1, node.bounds.size.x);
            points.Add(colour);
            return points;
        }

        foreach (Node n in node.subNodes)
        {
            if (n != null)
            {
                var childPoints = GetPointColours(n);
                points.AddRange(childPoints);
            }
        }

        return points;
    }


    public List<Vector3> GeneratePointCloud()
    {
        return GetPoints(root);
    }

    public List<Color> GenerateColourCloud()
    {
        return GetPointColours(root);
    }
}



public class Wall : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    public float wallThickness;
    public float wallHeight;

    MeshFilter filter;
    Mesh mesh;

    List<Vector3> verticies; 
    List<int> indices;

    Octtree tree;

    // Start is called before the first frame update
    void Start()
    {
        verticies = new List<Vector3>();
        indices = new List<int>();

        tree = new Octtree(new Bounds(Vector3.zero, Vector3.one * 1), 3);
        tree.RecalculateTree();
        tree.TriangulateTree(verticies, indices);

       

        mesh = new Mesh();
        mesh.vertices = verticies.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    void GenerateWallQuadTree()
    {

    }

    void GenerateWallMesh()
    {
        verticies = new List<Vector3>();
        indices = new List<int>();

    }

    void MakeQuad(Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
    {
        verticies.Add(bottomLeft);
        verticies.Add(topLeft);
        verticies.Add(topRight);
        verticies.Add(bottomRight);

        indices.Add(verticies.Count - 4);
        indices.Add(verticies.Count - 3);
        indices.Add(verticies.Count - 2);

        indices.Add(verticies.Count - 2);
        indices.Add(verticies.Count - 1);
        indices.Add(verticies.Count - 4);
    }

    private void OnDrawGizmos()
    {
        if (tree != null)
        {
            tree.DrawGizmos();
        }
    }
}
