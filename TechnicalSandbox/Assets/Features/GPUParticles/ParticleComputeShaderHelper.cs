using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleComputeShaderHelper : MonoBehaviour
{
    public ComputeShader cs;

    RenderTexture render;

    RawImage image;
    ComputeBuffer particleBuffer;

    public int particleCount = 512;

    public Color clearColour = Color.black;

    // Start is called before the first frame update
    void Start()
    {
        var kernelID = cs.FindKernel("BufferSetup");
        image = GetComponent<RawImage>();

        render = new RenderTexture(512*4, 512*4, 1);
        render.enableRandomWrite = true;
        render.Create();
        image.texture = render;


        cs.SetTexture(cs.FindKernel("RenderTextureSetup"), "Result", render);
        cs.SetVector("clearColour", clearColour);
        cs.Dispatch(cs.FindKernel("RenderTextureSetup"), render.width / 8, render.height / 8, 1);

        particleBuffer = new ComputeBuffer(particleCount, (sizeof(float)) * 4);
        cs.SetVector("renderResolution", Vector2.one * render.width);
        cs.SetBuffer(kernelID, "particles", particleBuffer);
        cs.SetTexture(kernelID, "Result", render);
        cs.Dispatch(kernelID, particleCount / 64, 1, 1);//256 / 8, 256 / 8, 1);

        //Setup other buffers that don't need to be stored multiple times :) 
        kernelID = cs.FindKernel("CSMain");
        cs.SetBuffer(kernelID, "particles", particleBuffer);
        cs.SetTexture(kernelID, "Result", render);

        cs.SetTexture(cs.FindKernel("RenderTextureFade"), "Result", render);
    }

    // Update is called once per frame
    void Update()
    {
        var kernelID = cs.FindKernel("CSMain");
        cs.SetFloat("dt", Time.deltaTime);
        cs.Dispatch(kernelID, particleCount / 64, 1, 1);//256 / 8, 256 / 8, 1);


        cs.SetFloat("dt", Time.deltaTime);
        cs.Dispatch(cs.FindKernel("RenderTextureFade"), render.width / 8, render.height / 8, 1);

    }


    private void OnDestroy()
    {
        particleBuffer.Dispose();
    }

    private void OnDisable()
    {
        particleBuffer.Dispose();
    }


}
