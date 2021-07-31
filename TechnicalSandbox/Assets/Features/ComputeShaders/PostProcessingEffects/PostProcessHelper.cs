using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessHelper : MonoBehaviour
{
    public List<PPFXScriptableObjects> postProcessingModules;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < postProcessingModules.Count; i++)
        {
            postProcessingModules[i].Init();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture rt = source;

        for (int i = 0; i < postProcessingModules.Count; i++)
        {
            rt = postProcessingModules[i].Dispatch(rt);
        }

        Graphics.Blit(rt, destination);
    }
}
