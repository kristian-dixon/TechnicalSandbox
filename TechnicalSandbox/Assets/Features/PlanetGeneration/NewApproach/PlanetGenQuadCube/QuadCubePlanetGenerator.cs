using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseTextures
{
    public float displacementAmount = 1f;
    public Texture2D texture = null;
}


public class MeshTreeNode
{
    public MeshTreeNode()
    {
        children = new MeshTreeNode[2, 2];
    }

    public GameObject generatedMesh;
    public Bounds bounds;
    public MeshTreeNode[,] children;
}

public class TerrainFace 
{
    Mesh mesh;
    int resolution;

    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;

    public TerrainFace(Mesh mesh, Vector3 localUp, int resolution)
    {
        this.mesh = mesh;
        this.localUp = localUp;
        axisA = new Vector3(this.localUp.y, this.localUp.z, this.localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        this.resolution = resolution;
    }

    public void ConstructMesh()
    {
        if (mesh == null) return;

        Vector3[] verticies = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 positionOnUnitCube = localUp + axisA * (percent.x - 0.5f) * 2 + axisB * (percent.y - 0.5f) * 2;
                Vector3 pointOnUnitSphere = positionOnUnitCube.normalized;

                verticies[i] = pointOnUnitSphere;

                if(x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + resolution + 1;
                    triangles[triIndex++] = i + resolution;

                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + resolution + 1;

                }
            }
        }

        mesh.Clear();
        mesh.SetVertices(verticies);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();

    }
}


public class QuadCubePlanetGenerator : MonoBehaviour
{
    public Transform player;
    public List<NoiseTextures> noiseTextures;

    public MeshFilter prefab;
    int cellCount = 32;

    public float quadScale = 1f;
    Color[] pixels;

    MeshTreeNode rootNode;

    [Header("New Stuff :)")]
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    [Range(2,256)]
    public int resolution = 3;
    // Start is called before the first frame update
    void Start()
    {
        /*var tex = noiseTextures[0].texture;
        pixels = tex.GetPixels(0, 0, 512, 512);
        var lvl0 = new GameObject();
        */



        


        
        /*var bigMesh = GenerateTerrainChunk(Vector2Int.zero, 1.0f, noiseTextures[0].displacementAmount).gameObject;

        rootNode = new MeshTreeNode();
        rootNode.generatedMesh = bigMesh;
        */


        //GenerateTree(rootNode, Vector2Int.zero, 0, 0.5f);
    }

    private void OnValidate()
    {
        Initialise();
        GenerateMesh();
    }

    void Initialise()
    {
        if(meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.forward, Vector3.right, Vector3.back, Vector3.down, Vector3.left };
        for(int i = 0; i < 6; i++)
        {
            if(meshFilters[i] == null)
            {
                var meshFilter = Instantiate(prefab, transform);
                meshFilters[i] = meshFilter;
                meshFilter.sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], resolution);
        }
    }

    void GenerateMesh()
    {
        foreach(var face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }

    
    public float heightScale = 0.1f;
    void GenerateUnitSquare()
    {
        
    }

    public int depthLimit = 4;
    void GenerateTree(MeshTreeNode node, Vector2Int offset, int depth, float scale)
    {
        if (depth >= depthLimit)
        {
            return;
        }

        offset *= 2;


        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                var childNode = node.children[x, z] = new MeshTreeNode();
                var cell = (new Vector2Int(x, z) + offset) * 31 ;
                var chunk = GenerateTerrainChunk(cell, scale, noiseTextures[0].displacementAmount);
                chunk.name += depth;
                childNode.generatedMesh = chunk.gameObject;
                childNode.bounds = chunk.mesh.bounds;
                childNode.bounds.Expand(childNode.bounds.size);
                GenerateTree(childNode, (new Vector2Int(x, z) + offset) , depth + 1, scale * 0.5f);
            }
        }
    }

    private void Update()
    {
        return;
        TreeRender();
    }

    void TreeRender()
    {
        if(TreeRenderCheck(player.position, rootNode) == false)
        {
            rootNode.generatedMesh.SetActive(true);
        }
    }

    bool TreeRenderCheck(Vector3 playerPos, MeshTreeNode node)
    {
        if (node == null) return false;
        node.generatedMesh.SetActive(false);

        List<MeshTreeNode> preventRenderList = new List<MeshTreeNode>();
        
        foreach(var child in node.children)
        {
            if(TreeRenderCheck(playerPos, child))
            {
                preventRenderList.Add(child);
            }
        }

        if (preventRenderList.Count > 0)
        {
            foreach (var child in node.children)
            {
                if (preventRenderList.Contains(child)) continue;

                child.generatedMesh.SetActive(true);
            }

            return true;
        }

        if (node.bounds.Contains(playerPos))
        {
            node.generatedMesh.SetActive(true);
            return true;
        }

        return false;
    }

    //Section is rendered if player is inside bounds

    MeshFilter GenerateTerrainChunk(Vector2Int startPosition, float scale, float height)
    {
        var instance = Instantiate(prefab);
        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        // we need to know how big each step is between each cell boundry. Ideally if we're using
        // the full res of the texture this would be ((512 / cellCount) * scale) with scale being .5 .25 .125 etc.
        int textureLookupStepScale = (int)((512 / cellCount) * scale); //Casted to int for hopefully easier lookup
        float cellPhysicalScale = scale * quadScale;

        //Vector3 position = new Vector3(startPosition.x, 0, startPosition.y) * cellPhysicalScale;
        //instance.transform.position = position;

        //
        for(int z = 2 + startPosition.y; z < cellCount + startPosition.y - 1; z++)
        {
            int x = 1 + startPosition.x;
            Vector3 topLeft = (Vector3.right * cellPhysicalScale * x) + (Vector3.forward * cellPhysicalScale * z) +
                              Vector3.up * height * pixels[512 * textureLookupStepScale * (z) + x * textureLookupStepScale].r;

            Vector3 bottomLeft = (Vector3.right * cellPhysicalScale * x) + (Vector3.forward * cellPhysicalScale * (z - 1)) +
                                   Vector3.up * height * pixels[512 * textureLookupStepScale * (z - 1) + x * textureLookupStepScale].r;
            x++;
            for (; x < cellCount + startPosition.x - 1; x++)
            {
                Vector3 topRight = (Vector3.right * cellPhysicalScale * x) + (Vector3.forward * cellPhysicalScale * z) +
                              Vector3.up * height * pixels[512 * textureLookupStepScale * (z) + x * textureLookupStepScale].r;

                Vector3 bottomRight = (Vector3.right * cellPhysicalScale * x) + (Vector3.forward * cellPhysicalScale * (z - 1)) +
                                   Vector3.up * height * pixels[512 * textureLookupStepScale * (z - 1) + x * textureLookupStepScale].r;

                Utils.TriangulateQuad(bottomLeft, topLeft, topRight, bottomRight, verts, indices);

                topLeft = topRight;
                bottomLeft = bottomRight;
            }
        }

        //Trying to handle strips
        //Left side
        //Debug.Log(cellPhysicalScale);
        float smallCellScale = Mathf.Pow(0.5f, depthLimit) * quadScale;
        int stripSize = (int)(cellPhysicalScale / smallCellScale);

        //Make a bunch of quads that cover the stip at the scale of the smallest possible cell
        float stripLength = (cellCount * cellPhysicalScale) / smallCellScale;
        int smallTextureLookupStepScale = (int)((512 / cellCount) * Mathf.Pow(0.5f, depthLimit));

        Vector3 offset = new Vector3(startPosition.x * cellPhysicalScale, 0, startPosition.y * cellPhysicalScale);

        Debug.Log(stripSize);

        //Left strip
        for(int z = 2; z < stripLength - (stripSize * 2 - 1); z++)
        {
            int x = 0;

            int cell = (x * smallTextureLookupStepScale) + (startPosition.x * textureLookupStepScale) + (z * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);
            int cellPrev = (x * smallTextureLookupStepScale) + (startPosition.x * textureLookupStepScale) + ((z - 1) * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);

            //Debug.Log("X = " + (x * smallTextureLookupStepScale + startPosition.x * textureLookupStepScale) + " Y = " + (z * smallTextureLookupStepScale + startPosition.y * textureLookupStepScale));

            Vector3 topLeft = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * z) + offset
                + Vector3.up * height * 1 * pixels[cell].r;

            Vector3 bottomLeft = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * (z - 1)) + offset
                + Vector3.up * height * pixels[cellPrev].r;


            x++;
            for (; x <= stripSize - 1; x++)
            {
                cell = (x * smallTextureLookupStepScale) + (startPosition.x * textureLookupStepScale) + (z * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);
                cellPrev = (x * smallTextureLookupStepScale) + (startPosition.x * textureLookupStepScale) + ((z - 1) * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);

                Vector3 topRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * z) + offset
                    +Vector3.up * height * pixels[cell].r;

                Vector3 bottomRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * (z - 1)) + offset
                    + Vector3.up * height * pixels[cellPrev].r;
                //512 * textureLookupStepScale * (int)(prevTextureCellZ) + rightXCell
                Utils.TriangulateQuad(bottomLeft, topLeft, topRight, bottomRight, verts, indices);

                topLeft = topRight;
                bottomLeft = bottomRight;
            }


            

            //Direct connection
            {
                float textureCellZ = z * (smallCellScale / cellPhysicalScale) + startPosition.y;
                float prevTextureCellZ = (z - 1) * (smallCellScale / cellPhysicalScale) + startPosition.y;
                int xCell = ((1 + startPosition.x) * textureLookupStepScale);

                float topRightCellFloor = pixels[xCell + (512 * textureLookupStepScale * (int)Mathf.Floor(textureCellZ))].r * height;
                float topRightCellCeil  = pixels[xCell + (512 *  ((textureLookupStepScale * (int)Mathf.Ceil(textureCellZ)) % 512) )].r * height;
                float topRightHeight = Mathf.Lerp(topRightCellFloor, topRightCellCeil, textureCellZ % 1);

                //
                float bottomRightCellFloor = pixels[xCell + (512 * textureLookupStepScale * (int)Mathf.Floor(prevTextureCellZ))].r * height;
                float bottomRightCellCeil = pixels[xCell + (512 * ( (textureLookupStepScale * (int)Mathf.Ceil(prevTextureCellZ)) % 512))].r * height;

                float bottomRightHeight = Mathf.Lerp(bottomRightCellFloor, bottomRightCellCeil, prevTextureCellZ % 1); 

                Vector3 topRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * z) + offset + Vector3.up * topRightHeight;
                //+Vector3.up * height * pixels[512 * textureLookupStepScale * (z) + x * textureLookupStepScale].r;

                Vector3 bottomRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * (z - 1)) + offset + Vector3.up * bottomRightHeight;
                //+ Vector3.up * height * pixels[512 * textureLookupStepScale * (z - 1) + x * textureLookupStepScale].r;

                Utils.TriangulateQuad(bottomLeft, topLeft, topRight, bottomRight, verts, indices);
                if(startPosition == Vector2Int.zero)
                {
                    //Debug.Log(z + ":" + (int)bottomRightHeight);
                }
            }

        }

        offset = new Vector3((cellCount - 2 + startPosition.x) * cellPhysicalScale, 0, startPosition.y * cellPhysicalScale);

        //right strip
        for (int z = 2; z < stripLength - (stripSize * 2 - 1); z++)
        {
            int x = 0;

            Vector3 topLeft = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * z) + offset;
            //+ Vector3.up * height * pixels[512 * textureLookupStepScale * (z) + x * textureLookupStepScale].r;

            Vector3 bottomLeft = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * (z - 1)) + offset;
            //+ Vector3.up * height * pixels[512 * textureLookupStepScale * (z - 1) + x * textureLookupStepScale].r;
            
            int xCell = ((cellCount + startPosition.x - 2) * textureLookupStepScale);
            
            //Direct connection to main mesh
            float prevTextureCellZ = (z - 1) * (smallCellScale / cellPhysicalScale) + startPosition.y;
            float textureCellZ = z * (smallCellScale / cellPhysicalScale) + startPosition.y;
            { 
                float topRightCellFloor = pixels[xCell + (512 * textureLookupStepScale * (int)Mathf.Floor(textureCellZ))].r * height;
                float topRightCellCeil = pixels[xCell + (512 * ((textureLookupStepScale * (int)Mathf.Ceil(textureCellZ)) % 512))].r * height;
                float topRightHeight = Mathf.Lerp(topRightCellFloor, topRightCellCeil, textureCellZ % 1);

                float bottomRightCellFloor = pixels[xCell + (512 * textureLookupStepScale * (int)Mathf.Floor(prevTextureCellZ))].r * height;
                float bottomRightCellCeil = pixels[xCell + (512 * ((textureLookupStepScale * (int)Mathf.Ceil(prevTextureCellZ)) % 512))].r * height;

                float bottomRightHeight = Mathf.Lerp(bottomRightCellFloor, bottomRightCellCeil, prevTextureCellZ % 1);
                topLeft.y += topRightHeight;
                bottomLeft.y += bottomRightHeight;
            }


            x++;
            for (; x <= stripSize; x++)
            {
                int rightXCell = xCell + x * smallTextureLookupStepScale;
                int cell = (x * smallTextureLookupStepScale) + (xCell) + (z * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);
                int cellPrev = (x * smallTextureLookupStepScale) + (xCell) + ((z - 1) * 512 * smallTextureLookupStepScale) + (startPosition.y * 512 * textureLookupStepScale);


                Vector3 topRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * z) + offset
                + Vector3.up * 1 * height * pixels[cell].r;

                Vector3 bottomRight = (Vector3.right * smallCellScale * x) + (Vector3.forward * smallCellScale * (z - 1)) + offset
                + Vector3.up * 1 * height * pixels[cellPrev].r;

                Utils.TriangulateQuad(bottomLeft, topLeft, topRight, bottomRight, verts, indices);

                topLeft = topRight;
                bottomLeft = bottomRight;
            }



            

        }


        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        instance.mesh = mesh;

        return instance;
    }

    private void OnDrawGizmos()
    {
        TreeGizmoRender(rootNode);
    }

    void TreeGizmoRender(MeshTreeNode node)
    {
        if (node != null)
        {
            if(node.generatedMesh.activeSelf)
            {
                Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
            }

            foreach (var item in node.children)
            {
                TreeGizmoRender(item);
            }
        }
    }
}
