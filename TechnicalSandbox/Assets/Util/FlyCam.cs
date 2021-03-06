﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour
{
    // Start is called before the first frame update
    public float sensitivityHorizontal = 1, sensitivityVertical = 1;
    public float movSpeed = 5f;

    float xRotation, yRotation;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float up = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0;

        var movDir = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical") + transform.up * up;

        movDir = movDir.normalized;
        transform.position += movDir * Time.deltaTime * movSpeed;

        xRotation += Input.GetAxis("Mouse X") * sensitivityHorizontal * Time.deltaTime;
        yRotation += Input.GetAxis("Mouse Y") * sensitivityVertical * Time.deltaTime;
        transform.rotation = Quaternion.Euler(yRotation, xRotation, 0);

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
