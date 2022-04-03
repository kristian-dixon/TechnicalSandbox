using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseVP : MonoBehaviour
{
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;    
    }

    // Update is called once per frame
    void Update()
    {
        var proj = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        Shader.SetGlobalMatrix("_IP", proj.inverse);

        var prevVP = camera.worldToCameraMatrix ;
        Shader.SetGlobalMatrix("_IV", prevVP.inverse);

        var vpMat = Matrix4x4.Inverse(GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix);
        var vpMat2 = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false).inverse  * camera.worldToCameraMatrix.inverse;

        Shader.SetGlobalMatrix("_IVP", vpMat);
       // Debug.Log($"{vpMat.inverse} ::::: {test2}");

    }
}
