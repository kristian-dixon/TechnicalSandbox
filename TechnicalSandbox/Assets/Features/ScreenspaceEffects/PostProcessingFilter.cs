using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")]
public class PostProcessingFilter : MonoBehaviour
{
    [SerializeField]
    private Shader effectShader;

    private Material _EffectMaterial;
    private Camera currentCamera;

    public int index = 0;

    public Material EffectMaterial
    {
        get
        {
            if (!_EffectMaterial && effectShader)
            {
                _EffectMaterial = new Material(effectShader);
                _EffectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return _EffectMaterial;
        }
    }

    public Camera CurrentCamera
    {
        get
        {
            if (!currentCamera)
            {
                currentCamera = GetComponent<Camera>();
                currentCamera.depthTextureMode = DepthTextureMode.Depth;
            }
            return currentCamera;
        }
    }

   

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!EffectMaterial)
        {
            //Do nothing
            Graphics.Blit(source, destination);
            return;
        }
        
        //Note:: Could pass in view matrix instead
        EffectMaterial.SetVector("_CameraPos", CurrentCamera.transform.position);
        EffectMaterial.SetVector("_CameraFwd", CurrentCamera.transform.forward);
        EffectMaterial.SetVector("_CameraUp", CurrentCamera.transform.up);

        Graphics.Blit(source, destination, EffectMaterial);
    }

}
