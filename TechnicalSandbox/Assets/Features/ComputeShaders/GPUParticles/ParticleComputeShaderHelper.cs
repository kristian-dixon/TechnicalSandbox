using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleComputeShaderHelper : MonoBehaviour
{
    public ComputeShader cs;
    public Material renderMaterial;


    ComputeBuffer particleBuffer;
    ComputeBuffer indirectArgs;


    public int particleCount = 512;

    public Color clearColour = Color.black;


    int selectedAttractorIdx = 0;
    string[] attractors = new[] { "ThreeCellCNNAttractor", "HalvorsenAttractor", "NoseHooverAttractor", "RandomMovement" };

    int attractorKernelID = -1;
    int fadeKernelID = -1;
    int bloomKernelID = -1;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        particleBuffer = new ComputeBuffer(particleCount, (sizeof(float)) * 6);
        indirectArgs = new ComputeBuffer(1, sizeof(uint) * 4, ComputeBufferType.IndirectArguments);
        indirectArgs.SetData(new int[] { particleCount, 1, 0, 0 });

        cs.SetInt("_ParticleCount", particleCount);

        var kernelID = cs.FindKernel("BufferSetup");
        cs.SetBuffer(kernelID, "particles", particleBuffer);
        //Setup the particle buffer in a shader since it'll be faster.

        cs.Dispatch(kernelID, Mathf.CeilToInt(particleCount / 64f), 1, 1);//256 / 8, 256 / 8, 1);

        renderMaterial.SetBuffer("_ParticleBuffer", particleBuffer);

        ChangeAttractor(0);
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
            cs.Dispatch(cs.FindKernel("BufferSetup"), Mathf.CeilToInt(particleCount / 64f), 1, 1);
        }

        cs.SetBuffer(attractorKernelID, "particles", particleBuffer);
    }

    float xRot, yRot;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) Application.Quit();

        //var xRotationMatrix = Matrix4x4.identity;// * cam.projectionMatrix;
        //xRotationMatrix[1, 1] = xRotationMatrix[2, 2] = Mathf.Cos(xRot);
        //xRotationMatrix[1, 2] = Mathf.Sin(xRot);
        //xRotationMatrix[2, 1] = -Mathf.Sin(xRot);

        //var yRotationMatrix = Matrix4x4.identity;// * cam.projectionMatrix;
        //yRotationMatrix[0, 0] = yRotationMatrix[2, 2] = Mathf.Cos(yRot);
        //yRotationMatrix[0, 2] = -Mathf.Sin(yRot);
        //yRotationMatrix[2, 0] = Mathf.Sin(yRot);

        //var matrix =  xRotationMatrix  * yRotationMatrix;

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

   

        cs.SetFloat("dt", Time.deltaTime);
        cs.SetVector("mousePos", transform.position);
        cs.Dispatch(attractorKernelID, Mathf.CeilToInt(particleCount / 64f), 1, 1);//256 / 8, 256 / 8, 1);

        Graphics.DrawProceduralIndirect(renderMaterial, new Bounds(Vector3.zero, Vector3.one * 100), MeshTopology.Points, indirectArgs, 0, null, null, UnityEngine.Rendering.ShadowCastingMode.On, true, gameObject.layer);

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
