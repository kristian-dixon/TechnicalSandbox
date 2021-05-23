using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

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

    List<Vector3> triangles = new List<Vector3>();

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
        return BoundsTriangleIntersection(b, triangles);
    }

    //Taken from: https://gdbooks.gitbooks.io/3dcollisions/content/Chapter4/aabb-triangle.html
    bool BoundsTriangleIntersection(Bounds b, List<Vector3> triangles)
    {
        for(int i = 0; i < triangles.Count; i+=3)
        {
            var v0 = triangles[i];
            var v1 = triangles[i + 1];
            var v2 = triangles[i + 2];

            var c = b.center;
            var e = b.extents;


            // Translate the triangle as conceptually moving the AABB to origin
            // This is the same as we did with the point in triangle test
            v0 -= c;
            v1 -= c;
            v2 -= c;

            // Compute the edge vectors of the triangle  (ABC)
            // That is, get the lines between the points as vectors
            Vector3 f0 = v1 - v0; // B - A
            Vector3 f1 = v2 - v1; // C - B
            Vector3 f2 = v0 - v2; // A - C

            // Compute the face normals of the AABB, because the AABB
            // is at center, and of course axis aligned, we know that 
            // it's normals are the X, Y and Z axis.
            Vector3 u0 = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 u1 = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 u2 = new Vector3(0.0f, 0.0f, 1.0f);

            // There are a total of 13 axis to test!

            // We first test against 9 axis, these axis are given by
            // cross product combinations of the edges of the triangle
            // and the edges of the AABB. You need to get an axis testing
            // each of the 3 sides of the AABB against each of the 3 sides
            // of the triangle. The result is 9 axis of seperation
            // https://awwapp.com/b/umzoc8tiv/

            // Compute the 9 axis
            Vector3 axis_u0_f0 = Vector3.Cross(u0, f0);
            Vector3 axis_u0_f1 = Vector3.Cross(u0, f1);
            Vector3 axis_u0_f2 = Vector3.Cross(u0, f2);

            Vector3 axis_u1_f0 = Vector3.Cross(u1, f0);
            Vector3 axis_u1_f1 = Vector3.Cross(u1, f1);
            Vector3 axis_u1_f2 = Vector3.Cross(u2, f2);

            Vector3 axis_u2_f0 = Vector3.Cross(u2, f0);
            Vector3 axis_u2_f1 = Vector3.Cross(u2, f1);
            Vector3 axis_u2_f2 = Vector3.Cross(u2, f2);

            if (SeperatingAxisTest(axis_u0_f0, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u0_f1, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u0_f2, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u1_f0, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u1_f1, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u1_f2, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u2_f0, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u2_f1, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(axis_u2_f2, e, v0, v1, v2, u0, u1, u2) == false) continue;

            if (SeperatingAxisTest(u0, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(u1, e, v0, v1, v2, u0, u1, u2) == false) continue;
            if (SeperatingAxisTest(u2, e, v0, v1, v2, u0, u1, u2) == false) continue;

            Vector3 triangleNormal = Vector3.Cross(f0, f1);
            if (SeperatingAxisTest(triangleNormal, e, v0, v1, v2, u0, u1, u2) == false) continue;

            return true;
        }
        return false;
    }

    public bool SeperatingAxisTest(Vector3 axis, Vector3 e, Vector3 v0, Vector3 v1, Vector3 v2,
                                   Vector3 u0, Vector3 u1, Vector3 u2)
    {
        // Testing axis: axis_u0_f0
        // Project all 3 vertices of the triangle onto the Seperating axis
        float p0 = Vector3.Dot(v0, axis);
        float p1 = Vector3.Dot(v1, axis);
        float p2 = Vector3.Dot(v2, axis);
        // Project the AABB onto the seperating axis
        // We don't care about the end points of the prjection
        // just the length of the half-size of the AABB
        // That is, we're only casting the extents onto the 
        // seperating axis, not the AABB center. We don't
        // need to cast the center, because we know that the
        // aabb is at origin compared to the triangle!
        float r = e.x * Abs(Vector3.Dot(u0, axis)) +
                    e.y * Abs(Vector3.Dot(u1, axis)) +
                    e.z * Abs(Vector3.Dot(u2, axis));
        // Now do the actual test, basically see if either of
        // the most extreme of the triangle points intersects r
        // You might need to write Min & Max functions that take 3 arguments
        if (Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            // This means BOTH of the points of the projected triangle
            // are outside the projected half-length of the AABB
            // Therefore the axis is seperating and we can exit
            return false;
        }
        return true;
    }

    public void RecalculateTree(List<Vector3> triangles)
    {
        this.triangles = triangles;

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
        for(int i = 0; i < triangles.Count; i++)
        {
            Gizmos.DrawLine(triangles[i], triangles[(i + 1) % triangles.Count]);

        }

        //DrawRecursively(root);
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
    public int subdivisionLimit = 5;
    public float triangleScale = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        verticies = new List<Vector3>();
        indices = new List<int>();

        tree = new Octtree(new Bounds(Vector3.zero, Vector3.one * 1), subdivisionLimit);
        
        tree.TriangulateTree(verticies, indices);

       

        mesh = new Mesh();
        mesh.vertices = verticies.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.G))
        {
            Vector3 center = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * 0.5f;

            tree.RecalculateTree(new List<Vector3> { 
                center + new Vector3(Random.Range(-1f * triangleScale, 1f * triangleScale),
                                     Random.Range(-1f * triangleScale, 1f * triangleScale), 0),
                center + new Vector3(Random.Range(-1f * triangleScale, 1f * triangleScale),
                                     Random.Range(-1f * triangleScale, 1f * triangleScale), 0),
                center + new Vector3(Random.Range(-1f * triangleScale, 1f * triangleScale),
                                     Random.Range(-1f * triangleScale, 1f * triangleScale), 0)
            });

            tree.TriangulateTree(verticies, indices);

            mesh = new Mesh();
            mesh.vertices = verticies.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            filter = GetComponent<MeshFilter>();
            filter.mesh = mesh;
        }
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
