using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosBallsHelper : MonoBehaviour
{
    public ComputeShader cs;

    RenderTexture trails;
    RenderTexture result;

    ComputeBuffer ballBuffer;
    public int textureResolution = 256;


    public int particleCount = 64;
    public float gravityStrength = 0.2f;


    int physicsKernelId;
    int mainKernelId;

    // Start is called before the first frame update
    void Start()
    {
        physicsKernelId = cs.FindKernel("CSPhysicsSim");
        mainKernelId = cs.FindKernel("CSMain");

        trails = new RenderTexture(textureResolution, textureResolution, 1);
        trails.enableRandomWrite = true;
        trails.Create();

        result = new RenderTexture(textureResolution, textureResolution, 1);
        result.enableRandomWrite = true;
        result.Create();
        cs.SetTexture(mainKernelId, "Result", result);
        cs.SetTexture(mainKernelId, "Trails", trails);
        cs.SetTexture(physicsKernelId, "Trails", trails);

        ballBuffer = new ComputeBuffer(particleCount, 4 * sizeof(float));
        List<float> ballBufferData = new List<float>();
        for(int i = 0; i < particleCount; i++)
        {
            ballBufferData.Add(0.0f + Random.Range(-0.001f, 0.001f));
            ballBufferData.Add(0.0f + Random.Range(-0.001f, 0.001f));
            ballBufferData.Add(0);
            ballBufferData.Add(0);
        }
        ballBuffer.SetData(ballBufferData);
        
        cs.SetBuffer(physicsKernelId, "balls", ballBuffer);
        cs.SetBuffer(mainKernelId, "balls", ballBuffer);
        cs.SetInt("textureSize", textureResolution);
        var img = GetComponent<UnityEngine.UI.RawImage>();
        img.texture = result;
    }

    // Update is called once per frame
    void Update()
    {
        cs.SetFloat("deltaTime", Time.deltaTime);
        cs.SetFloat("gravityStrength", -gravityStrength);

        cs.Dispatch(physicsKernelId, particleCount / 64, 1, 1);
        cs.Dispatch(mainKernelId, textureResolution / 8, textureResolution / 8, 1);
    }

    private void OnDestroy()
    {
        ballBuffer.Dispose();
    }
}
