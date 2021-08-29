using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow2DCameraBlit : MonoBehaviour
{
    public RenderTexture shadowMap;
    public Material shader;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, shader);
    }
}
