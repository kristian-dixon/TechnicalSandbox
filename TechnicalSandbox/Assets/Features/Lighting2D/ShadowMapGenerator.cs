using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowMapGenerator : MonoBehaviour
{
    public Transform light;
    Mesh collisionMesh;
    Mesh shadowMesh;
    MeshFilter mf;

    // Start is called before the first frame update
    void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {
        shadowMesh = new Mesh();


        List<Vector3> verts = new List<Vector3>();
        List<int> inds = new List<int>();


        if (collisionMesh == null)
        {
            TilemapCollider2D collider = FindObjectOfType<TilemapCollider2D>();
            collisionMesh = collider.CreateMesh(true, true);
        }
        for(int i = 0; i < collisionMesh.triangles.Length; i += 3)
        {
            CreateShadow(collisionMesh.vertices[collisionMesh.triangles[i]], collisionMesh.vertices[collisionMesh.triangles[i + 1]], light.position, verts, inds);
            CreateShadow(collisionMesh.vertices[collisionMesh.triangles[i]], collisionMesh.vertices[collisionMesh.triangles[i + 2]], light.position, verts, inds);
            CreateShadow(collisionMesh.vertices[collisionMesh.triangles[i + 1]], collisionMesh.vertices[collisionMesh.triangles[i + 2]], light.position, verts, inds);
        }

        
        shadowMesh.SetVertices(verts);
        shadowMesh.SetIndices(inds, MeshTopology.Triangles, 0);
        
        mf = GetComponent<MeshFilter>();
        mf.mesh = shadowMesh;

    }
    void CreateShadow(Vector3 pointA, Vector3 pointB, Vector3 lightPos, List<Vector3> verts, List<int> inds)
    {
        int size = verts.Count;
        verts.Add(pointA);
        verts.Add(pointA + (pointA - lightPos) * 100);
        verts.Add(pointB);
        verts.Add(pointB + (pointB - lightPos) * 100);

        inds.Add(size + 0);
        inds.Add(size + 2);
        inds.Add(size + 1);

        inds.Add(size + 2);
        inds.Add(size + 3);
        inds.Add(size + 1);
    }

}
