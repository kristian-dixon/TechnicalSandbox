using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessHelper : MonoBehaviour
{
    public List<PPFXScriptableObjects> postProcessingModules;

    Camera cam;
    bool test = false;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        for(int i = 0; i < postProcessingModules.Count; i++)
        {
            postProcessingModules[i].Init();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(test)
        {
            Debug.Log("TEST IS TRUE");
        }
        test = true;

        if (!cam) return;
        RenderTexture rt = source;

        for (int i = 0; i < postProcessingModules.Count; i++)
        {
            rt = postProcessingModules[i].Dispatch(rt);
        }

        Graphics.Blit(rt, destination);
    }

    private void LateUpdate()
    {
        test = false;
    }
}
