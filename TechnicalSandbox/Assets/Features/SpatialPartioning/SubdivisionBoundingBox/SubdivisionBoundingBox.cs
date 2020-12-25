using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> subNodes = new List<Node>();
    public bool isLeaf = false;
    public Bounds bounds;
} 

public class Octtree 
{
    Node root;
    Vector3[] vertices;

    Bounds rootBounds;

    int depthLimit;

    public Octtree(Bounds bounds, Mesh mesh, int depthLimit)
    {
        rootBounds = bounds;
        vertices = mesh.vertices;
        this.depthLimit = depthLimit;
        Generate();
    }

    void Generate()
    {
        root = new Node();
        Subdivide(root, rootBounds, 0);
    }

    bool Subdivide(Node currentNode, Bounds currentBounds, int depth)
    {
        currentNode.bounds = currentBounds;
        bool areVerticiesPresentInBounds = false;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (currentBounds.Contains(vertices[i]))
            {
                areVerticiesPresentInBounds = true;
                break;
            }
        }

        if (!areVerticiesPresentInBounds)
        {
            return false;
        }

        if (depth == depthLimit)
        {
            currentNode.isLeaf = true;
            return true;
        }

        bool retVal = false;
        if (areVerticiesPresentInBounds)
        {
            //Split into multiple bounds
            Bounds subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, 1, 1)) * 0.5f, currentBounds.extents);
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;


                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, 1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes.Add(node);

            }
        }

        return retVal;
    }

    public void DrawGizmos()
    {
        DrawRecursively(root);
    }

    //Returns true when bottommost child is true.
    bool DrawRecursively(Node node)
    {
        bool drawWithColour = false;
        for(int i = 0; i < node.subNodes.Count; i++)
        {
            drawWithColour |= DrawRecursively(node.subNodes[i]);
        }

        if(node.subNodes.Count == 0)
        {
            drawWithColour = node.isLeaf;
        }

        Gizmos.color = drawWithColour ? Color.green : Color.white;
        Gizmos.DrawWireCube( node.bounds.center, node.bounds.size);
        return drawWithColour;
    }

}

public class SubdivisionBoundingBox : MonoBehaviour
{
    public Mesh mesh;

    public int depthLimit = 4;

    Vector3[] vertices;

    Octtree tree = null;

    void Start()
    {
        CreateBoundingBoxes();
    }


    void CreateBoundingBoxes()
    {
        if (mesh == null) return;
        var originalBounds = mesh.bounds;

        float max = Mathf.Max(Mathf.Max(originalBounds.size.x, originalBounds.size.y), originalBounds.size.z);

        originalBounds.size = Vector3.one * max;

        tree = new Octtree(originalBounds, mesh, 4);
    }

    private void OnDrawGizmos()
    {
        if (tree != null)
        {
            tree.DrawGizmos();
        }

        /*Bounds origin = new Bounds(Vector3.zero, new Vector3(20, 25, 7));

        Gizmos.DrawWireCube(origin.center, origin.size);

        for (int i = 0; i < 3; i++)
        {
            Bounds subBounds = new Bounds(origin.center - Vector3.Scale(origin.extents, new Vector3(1,-1,-1)) * 0.5f, origin.extents);
            Gizmos.DrawWireCube(subBounds.center, subBounds.size);
            origin = subBounds;
        }
        */

        /*for(int i = 0; i < bounds.Count; i++)
        {
            Bounds subBounds = bounds[i];
            Gizmos.DrawWireCube(transform.TransformPoint(subBounds.center), Vector3.Scale(transform.localScale, subBounds.size));
        }*/
    }
}
