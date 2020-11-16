using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;
using System;

public class ARTapToCreateWithIndicator : MonoBehaviour
{
    public GameObject objectToPlace;
    public GameObject placementIndicator;

    private ARRaycastManager _arRayCastManager;
    private ARSessionOrigin _arOrigin;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private GameObject spwanedObject;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        _arOrigin = FindObjectOfType<ARSessionOrigin>();
        _arRayCastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (spwanedObject == null)
        {
            spwanedObject = Instantiate(objectToPlace, placementPose.position, placementPose.rotation);
        }
        else{
            spwanedObject.transform.position = placementPose.position;
            spwanedObject.transform.rotation = placementPose.rotation;
        }
    }

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        // var hits = new List<ARRaycastHit>();
        _arRayCastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}
