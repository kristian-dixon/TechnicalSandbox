using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Paintable : MonoBehaviour
{
    public Material paintMaterial;

    RenderTexture rt;
    Renderer renderer;
    CommandBuffer command;


    // Start is called before the first frame update
    void Start()
    {
        rt = new RenderTexture(256, 256,1);
        rt.enableRandomWrite = true;
        rt.Create();
        renderer = GetComponent<Renderer>();
        Shader.SetGlobalTexture("_PaintMaskTex", rt);
        command = new CommandBuffer();
        command.name = "Paint";
        command.SetRenderTarget(rt);
        command.DrawRenderer(renderer, paintMaterial, 0);
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButton(0))
        {
            paintMaterial.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
            Graphics.ExecuteCommandBuffer(command);
        }
        else if (Input.GetMouseButton(1))
        {
            paintMaterial.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
            Graphics.ExecuteCommandBuffer(command);
        }

    }
}
