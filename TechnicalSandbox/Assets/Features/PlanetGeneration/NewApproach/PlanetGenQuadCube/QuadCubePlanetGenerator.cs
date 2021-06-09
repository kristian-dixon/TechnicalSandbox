using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseTextures
{
    public float displacementAmount = 1f;
    public Texture2D texture = null;
}


public class QuadCubePlanetGenerator : MonoBehaviour
{
    public List<NoiseTextures> noiseTextures;

    /*MeshFilter filter;
    new MeshRenderer renderer;

    public Material material;
    Mesh mesh;

    List<Vector3> verts;
    List<int> indices;
    */
    public float quadScale = 1f;

    //Start generating

    //LVL0
    //Generate whole map at low detail - store

    //LVL1
    //Split into 4 - generate each of these at the same LOD as above 
    Color[] pixels;



    // Start is called before the first frame update
    void Start()
    {
        var tex = noiseTextures[0].texture;
        pixels = tex.GetPixels(0, 0, 512, 512);

        var lvl0 = new GameObject();

        GenerateTerrainChunk(Vector2Int.zero, 1.0f, noiseTextures[0].displacementAmount).transform.parent = lvl0.transform;

        var lvl1 = new GameObject();
        GenerateTerrainChunk(Vector2Int.zero, 0.5f, noiseTextures[0].displacementAmount).transform.parent = lvl1.transform;
        GenerateTerrainChunk(Vector2Int.right * 31, 0.5f, noiseTextures[0].displacementAmount).transform.parent = lvl1.transform;
        GenerateTerrainChunk(Vector2Int.up * 31, 0.5f, noiseTextures[0].displacementAmount).transform.parent = lvl1.transform;
        GenerateTerrainChunk(Vector2Int.one * 31, 0.5f, noiseTextures[0].displacementAmount).transform.parent = lvl1.transform;

        var lvl2 = new GameObject();
        GenerateTerrainChunk(Vector2Int.zero, 0.25f, noiseTextures[0].displacementAmount).transform.parent = lvl2.transform;
        GenerateTerrainChunk(Vector2Int.right * 31, 0.25f, noiseTextures[0].displacementAmount).transform.parent = lvl2.transform;
        GenerateTerrainChunk(Vector2Int.up * 31, 0.25f, noiseTextures[0].displacementAmount).transform.parent = lvl2.transform;
        GenerateTerrainChunk(Vector2Int.one * 31, 0.25f, noiseTextures[0].displacementAmount).transform.parent = lvl2.transform;

        return;

        /*
        Mesh mesh = new Mesh();

        filter = gameObject.AddComponent<MeshFilter>();

        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = material;


        verts = new List<Vector3>(); indices = new List<int>();

        
        float displacement = noiseTextures[0].displacementAmount;

        for (int z = 0; z < 100; z++)
        {
            Vector3 botLeft = Vector3.forward * z * quadScale + Vector3.up * pixels[512 * z].r * displacement;
            Vector3 topLeft = Vector3.forward * (z + 1) * quadScale + Vector3.up * pixels[512 * (z + 1)].r * displacement;

            Vector3 fwdAmnt = Vector3.forward * z * quadScale;

            for (int x = 1; x < 100; x++)
            {
                Vector3 botRight = fwdAmnt + Vector3.right * x * quadScale + Vector3.up * pixels[x + (z * 512)].r * displacement;
                Vector3 topRight = fwdAmnt + Vector3.forward * quadScale + Vector3.right * x * quadScale + Vector3.up * pixels[x + ((z + 1) * 512)].r * displacement;

                Utils.TriangulateQuad(botLeft, topLeft, topRight, botRight, verts, indices);

                botLeft = botRight;
                topLeft = topRight;
            }
        }

        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        filter.mesh = mesh;
        */
    }

    public MeshFilter prefab; 
    int cellCount = 32;
    Transform GenerateTerrainChunk(Vector2Int startPosition, float scale, float height)
    {
        var instance = Instantiate(prefab);
        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        // we need to know how big each step is between each cell boundry. Ideally if we're using
        // the full res of the texture this would be ((512 / cellCount) * scale) with scale being .5 .25 .125 etc.
        int textureLookupStepScale = (int)((512 / cellCount) * scale); //Casted to int for hopefully easier lookup
        float cellPhysicalScale = scale * quadScale;

        Vector3 position = new Vector3(startPosition.x, 0, startPosition.y) * cellPhysicalScale;
        //instance.transform.position = position;

        for(int z = 1 + startPosition.y; z < cellCount + startPosition.y; z++)
        {
            Vector3 topLeft = (Vector3.right * cellPhysicalScale * startPosition.x) + (Vector3.forward * cellPhysicalScale * z) + 
                               Vector3.up * height * pixels[512 * textureLookupStepScale * (z) + startPosition.x * textureLookupStepScale].r;
            
            Vector3 bottomLeft = (Vector3.right * cellPhysicalScale * startPosition.x) + (Vector3.forward * cellPhysicalScale * (z - 1)) +
                               Vector3.up * height * pixels[512 * textureLookupStepScale * (z - 1) + startPosition.x * textureLookupStepScale].r;

            for (int x = 1 + startPosition.x; x < cellCount + startPosition.x; x++)
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

        mesh.SetVertices(verts);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        instance.mesh = mesh;

        return instance.transform;
    }
}
