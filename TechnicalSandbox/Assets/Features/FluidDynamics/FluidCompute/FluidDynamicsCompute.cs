using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidDynamicsCompute : MonoBehaviour
{
    public ComputeShader shader;

    ComputeBuffer densityPrevBuffer;
    ComputeBuffer densityBuffer;

    ComputeBuffer xVelBuffer;
    ComputeBuffer yVelBuffer;
    ComputeBuffer xVelPrevBuffer;
    ComputeBuffer yVelPrevBuffer;

    public int resolution = 256;
    public bool inclZ = false;

    RenderTexture rt;
    int kernelId;
    // Start is called before the first frame update
    void Start()
    {
        kernelId = shader.FindKernel("CSMain");

        rt = new RenderTexture(resolution, resolution, 1);
        rt.enableRandomWrite = true;
        rt.Create();

        var imgDisplay = GetComponent<UnityEngine.UI.RawImage>();
        imgDisplay.texture = rt;

        CreateBufferWithRandomData(densityBuffer); 
        CreateBufferWithRandomData(densityPrevBuffer); 
        CreateBufferWithRandomData(xVelBuffer, -1,1); 
        CreateBufferWithRandomData(xVelPrevBuffer, -1,1); 
        CreateBufferWithRandomData(yVelBuffer, -1,1); 
        CreateBufferWithRandomData(yVelPrevBuffer, -1,1);
 
        
        shader.SetInt("size", resolution);
        shader.SetBuffer(kernelId, "density", densityBuffer);
        shader.SetBuffer(kernelId, "densityPrev", densityPrevBuffer);
        shader.SetTexture(kernelId, "Result", rt);

    }

    void CreateBufferWithRandomData(ComputeBuffer buffer, float rngMin = 0, float rngMax = 1)
    {
        if(buffer == null)
        {
            buffer = new ComputeBuffer(resolution * resolution, sizeof(float));
        }

        var buffData = new float[resolution * resolution];
        for (int i = 0; i < resolution * resolution; i++)
        {
            buffData[i] = Random.Range(rngMin, rngMax);
        }
        buffer.SetData(buffData);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.Space))
        {
            shader.Dispatch(kernelId, resolution / 8, resolution / 8, 1);//256 / 8, 256 / 8, 1);

        }
    }

    private void OnDestroy()
    {
        densityBuffer.Dispose();
        densityPrevBuffer.Dispose();

        xVelBuffer.Dispose();
        yVelBuffer.Dispose();
        yVelPrevBuffer.Dispose();
        xVelPrevBuffer.Dispose();
    }
}
