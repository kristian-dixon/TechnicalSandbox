using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplacementShader : MonoBehaviour
{
    
    Camera camera;
    public Shader shader;
    public string replacementTag = "RenderType";
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.SetReplacementShader(shader, replacementTag);
    }

   
}
