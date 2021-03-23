using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacementManagerWithIndicator : MonoBehaviour
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

    [SerializeField]
    private GameObject placementIndicator = new GameObject();

    private GameObject[] arPlanes;
    private Pose indicator;
    private bool indicatorIsValid = false;
    private float halfHeight;
    private float halfWidth;
    private List<ARRaycastHit> planeHits = new List<ARRaycastHit>();
    private RaycastHit gameObjectHit = new RaycastHit();

    private ARRaycastManager _arRaycastManager = null;
    private ARAnchorManager _arAnchorManager = null;
    private ARPlaneManager _arPlaneManger = null;
    private ARCloudAnchorManager _arCloudAnchorManager = null;
    private ARDebugManager _arDebugManager = null;

    void Awake() 
    {
        _arRaycastManager = GetComponent<ARRaycastManager>();
        _arAnchorManager = GetComponent<ARAnchorManager>();
        _arPlaneManger = GetComponent<ARPlaneManager>();
        _arCloudAnchorManager = GetComponent<ARCloudAnchorManager>();
        _arDebugManager = GetComponent<ARDebugManager>();

        for (int i = 0; i < _arCloudAnchorManager.NUM_OF_ANCHOR; i++)
        {
            placedGameObjectList.Add(new GameObject());
        }

        halfHeight = Screen.height * 0.5f;
        halfWidth = Screen.width * 0.5f;
        placementIndicator.SetActive(false);
    }

    // bool IsPointOverUIObject(Vector2 pos)
    // {
    //     if (EventSystem.current.IsPointerOverGameObject())
    //     {
    //         return false;
    //     }
        
    //     // return true;
    //     PointerEventData eventPosition = new PointerEventData(EventSystem.current);
    //     eventPosition.position = new Vector2(pos.x, pos.y);

    //     List<RaycastResult> results = new List<RaycastResult>();
    //     EventSystem.current.RaycastAll(eventPosition, results);

    //     return results.Count > 0;
    // }

    // bool TryGetTouchPosition(out Vector2 touchPosition)
    // {
    //     if(Input.touchCount > 0)
    //     {
    //         var touch = Input.GetTouch(0);
    //         _arDebugManager.LogInfo($"touch: {touch.position.ToString()}, {touch.phase.ToString()}");
    //         Debug.Log($"touch: {touch.position.ToString()}, {touch.phase.ToString()}");
    //         if(touch.phase == TouchPhase.Began)
    //         {
    //             touchPosition = touch.position;
    //             bool isOverUI = IsPointOverUIObject(touchPosition);
    //             _arDebugManager.LogInfo($"hit");
    //             Debug.Log("hit!!! " + touchPosition.ToString() + " ," + isOverUI.ToString());
    //             return isOverUI ? false : true;
    //         }
    //     }
    //     touchPosition = default;
    //     return false;
    // }

    public void RemovePlacements()
    {
        // Destroy(placedGameObject);
        // placedGameObject = null;
        _arDebugManager.LogInfo($"RemovePlacements");
        for (int i = 0; i < _arCloudAnchorManager.NUM_OF_ANCHOR; i++)
        {
            Destroy(placedGameObjectList[i]);
            _arDebugManager.LogInfo($"RemovePlacements #{i}");
            placedGameObjectList[i] = new GameObject();
        }
        _arCloudAnchorManager.numOfQueued = 0;
        numOfPlaced = 0;
    }

    void Update()
    {
        // if(!TryGetTouchPosition(out Vector2 touchPosition))
        //     return;

        // if(placedGameObject != null)
        //     return;

        
        if (numOfPlaced == _arCloudAnchorManager.NUM_OF_ANCHOR)
            return;


        // if(_arRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        // {
        //     var hitPose = hits[0].pose;
        //     placedGameObjectTmp = Instantiate(placedPrefab, hitPose.position, hitPose.rotation);
        //     placedGameObjectList[_arCloudAnchorManager.numOfQueued] = placedGameObjectTmp;
        //     var anchor = _arAnchorManager.AddAnchor(new Pose(hitPose.position, hitPose.rotation));
        //     placedGameObjectList[_arCloudAnchorManager.numOfQueued].transform.parent = anchor.transform;

        //     // this won't host the anchor just add a reference to be later host it
        //     _arCloudAnchorManager.QueueAnchor(anchor);
        //     numOfPlaced ++;
        // }

        UpdateIndicatorPose();
        UpdateIndicator();
    }

    public void ReCreatePlacement(Transform transform, int index)
    {
        placedGameObjectTmp = Instantiate(placedPrefab, transform.position, transform.rotation);
        placedGameObjectList[index] = placedGameObjectTmp;
        placedGameObjectList[index].transform.parent = transform;
        numOfPlaced ++;
    }

    private void PlaceAnchor()
    {
        
    }
    
    private void UpdateIndicator()
    {
        if (indicatorIsValid)
        {
            Debug.Log("placementIndicator is shown");
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
        if (numOfPlaced < _arCloudAnchorManager.NUM_OF_ANCHOR)
        {
            // _arRayCastManager.Raycast(screenCenter, planeHits, TrackableType.Planes);

            indicatorIsValid = planeHits.Count > 0;
            if (indicatorIsValid)
            {
                indicator = planeHits[0].pose;

                var cameraForward = Camera.current.transform.forward;
                var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
                indicator.rotation = Quaternion.LookRotation(cameraBearing);
            }
        }
    }
}