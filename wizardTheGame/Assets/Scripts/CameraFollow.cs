using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothFollow = 0f;

    Vector3 moveDirection = Vector3.zero;
    private Vector3 desiredPosition;
    private Vector3 currentposition;
    private Vector3 offset = new Vector3(0.5f, 1.5f, -2.5f);
    private Vector3 currentVelocity = Vector3.zero;

    private void Update()
    {
        currentposition = transform.position;
        desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(currentposition, desiredPosition, ref currentVelocity, smoothFollow);
       
    }

    private void RotateCamera()
    {
        //rotate camera according to player's movement direction

        //rotate around player
    }

}
