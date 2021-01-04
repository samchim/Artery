using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;
using System;

public class ARTapToCreateWithIndicator : MonoBehaviour
{
    public GameObject gameBase;
    public GameObject placementIndicator;
    // public GameObject placementIndicatorSrc;
    // public List<GameObject> cubes;
    // public GameObject smallIndicator;

    // private GameObject placementIndicator;
    private ARRaycastManager _arRayCastManager;
    private ARSessionOrigin _arOrigin;
    private ARPlaneManager _arPlaneManger;
    private GameObject[] _arPlanes;
    private Pose indicator;
    private bool indicatorIsValid = false;
    private int tabCounter = 0;
    private int listSize;
    private float halfHeight;
    private float halfWidth;

    static List<ARRaycastHit> planeHits = new List<ARRaycastHit>();
    static RaycastHit gameObjectHit = new RaycastHit();

    void Start()
    {
        _arOrigin = FindObjectOfType<ARSessionOrigin>();
        _arRayCastManager = GetComponent<ARRaycastManager>();
        _arPlaneManger = GetComponent<ARPlaneManager>();
        // listSize = cubes.Capacity;
        tabCounter = 0;
        
        halfHeight = Screen.height * 0.5f;
        halfWidth = Screen.width * 0.5f;
        Debug.Log("halfHeight: " + halfHeight.ToString());
        Debug.Log("halfWidth: " + halfWidth.ToString());

        // placementIndicator = Instantiate(placementIndicator, new Vector3(0,0,0), new Quaternion(0,0,0,1));
        placementIndicator.SetActive(false);
        // smallIndicator.SetActive(false);
    }

    void Update()
    {
        UpdateIndicatorPose();
        UpdateIndicator();

        if (indicatorIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    private void PlaceObject()
    {
        if (tabCounter == 0)
        {
            Debug.Log("Instantiate gameBase");
            Instantiate(gameBase, indicator.position, indicator.rotation);
            tabCounter += 1;    
            _arPlaneManger.enabled = false;
            _arPlanes = GameObject.FindGameObjectsWithTag("ARPlane");
            foreach (GameObject ARPlane in _arPlanes) {
                ARPlane.SetActive(false);
            }
        }
        // else
        // {
        //     Instantiate(cubes[0], new Vector3(indicator.position.x, indicator.position.y + 20, indicator.position.z), indicator.rotation);
        //     tabCounter += 1;
        // }
    }

    private void UpdateIndicator()
    {
        if (indicatorIsValid && tabCounter == 0)
        {
            Debug.Log("placementIndicator is shown");
            // smallIndicator.SetActive(false);
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(indicator.position, indicator.rotation);
        }
        // else if (indicatorIsValid && tabCounter >= 1)
        // {
        //     placementIndicator.SetActive(false);
        //     smallIndicator.SetActive(true);
        //     smallIndicator.transform.SetPositionAndRotation(indicator.position, indicator.rotation);
        // }
        else
        {
            placementIndicator.SetActive(false);
            // smallIndicator.SetActive(false);
        }
    }

    private void UpdateIndicatorPose()
    {
        var camera = Camera.current;
        var screenCenter = camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var Ray = camera.ScreenPointToRay(new Vector3(halfWidth, halfHeight));
        // var planeHits = new List<ARRaycastHit>();
        if (tabCounter < 1)
        {
            _arRayCastManager.Raycast(screenCenter, planeHits, TrackableType.Planes);

            indicatorIsValid = planeHits.Count > 0;
            if (indicatorIsValid)
            {
                indicator = planeHits[0].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                indicator.rotation = Quaternion.LookRotation(cameraBearing);
            }
        }
        // else
        // {
        //     if (Physics.Raycast(Ray, out gameObjectHit, 200f))
        //     {
        //         // Debug.DrawRay(Ray.po, Color.yellow);
        //         // if (gameObjectHit.transform.name.Contains("GameBase")){}
        //         indicator.position = gameObjectHit.point;
        //         var cameraForward = Camera.current.transform.forward;
        //         var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
        //         indicator.rotation = Quaternion.LookRotation(cameraBearing);
        //         // indicator.rotation = Quaternion.LookRotation(gameObjectHit.normal);
        //     }

        // }
    }
}
