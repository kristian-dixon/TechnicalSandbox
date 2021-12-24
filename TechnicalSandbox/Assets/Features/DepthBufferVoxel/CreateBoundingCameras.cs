using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateBoundingCameras : MonoBehaviour
{
    public Shader depthShader;
    RenderTexture rt;


    public RawImage test;
    Camera cam;

    Bounds renderBounds;

    // Start is called before the first frame update
    void Start()
    {
        Texture2D tex = new Texture2D(32, 32);

        var renderers = GetComponentsInChildren<MeshRenderer>();
        Bounds b = new Bounds();


        if(renderers.Length > 0)
        {
            b = renderers[0].bounds;
        }

        foreach (var r in renderers)
        {
            b.Encapsulate(r.bounds);
        }

        renderBounds = b;
        GameObject go = new GameObject("DepthCamZ");
        go.transform.parent = transform;

        cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(b.extents.x, b.extents.y);
        cam.nearClipPlane = 0;
        cam.farClipPlane = b.size.z;

        cam.targetDisplay = 1;
        cam.transform.localPosition = b.center + Vector3.back * b.extents.z;
        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.Color;

        rt = new RenderTexture(32, 32, 1);
        rt.enableRandomWrite = true;
        //rt.antiAliasing = 0;
        rt.filterMode = FilterMode.Point;

        cam.targetTexture = rt;
        cam.SetReplacementShader(depthShader, "");
        cam.Render();
        //cam.cullingMask = LayerMask.NameToLayer("Default");
        
    }

    private void LateUpdate()
    {
        if (cam.enabled)
        {
            cam.enabled = false;

            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGB24, false);
            tex.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            test.texture = tex;
            Voxelise(tex);
        }
    }


    List<Bounds> bounds = new List<Bounds>();
    void Voxelise(Texture2D tex)
    {
        var pixels = tex.GetPixels();

        Vector3 dim = renderBounds.size / 32f; 

        for(int y = 0; y < 32; y++)
        {
            for(int x = 0; x < 32; x++)
            {
                float val = pixels[x + y * 32].r;
                if (val == 0) continue;

                bounds.Add(new Bounds(renderBounds.min + new Vector3(x * dim.x, y * dim.y, -renderBounds.size.z * val), dim));
            }
        }
    }

    private void OnDrawGizmos()
    {
        foreach(var b in bounds)
        {
            Gizmos.DrawCube(b.center, b.size);
        }
    }

    private void OnDestroy()
    {
        rt.Release();
    }
}
