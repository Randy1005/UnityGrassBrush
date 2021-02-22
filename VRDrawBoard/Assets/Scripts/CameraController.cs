using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    float mouseSensitivity;

    float xAxisClamp = 0;

    [SerializeField]
    Transform playerBody, playerArm;
    void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        RotateCamera();
    }

    void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float rotAmountX = mouseX * mouseSensitivity;
        float rotAmountY = mouseY * mouseSensitivity;

        xAxisClamp -= rotAmountY;

        Vector3 rotPlayerArm = playerArm.transform.rotation.eulerAngles;
        Vector3 rotPlayerBody = playerBody.transform.rotation.eulerAngles;

        rotPlayerArm.x -= rotAmountY;
        rotPlayerArm.z = 0;
        rotPlayerBody.y += rotAmountX;

        if (xAxisClamp > 90)
        {
            xAxisClamp = 90;
            rotPlayerArm.x = 90;
        } 
        else if (xAxisClamp < -90)
        {
            xAxisClamp = -90;
            rotPlayerArm.x = 270;
        }

        playerArm.rotation = Quaternion.Euler(rotPlayerArm);
        playerBody.rotation = Quaternion.Euler(rotPlayerBody);
    }
}
