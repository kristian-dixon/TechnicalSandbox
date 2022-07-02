using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour
{
    // Start is called before the first frame update
    public float sensitivityHorizontal = 100, sensitivityVertical = 100;
    public float movSpeed = 5f;

    bool firstFrame = true;

    float xRotation, yRotation;
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Cursor.lockState == CursorLockMode.Locked && !Cursor.visible)
        {
            if (firstFrame) { firstFrame = false; return; }
            if (Time.deltaTime > 1) return;

            float up = Input.GetKey(KeyCode.Q) ? -1 : Input.GetKey(KeyCode.E) ? 1 : 0;

            var movDir = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical") + transform.up * up;

            movDir = movDir.normalized;
            transform.position += movDir * Time.deltaTime * movSpeed;

            if (Time.timeSinceLevelLoad > 3)
            {
                xRotation += Input.GetAxis("Mouse X") * sensitivityHorizontal * Time.deltaTime;
                yRotation += Input.GetAxis("Mouse Y") * sensitivityVertical * Time.deltaTime;
                transform.rotation = Quaternion.Euler(yRotation, xRotation, 0);
            }
        }
       

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (!Cursor.visible)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false ;
                Cursor.lockState = CursorLockMode.Locked;
            }
            
        }
    }
}
