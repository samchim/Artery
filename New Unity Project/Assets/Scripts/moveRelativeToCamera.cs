using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class moveRelativeToCamera : MonoBehaviour
{
    private Camera mainCamera;
    private float CameraZDistance;
    private Vector3 ScreenPosition;
    private Vector3 NewWorldPosition;
    private Rigidbody rb;

    void Start()
    {
        mainCamera = Camera.main;
        CameraZDistance =
            mainCamera.WorldToScreenPoint(transform.position).z; //z axis of the game object for screen view
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime, 0f, Input.GetAxis("Vertical") * Time.deltaTime);
        CameraZDistance = mainCamera.WorldToScreenPoint(transform.position).z;
    }


}
