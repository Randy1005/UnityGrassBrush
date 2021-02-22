using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotion : MonoBehaviour
{
    float moveSpeed = 4;
    CharacterController controller;
    Vector3 moveDirection;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        if (controller.isGrounded)
        {
            moveDirection = new Vector3(moveX, 0, moveZ);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;
        }

        moveDirection.y -= 4.0f;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
