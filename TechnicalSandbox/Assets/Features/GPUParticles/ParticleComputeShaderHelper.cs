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


    int selectedAttractorIdx = 0;
    string[] attractors = new[] { "ThreeCellCNNAttractor", "HalvorsenAttractor", "NoseHooverAttractor", "MouseAttractor" };

    int attractorKernelID = -1;
    int fadeKernelID = -1;

    Camera cam;

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

        particleBuffer = new ComputeBuffer(particleCount, (sizeof(float)) * 5);
        cs.SetVector("renderResolution", Vector2.one * render.width);
        cs.SetBuffer(kernelID, "particles", particleBuffer);
        cs.SetTexture(kernelID, "Result", render);
        cs.Dispatch(kernelID, particleCount / 64, 1, 1);//256 / 8, 256 / 8, 1);

        ChangeAttractor(0);

        fadeKernelID = cs.FindKernel("RenderTextureFade");
        cs.SetTexture(fadeKernelID, "Result", render);

        cam = Camera.main;
    }

    void ChangeAttractor(int dir, bool forceResetBuffers = false)
    {
        selectedAttractorIdx = (selectedAttractorIdx + dir) % attractors.Length;
        if(selectedAttractorIdx < 0)
        {
            selectedAttractorIdx += attractors.Length;
        }

        attractorKernelID = cs.FindKernel(attractors[selectedAttractorIdx]);

        if (forceResetBuffers)
        {
            cs.Dispatch(cs.FindKernel("RenderTextureSetup"), render.width / 8, render.height / 8, 1);
            cs.Dispatch(cs.FindKernel("BufferSetup"), particleCount / 64, 1, 1);
        }

        cs.SetBuffer(attractorKernelID, "particles", particleBuffer);
        cs.SetTexture(attractorKernelID, "Result", render);
    }

    float xRot, yRot;

    // Update is called once per frame
    void Update()
    {
        var xRotationMatrix = Matrix4x4.identity;// * cam.projectionMatrix;
        xRotationMatrix[1, 1] = xRotationMatrix[2, 2] = Mathf.Cos(xRot);
        xRotationMatrix[1, 2] = Mathf.Sin(xRot);
        xRotationMatrix[2, 1] = -Mathf.Sin(xRot);

        var yRotationMatrix = Matrix4x4.identity;// * cam.projectionMatrix;
        yRotationMatrix[0, 0] = yRotationMatrix[2, 2] = Mathf.Cos(yRot);
        yRotationMatrix[0, 2] = -Mathf.Sin(yRot);
        yRotationMatrix[2, 0] = Mathf.Sin(yRot);

        var matrix =  xRotationMatrix  * yRotationMatrix;

        if (Input.GetKeyUp(KeyCode.LeftArrow)){
            ChangeAttractor(-1);
        }
        if(Input.GetKeyUp(KeyCode.RightArrow)){
            ChangeAttractor(1);
        }

        
        {
            //Input.getaxis randomly not working for me :)
            float x = (Input.GetKey(KeyCode.W) ? -1 : 0) + (Input.GetKey(KeyCode.S) ? 1 : 0);
            float y = (Input.GetKey(KeyCode.A)?-1 : 0) + (Input.GetKey(KeyCode.D) ? 1:0);
            
            xRot += x * Mathf.Deg2Rad * 60 * Time.deltaTime;
            yRot += y * Mathf.Deg2Rad * 60 * Time.deltaTime;
        }

        var mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        mousePos.x = Mathf.Clamp01(Input.mousePosition.x / cam.pixelWidth) * 2 - 1;
        mousePos.y = Mathf.Clamp01(Input.mousePosition.y / cam.pixelHeight) * 2 - 1;
        cs.SetMatrix("VPMatrix", matrix);

        cs.SetFloat("dt", Time.deltaTime);
        cs.SetVector("mousePos", mousePos);
        cs.Dispatch(attractorKernelID, particleCount / 64, 1, 1);//256 / 8, 256 / 8, 1);

        cs.Dispatch(fadeKernelID, render.width / 8, render.height / 8, 1);

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
