using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacementManager : MonoBehaviour
{ 
    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private GameObject placedPrefab = null;
    private GameObject placedGameObject = null;
    private ARRaycastManager arRaycastManager = null;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private ARAnchorManager arAnchorManager = null;

    private ARCloudAnchorManager arCloudAnchorManager = null;

    void Awake() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arCloudAnchorManager = GetComponent<ARCloudAnchorManager>();
    }

    bool IsPointOverUIObject(Vector2 pos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        
        // return true;
        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if(Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            Debug.Log("touch: " + touch.position.ToString() + ", " + touch.phase.ToString());
            if(touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;
                bool isOverUI = IsPointOverUIObject(touchPosition);
                Debug.Log("hit!!! " + touchPosition.ToString() + " ," + isOverUI.ToString());
                return isOverUI ? false : true;
            }
        }
        touchPosition = default;
        return false;
    }

    public void RemovePlacements()
    {
        Destroy(placedGameObject);
        placedGameObject = null;
    }

    void Update()
    {
        if(!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        if(placedGameObject != null)
            return;

        if(arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            placedGameObject = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
            var anchor = arAnchorManager.AddAnchor(new Pose(hitPose.position, hitPose.rotation));
            placedGameObject.transform.parent = anchor.transform;

            // this won't host the anchor just add a reference to be later host it
            arCloudAnchorManager.QueueAnchor(anchor);
        }
    }

    public void ReCreatePlacement(Transform transform)
    {
        placedGameObject = Instantiate(placedPrefab, transform.position, transform.rotation);
        placedGameObject.transform.parent = transform;
    }
}