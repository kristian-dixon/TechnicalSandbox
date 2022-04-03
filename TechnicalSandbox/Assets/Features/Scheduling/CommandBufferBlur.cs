using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferBlur : MonoBehaviour
{
    Camera camera;
    CommandBuffer commandBuffer;

    public Material ppfx;

    public void Start()
    {
        var r = GetComponent<Renderer>();

        camera = Camera.main;

        commandBuffer = new CommandBuffer();

        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        commandBuffer.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Bilinear);
        commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);//, ppfx);
        //commandBuffer.Blit(screenCopyID, BuiltinRenderTextureType.CurrentActive, ppfx);

        commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        commandBuffer.SetGlobalTexture("_MainTexed", screenCopyID);
        commandBuffer.DrawRenderer(r, ppfx);//, ppfx);


        camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
    }

    private void OnDestroy()
    {
        commandBuffer.Clear();
    }
}
