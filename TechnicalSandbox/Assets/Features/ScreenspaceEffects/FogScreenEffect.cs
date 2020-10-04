using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")]
public class FogScreenEffect : SceneViewFilter
{
    [SerializeField]
    private Shader effectShader;

    private Material _EffectMaterial;
    private Camera currentCamera;

    public Transform sunLight;
    public int worldRaidus = 100;

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

    ///Stores normalized rays representing camera frustum in a 4x4 matrix. Each row is a vector (optimisation for when passing over to shader)/// 
    /// The following rays are stored in each row (in viewspace, not worldspace):
    /// Top Left corner:     row=0
    /// Top Right corner:    row=1
    /// Bottom Right corner: row=2
    /// Bottom Left corner:  row=3
    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovHalf = camFov * 0.5f;
        float tanFov = Mathf.Tan(fovHalf * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tanFov * camAspect;
        Vector3 toTop = Vector3.up * tanFov;

        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 botRight = (-Vector3.forward + toRight - toTop);
        Vector3 botLeft = (-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, botRight);
        frustumCorners.SetRow(3, botLeft);

        return frustumCorners;
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

        //Pass frustum rays to the shader
        EffectMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera));
        EffectMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
        EffectMaterial.SetVector("_CameraWS", CurrentCamera.transform.position);
        EffectMaterial.SetVector("_LightDir", sunLight ? sunLight.forward : Vector3.down);
        EffectMaterial.SetFloat("_WorldRadius", worldRaidus);

        EffectMaterial.SetInt("_ScreenWidth", currentCamera.pixelWidth);
        EffectMaterial.SetInt("_ScreenHeight", currentCamera.pixelHeight);

        EffectMaterial.SetMatrix("xKernel", new Matrix4x4(new Vector4(-1, -2, -1, 0), new Vector4(0, 0, 0, 0), new Vector4(1, 2, 1, 0), Vector4.zero));
        EffectMaterial.SetMatrix("yKernel", new Matrix4x4(new Vector4(-1, 0, 1, 0), new Vector4(-2, 0, 2, 0), new Vector4(-1, 0, 1, 0), Vector4.zero));


        CustomGraphicsBlit(source, destination, EffectMaterial, index);
    }

    /// \brief Custom version of Graphics.Blit that encodes frustum corner indices into the input vertices.
    /// 
    /// In a shader you can expect the following frustum cornder index information to get passed to the z coordinate:
    /// Top Left vertex:     z=0, u=0, v=0
    /// Top Right vertex:    z=1, u=1, v=0
    /// Bottom Right vertex: z=2, u=1, v=1
    /// Bottom Left vertex:  z=3, u=1, v=0
    /// 
    /// \warning You may need to account for flipped UVs on DirectX machines due to differing UV semantics
    ///          between OpenGL and DirectX.  Use the shader define UNITY_UV_STARTS_AT_TOP to account for this.
    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;
        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho(); //Z values don't matter due to orthographic projection.

        fxMaterial.SetPass(passNr);
        GL.Begin(GL.QUADS);

        //Here, GL.MultitexCoord2(0,x,y) assigns the value (x,y) to the TEXCOORD0 slot in the shader.
        //GL.Vertex3(x,y,z)queues up a vertex at position (x,y,z) to be drawn. Note we're storing our own custom frustum info in the Z coord.
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0, 0, 3); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1, 0, 2); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1, 1, 1); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0, 1, 0); // TL

        GL.End();
        GL.PopMatrix();
    }
}
