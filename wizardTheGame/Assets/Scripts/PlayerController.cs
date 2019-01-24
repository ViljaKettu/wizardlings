using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float turnSpeed = 2f;
    public float jumpSpeed;
    public float smoothRotate;

    Vector3 moveDirection = Vector3.zero;
    Vector3 rotationDirection = Vector3.zero;

    private float gravity = 9.81f;

    private void Update()
    {
        //CheckGround();
        Move();
        //Jump();
    }

    private void Move()
    {      

        Vector3 inputvectorX = (Vector3.up * Input.GetAxisRaw("Horizontal") * turnSpeed);
        Vector3 inputvectorY = (Input.GetAxisRaw("Vertical") * Vector3.forward * speed) * Time.deltaTime;
        Vector3 inputvectorZ = (Input.GetAxisRaw("Jump") * Vector3.up * speed) * Time.deltaTime;


        moveDirection = inputvectorY + inputvectorZ;
        rotationDirection = inputvectorX;

        //if(Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        //{
        //    moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //    moveDirection = transform.TransformDirection(moveDirection);
        //}

        transform.Translate(moveDirection);
        transform.Rotate(rotationDirection);

    }

    private void Jump()
    {

    }

    private void CastSpell(int chosenSpell)
    {
        //check list for chosen spell

        //cast spell
    }

    private void CheckGround()
    {
       
    }

    private void CheckCollision()
    {

    }
}
