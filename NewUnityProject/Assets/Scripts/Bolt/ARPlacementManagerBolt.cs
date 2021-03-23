using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacementManagerBolt : MonoBehaviour
{ 
    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private GameObject placedPrefab = null;
    // private GameObject placedGameObject = null;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private GameObject placedGameObjectTmp = null;
    private List<GameObject> placedGameObjectList = new List<GameObject>();
    private int numOfPlaced = 0;

    private ARRaycastManager _arRaycastManager = null;
    private ARAnchorManager _arAnchorManager = null;
    private ARCloudAnchorManagerBolt _arCloudAnchorManagerBolt = null;
    private ARDebugManager _arDebugManager = null;

    void Awake() 
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
        _arAnchorManager = GetComponent<ARAnchorManager>();
        _arCloudAnchorManagerBolt = GetComponent<ARCloudAnchorManagerBolt>();
        _arDebugManager = GetComponent<ARDebugManager>();

        for (int i = 0; i < _arCloudAnchorManagerBolt.NUM_OF_ANCHOR; i++)
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
            // _arDebugManager.LogInfo($"touch: {touch.position.ToString()}, {touch.phase.ToString()}");
            Debug.Log($"touch: {touch.position.ToString()}, {touch.phase.ToString()}");
            if(touch.phase == TouchPhase.Began)
            {
                touchPosition = touch.position;
                bool isOverUI = IsPointOverUIObject(touchPosition);
                // _arDebugManager.LogInfo($"hit");
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
        _arDebugManager.LogInfo($"RemovePlacements");
        for (int i = 0; i < _arCloudAnchorManagerBolt.NUM_OF_ANCHOR; i++)
        {
            Destroy(placedGameObjectList[i]);
            _arDebugManager.LogInfo($"RemovePlacements #{i}");
            placedGameObjectList[i] = new GameObject();
        }
        _arCloudAnchorManagerBolt.numOfQueued = 0;
        numOfPlaced = 0;
    }

    void Update()
    {
        if(!TryGetTouchPosition(out Vector2 touchPosition))
            return;

        // if(placedGameObject != null)
        //     return;
        
        if (numOfPlaced == _arCloudAnchorManagerBolt.NUM_OF_ANCHOR)
            return;

        if(_arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;
            placedGameObjectTmp = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
            placedGameObjectList[_arCloudAnchorManagerBolt.numOfQueued] = placedGameObjectTmp;
            var anchor = _arAnchorManager.AddAnchor(new Pose(hitPose.position, hitPose.rotation));
            placedGameObjectList[_arCloudAnchorManagerBolt.numOfQueued].transform.parent = anchor.transform;

            // this won't host the anchor just add a reference to be later host it
            _arCloudAnchorManagerBolt.QueueAnchor(anchor);
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