using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class ShellRenderer : MonoBehaviour
{
    [System.Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;

        public Vertex(Mesh m, int triIdx)
        {
            position = m.vertices[triIdx];
            normal = m.normals[triIdx];
            uv = m.uv[triIdx];

        }
    }
    
    [System.Serializable, StructLayout(LayoutKind.Sequential)]
    public struct InputTriangle
    {
        public Vertex a;
        public Vertex b;
        public Vertex c;
    }


    // float * (pos3,normal3,uv2) * 3 verts
    const int INPUT_TRIANGLE_SIZE = sizeof(float) * (3 + 3 + 2 ) * 3;

    // float * (pos3,normal3,uv2,col4) * 3 verts
    const int OUTPUT_TRIANGLE_SIZE = sizeof(float) * (3 + 3 + 2 + 4) * 3;

    const int INDIRECT_ARGS_SIZE = sizeof(uint) * 4;

    ComputeBuffer inputTriangleBuffer;
    ComputeBuffer outputTriangleBuffer;
    ComputeBuffer indirectArgsBuffer;
    
    List<InputTriangle> inputTriangles = new List<InputTriangle>();
    Mesh mesh;

    int triangleCount = 0;

    public ComputeShader compute;
    public Material material;

    [Min(1)]
    public int layers = 1; 
    public float offset = 1;

    int kernelIndex = 0;
    bool isInitalised = false;

    MeshRenderer renderer;

    void Start()
    {
        Init();
    }

    void OnDisable()
    {
        ReleaseBuffers();
    }

    void OnValidate()
    {
        if(!Application.isPlaying) return;
        ReleaseBuffers();
        Init();
    }

    void Init()
    {
        renderer = GetComponent<MeshRenderer>();
        isInitalised = false;
        mesh = GetComponent<MeshFilter>().mesh;
        triangleCount = mesh.triangles.Length / 3;

        InitBuffers();
        InitData();
        GenerateGeometry();

        isInitalised = true;
    }

    void InitBuffers()
    {
        if(mesh == null) return;

        inputTriangleBuffer = new ComputeBuffer(triangleCount, INPUT_TRIANGLE_SIZE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        outputTriangleBuffer = new ComputeBuffer(triangleCount * triangleCount, OUTPUT_TRIANGLE_SIZE, ComputeBufferType.Append);
        outputTriangleBuffer.SetCounterValue(0);

        indirectArgsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_SIZE, ComputeBufferType.IndirectArguments);
    }

    void InitData()
    {
        if(mesh == null) return;
        inputTriangles = new List<InputTriangle>();


        for(int i = 0; i < mesh.triangles.Length; i+=3)
        {
            InputTriangle t = new InputTriangle();

            t.a = new Vertex(mesh, mesh.triangles[i]);
            t.b = new Vertex(mesh, mesh.triangles[i + 1]);
            t.c = new Vertex(mesh, mesh.triangles[i + 2]);

            inputTriangles.Add(t);

        }

        inputTriangleBuffer.SetData(inputTriangles);

        //inputTriangles.Clear();
        //outputTriangleBuffer.SetData(inputTriangles);
        indirectArgsBuffer.SetData(new int[]{0, 1, 0, 0});
    }

    void GenerateGeometry()
    {
        compute.FindKernel("CSMain");
        compute.GetKernelThreadGroupSizes(kernelIndex, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)triangleCount / threadGroupSizeX);

        compute.SetBuffer(kernelIndex, "_InputTrianglesBuffer", inputTriangleBuffer);
        compute.SetBuffer(kernelIndex, "_OutputTrianglesBuffer", outputTriangleBuffer);
        compute.SetBuffer(kernelIndex, "_IndirectArgsBuffer", indirectArgsBuffer);

        compute.SetFloat("_Offset", offset);        
        compute.SetInt("_Layers", layers);
        compute.SetInt("_TriangleCount", triangleCount);


        material.SetBuffer("_TrianglesBuffer", outputTriangleBuffer);
        compute.Dispatch(kernelIndex, threadGroupSize, 1, 1);
    }

    void Update()
    {
        if(isInitalised)
        {

            Graphics.DrawProceduralIndirect(material, renderer.bounds, MeshTopology.Triangles, indirectArgsBuffer, 0, null, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);
        }
    }


    void ReleaseBuffers()
    {
        ReleaseBuffer(inputTriangleBuffer);
        ReleaseBuffer(outputTriangleBuffer);
        ReleaseBuffer(indirectArgsBuffer);

        isInitalised = false;
    }

    void ReleaseBuffer(ComputeBuffer buff)
    {
        if(buff != null)
        {
            buff.Release();
        }
    }

}
