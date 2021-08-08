using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextureMap
{
    public string textureKey;
    public Texture texture;
}



[CreateAssetMenu(fileName = "Data", menuName = "PostProcessing/PostProcessingBaseEffect", order = 1)]
public class PPFXScriptableObjects : ScriptableObject
{
    public ComputeShader cs;
    public string mainProgramName = "CSMain";

    public List<TextureMap> textures;

    public bool forceTextureUpdate = false;

    RenderTexture rt = null;
    int kernel;

    public void Init()
    {
        kernel = cs.FindKernel(mainProgramName);

        foreach (var texture in textures)
        {
            cs.SetTexture(kernel, texture.textureKey, texture.texture);
        }
    }

    public RenderTexture Dispatch(RenderTexture source)
    {
        if(forceTextureUpdate)
        {
            foreach (var texture in textures)
            {
                cs.SetTexture(kernel, texture.textureKey, texture.texture);
            }
        }

        cs.SetFloat("elapsedTime", Time.time);

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
