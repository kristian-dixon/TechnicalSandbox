using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexTest : MonoBehaviour
{
    Texture2D tex;

    // Start is called before the first frame update
    void Start()
    {
        tex = new Texture2D(8, 8, TextureFormat.RGBAFloat, false);

        tex.SetPixel(0, 0, new Color(3, -40, 20));
        tex.Apply();

        Debug.LogError(tex.GetPixel(0, 0));
        Debug.LogError(new Color(3,5,0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
