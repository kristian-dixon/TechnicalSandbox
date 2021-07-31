using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "PostProcessing/PostProcessingBaseEffect", order = 1)]
public class PPFXScriptableObjects : ScriptableObject
{
    public ComputeShader cs;
    public string mainProgramName = "CSMain";
    RenderTexture rt = null;
    int kernel;

    public void Init()
    {
        kernel = cs.FindKernel(mainProgramName);

    }

    public RenderTexture Dispatch(RenderTexture source)
    {
        if (rt == null)
        {
            rt = new RenderTexture(source.width, source.height, 1);
            rt.enableRandomWrite = true;
            rt.Create();
            cs.SetTexture(kernel, "dstTexture", rt);
        }

        cs.SetTexture(kernel, "srcTexture", source);
        cs.Dispatch(kernel, rt.width / 8, rt.height / 8, 1);

        return rt;
    }
}
