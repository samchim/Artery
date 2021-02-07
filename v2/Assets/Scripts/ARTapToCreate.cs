using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARTapToCreate : MonoBehaviour
{
    public GameObject gameObjectToCreate;

    private GameObject spwanedObject;
    private ARRaycastManager _arRayCastManager;
    private Vector2 touchPosition;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();


    private void Awake()
    {
        _arRayCastManager = GetComponent<ARRaycastManager>();
    }

    bool TryGetTouchPostion(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!TryGetTouchPostion(out Vector2 touchPosition))
            return;

        if (_arRayCastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            if (spwanedObject == null)
            {
                spwanedObject = Instantiate(gameObjectToCreate, hitPose.position, hitPose.rotation);
            }
            else
            {
                spwanedObject.transform.position = hitPose.position;
            }
        }
    }
}
