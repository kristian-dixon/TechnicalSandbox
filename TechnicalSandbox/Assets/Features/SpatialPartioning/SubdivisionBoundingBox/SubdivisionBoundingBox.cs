using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    //public List<Node> subNodes = new List<Node>();
    public bool isLeaf = false;
    public Bounds bounds;

    public Node[,,] subNodes;// = new Node[2,2,2];

    public Node()
    {
        subNodes = new Node[2, 2, 2];
    }
} 

public class Octtree 
{
    Node root;
    Vector3[] vertices;

    Bounds rootBounds;

    int depthLimit;

    Texture3D texture;

    Vector3Int tCursor;

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
                currentNode.subNodes[0,0,0] = node;

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes[1,0,0] = node;

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                currentNode.subNodes[0, 1, 0] = node;
            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, 1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;
                }
                currentNode.subNodes[0, 0, 1] = node;
            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, 1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }

                currentNode.subNodes[1, 1, 0] = node;
            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes[1,1,1] = (node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes[1,0,1] = (node);

            }

            subBounds.center = currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, -1)) * 0.5f;
            {
                var node = new Node();
                if (Subdivide(node, subBounds, depth + 1))
                {
                    retVal = true;

                }
                currentNode.subNodes[0,1,1] = node;

            }
        }

        return retVal;
    }


    public void CreateTexture(Material material = null)
    {
        tCursor = Vector3Int.zero;
        texture = new Texture3D(256, 256, 256, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        //Call function
        for(int x = 0; x < 256; x++)
            for(int y = 0; y < 256; y++)
                for(int z = 0; z < 256; z++)
                {
                    texture.SetPixel(x, y, z, new Color(0f, y / 255f, z / 255f, 1));
                }



        PaintTexture(root);

       /* texture.SetPixel(0, 0, 0, new Color(0.0f, 0.0f, 0.0f, 1));
        texture.SetPixel(1, 0, 0, new Color(1.0f, 0.0f, 0.0f, 1));
        texture.SetPixel(0, 1, 0, new Color(0.0f, 1.0f, 0.0f, 1));
        texture.SetPixel(0, 0, 1, new Color(0.0f, 0.0f, 1.0f, 1));
        texture.SetPixel(1, 1, 0, new Color(1.0f, 1.0f, 0.0f, 1));
        texture.SetPixel(0, 1, 1, new Color(0.0f, 1.0f, 1.0f, 1));
        texture.SetPixel(1, 1, 1, new Color(1.0f, 1.0f, 1.0f, 1));
        */

        texture.Apply();
        if (material)
            material.SetTexture("_TreeTex", texture);
    }

    void PaintTexture(Node node)
    {
        var children = node.subNodes;

        var cursorStartPos = tCursor;

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    var n = children[x, y, z];
                    
                    var tpl = new Vector3Int(x, y, z) + cursorStartPos;
                    if (n != null)
                    {
                        if (n.isLeaf)
                        {
                            texture.SetPixel(tpl.x, tpl.y, tpl.z, new Color(1, UnityEngine.Random.Range(0.0f,1f), UnityEngine.Random.Range(0, 1f), 1));
                        }
                        else
                        {
                            tCursor.x += 2;
                            if (tCursor.x >= texture.width)
                            {
                                tCursor.x -= texture.width;

                                tCursor.y += 2;
                                if (tCursor.y >= texture.height)
                                {
                                    tCursor.y -= texture.height;

                                    tCursor.z += 2;
                                    if (tCursor.z >= texture.depth)
                                    {
                                        return;
                                    }
                                }
                            }
                            texture.SetPixel(tpl.x, tpl.y, tpl.z, new Color(tCursor.x / 255f, tCursor.y / 255f, tCursor.z / 255f, 0));


                            PaintTexture(n);
                        }
                    }
                    else
                    {
                        texture.SetPixel(tpl.x, tpl.y, tpl.z, new Color(0, 0, 0, 1));
                    }
                }
            }
        }

    }


    public void DrawGizmos()
    {
        DrawRecursively(root);
    }

    //Returns true when bottommost child is true.
    bool DrawRecursively(Node node)
    {
        bool drawWithColour = false;

        foreach(Node n in node.subNodes)
        {
            if(n != null)
                drawWithColour |= DrawRecursively(n);
        }

        if (drawWithColour == false)
        {
            drawWithColour = node.isLeaf;
        }

        Gizmos.color = drawWithColour ? Color.green : Color.white;
        Gizmos.DrawWireCube( node.bounds.center, node.bounds.size);
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
        if(node.isLeaf)
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
                    if(n != null)
                    {
                        if(n.bounds.IntersectRay(ray))
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
    
}

public class SubdivisionBoundingBox : MonoBehaviour
{
    public Mesh mesh;

    public int depthLimit = 4;

    Vector3[] vertices;

    Octtree tree = null;

    public Transform rayOrigin, rayTarget;

    public bool drawFullTree = false;
    public bool drawRayQuery = false;

    public Renderer volumeRenderer;

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

        tree = new Octtree(originalBounds, mesh, depthLimit);

        var material = FindObjectOfType<PostProcessingFilter>().EffectMaterial;
        tree.CreateTexture(material);
        
        //tree.CreateTexture(volumeRenderer);
    }

    private void OnDrawGizmos()
    {
        if (tree == null)
        {
            return;
        }

        if (drawFullTree)
        {
            tree.DrawGizmos();
        }

        if (drawRayQuery)
        {
            if (rayOrigin && rayTarget)
            {
                Ray ray = new Ray(rayOrigin.position, Vector3.Normalize(rayTarget.position - rayOrigin.position));

                tree.DrawRayQuery(ray);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(rayOrigin.position, rayTarget.position);
            }
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
