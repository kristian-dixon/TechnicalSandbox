using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentParticles : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material material;

    public int particleCount = 1200;
    public float startRadius = 0.1f;

    public string kernelName = "CSMain";

    bool isInitialised = false;
    int runtimeParticleCount = 0;
    int kernelID;
    uint threadGroupsX;
    ComputeBuffer particleBuffer;

    ComputeBuffer indirectArgs;

    // Start is called before the first frame update
    void Start()
    {
        if(!computeShader)
        {
            this.enabled = false;
            return;
        }

        runtimeParticleCount = particleCount;
        computeShader.FindKernel(kernelName);

        InitBuffers();
    }

    void InitBuffers()
    {
        //Size = float3 pos, float3 velocity
        particleBuffer = new ComputeBuffer(particleCount, sizeof(float) * 6);
        indirectArgs = new ComputeBuffer(1, sizeof(uint) * 4, ComputeBufferType.IndirectArguments);

        float[] particles = new float[particleCount * 6];
        for(int i = 0; i < particleCount; i++)
        {
            int baseIndex = i * 6;
            particles[baseIndex + 0] = Random.Range(-1f,1f) * startRadius;
            particles[baseIndex + 1] = Random.Range(-1f,1f) * startRadius;
            particles[baseIndex + 2] = Random.Range(-1f,1f) * startRadius;
            particles[baseIndex + 3] = 0;//Random.Range(0,1) * startRadius;
            particles[baseIndex + 4] = 0;//Random.Range(0,1) * startRadius;
            particles[baseIndex + 5] = 0;//Random.Range(0,1) * startRadius;
        }

        particleBuffer.SetData(particles);
        computeShader.SetBuffer(kernelID, "_ParticleBuffer", particleBuffer);
        computeShader.SetInt("_ParticleCount", particleCount);
        indirectArgs.SetData(new int[]{particleCount, 1, 0, 0});

        computeShader.GetKernelThreadGroupSizes(kernelID, out threadGroupsX, out _, out _);

        material.SetBuffer("_ParticleBuffer", particleBuffer);
        isInitialised = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isInitialised) return;
        
        computeShader.SetFloat("_DeltaTime", Time.deltaTime);
        computeShader.Dispatch(kernelID, Mathf.CeilToInt((float) particleCount / threadGroupsX), 1, 1);

        Graphics.DrawProceduralIndirect(material, new Bounds(Vector3.zero, Vector3.one * 10), MeshTopology.Points, indirectArgs, 0, null, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);
    }

    void OnDestroy()
    {
        if(particleBuffer != null)
        {
            particleBuffer.Dispose();
        }
    }
}
