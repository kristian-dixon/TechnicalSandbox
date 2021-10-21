using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateBottleValues : MonoBehaviour
{
    public Transform connectedBody;
    Material mat;
    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetVector("_WaterDirection", Vector3.up * 5 + transform.position - connectedBody.position);
    }
}
