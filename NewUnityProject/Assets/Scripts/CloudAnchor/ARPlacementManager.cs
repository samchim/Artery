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
    // private GameObject placedGameObject = null;
    private ARRaycastManager arRaycastManager = null;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private ARAnchorManager arAnchorManager = null;

    private GameObject placedGameObjectTmp = null;
    private List<GameObject> placedGameObjectList = new List<GameObject>();
    private int numOfPlaced = 0;

    private ARCloudAnchorManager arCloudAnchorManager = null;
    private ARDebugManager arDebugManager = null;

    void Awake() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();
        arCloudAnchorManager = GetComponent<ARCloudAnchorManager>();
        arDebugManager = GetComponent<ARDebugManager>();

        for (int i = 0; i < arCloudAnchorManager.NUM_OF_ANCHOR; i++)
        {
            placedGameObjectList.Add(new GameObject());
        }
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
        // Destroy(placedGameObject);
        // placedGameObject = null;
        arDebugManager.LogInfo($"RemovePlacements");
        for (int i = 0; i < arCloudAnchorManager.NUM_OF_ANCHOR; i++)
        {
            Destroy(placedGameObjectList[i]);
            arDebugManager.LogInfo($"RemovePlacements #{i}");
            placedGameObjectList[i] = new GameObject();
        }
        arCloudAnchorManager.numOfQueued = 0;
        numOfPlaced = 0;
    }

    void Update()
    {
        if(!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        // if(placedGameObject != null)
        //     return;
        
        if (numOfPlaced == arCloudAnchorManager.NUM_OF_ANCHOR)
            return;

        if(arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            placedGameObjectTmp = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
            placedGameObjectList[arCloudAnchorManager.numOfQueued] = placedGameObjectTmp;
            var anchor = arAnchorManager.AddAnchor(new Pose(hitPose.position, hitPose.rotation));
            placedGameObjectList[arCloudAnchorManager.numOfQueued].transform.parent = anchor.transform;

            // this won't host the anchor just add a reference to be later host it
            arCloudAnchorManager.QueueAnchor(anchor);
            numOfPlaced ++;
        }
    }

    public void ReCreatePlacement(Transform transform, int index)
    {
        placedGameObjectTmp = Instantiate(placedPrefab, transform.position, transform.rotation);
        placedGameObjectList[index] = placedGameObjectTmp;
        placedGameObjectList[index].transform.parent = transform;
        numOfPlaced ++;
    }
}