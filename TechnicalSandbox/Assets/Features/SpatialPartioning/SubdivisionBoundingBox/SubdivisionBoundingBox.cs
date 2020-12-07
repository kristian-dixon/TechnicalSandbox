using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubdivisionBoundingBox : MonoBehaviour
{
    public Mesh mesh;

    public int depthLimit = 4;
    List<Bounds> bounds;

    Vector3[] vertices;

    void CreateBoundingBoxes()
    {
        if (mesh == null) return;
        var originalBounds = mesh.bounds;
        bounds = new List<Bounds>();
        vertices = mesh.vertices;
        bounds.Add(originalBounds);
        A(originalBounds, 0);
    }

    void A(Bounds currentBounds, int depth)
    {
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
            return;
        }

        if (depth == depthLimit)
        {
            bounds.Add(currentBounds);
            return;
        }

        

        if (areVerticiesPresentInBounds)
        {
            //Split into multiple bounds
            Bounds subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, 1, 1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);
            
            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, 1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, 1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, 1, -1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, 1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, -1, -1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(-1, 1, -1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);

            subBounds = new Bounds(currentBounds.center - Vector3.Scale(currentBounds.extents, new Vector3(1, -1, -1)) * 0.5f, currentBounds.extents);
            A(subBounds, depth + 1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        CreateBoundingBoxes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        /*Bounds origin = new Bounds(Vector3.zero, new Vector3(20, 25, 7));

        Gizmos.DrawWireCube(origin.center, origin.size);

        for (int i = 0; i < 3; i++)
        {
            Bounds subBounds = new Bounds(origin.center - Vector3.Scale(origin.extents, new Vector3(1,-1,-1)) * 0.5f, origin.extents);
            Gizmos.DrawWireCube(subBounds.center, subBounds.size);
            origin = subBounds;
        }
        */

        for(int i = 0; i < bounds.Count; i++)
        {
            Bounds subBounds = bounds[i];
            Gizmos.DrawWireCube(transform.TransformPoint(subBounds.center), Vector3.Scale(transform.localScale, subBounds.size));
        }
    }
}
