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
        Shader.SetGlobalMatrix("_IP", camera.projectionMatrix.inverse);

        var prevVP = camera.worldToCameraMatrix.inverse; ;
        Shader.SetGlobalMatrix("_IV", prevVP.inverse);
    }
}
