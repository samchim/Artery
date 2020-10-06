using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class moveByDrag : MonoBehaviour
{
    private Camera mainCamera;
    private float CameraZDistance;
    private Vector3 ScreenPosition;
    private Vector3 NewWorldPosition;
    private Rigidbody rb;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
        Debug.Log(rb.useGravity);
    }

    void Update()
    {
        // transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime, 0f, Input.GetAxis("Vertical") * Time.deltaTime);
        // CameraZDistance = mainCamera.WorldToScreenPoint(transform.position).z;

        if (Input.GetMouseButtonDown(0))
        {
            // Debug.Log(CameraZDistance);
        }
        else if (Input.GetMouseButton(0))
        {
            rb.useGravity = false;
            CameraZDistance = mainCamera.WorldToScreenPoint(transform.position).z;
            ScreenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, CameraZDistance); //z axis added to screen point 
            NewWorldPosition = mainCamera.ScreenToWorldPoint(ScreenPosition); //Screen point converted to world point
            transform.position = NewWorldPosition;

            Debug.Log(rb.useGravity);
            Debug.Log(CameraZDistance);
            // Debug.Log(NewWorldPosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            rb.useGravity = true;
        }

    }

    void OnMouseDrag()
    {

    }
}
