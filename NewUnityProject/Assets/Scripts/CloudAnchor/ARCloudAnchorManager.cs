using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class ARCloudAnchorManager : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    private float resolveAnchorPassedTimeout = 10.0f;

    private ARAnchorManager arAnchorManager = null;
    private ARAnchor pendingHostAnchor = null;
    private ARCloudAnchor cloudAnchor = null;
    private string anchorToResolve;
    private bool anchorUpdateInProgress = false;
    private bool anchorResolveInProgress = false;
    private float safeToResolvePassed = 0;
    private UnityEvent<Transform> resolver = null;

    private ARPlacementManager arPlacementManager = null;
    private ARDebugManager arDebugManager = null;

    private void Awake() 
    {
        arPlacementManager = GetComponent<ARPlacementManager>();
        arDebugManager = GetComponent<ARDebugManager>();

        resolver = new UnityEvent<Transform>();   
        resolver.AddListener((t) => arPlacementManager.ReCreatePlacement(t));
    }

    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position,
            arCamera.transform.rotation);
    }

    #region Anchor Cycle

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchor = arAnchor;
    }

    public void HostAnchor()
    {
        arDebugManager.LogInfo($"HostAnchor executing");

        FeatureMapQuality quality =
            arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());

        cloudAnchor = arAnchorManager.HostCloudAnchor(pendingHostAnchor, 1);
    
        if(cloudAnchor == null)
        {
            arDebugManager.LogError("Unable to host cloud anchor");
        }
        else
        {
            anchorUpdateInProgress = true;
        }
    }
    
    public void Resolve()
    {
        arDebugManager.LogInfo("Resolve executing");

        cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorToResolve);

        if(cloudAnchor == null)
        {
            arDebugManager.LogError($"Failed to resolve cloud achor id {cloudAnchor.cloudAnchorId}");
        }
        else
        {
            anchorResolveInProgress = true;
        }
    }

    private void CheckHostingProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        if(cloudAnchorState == CloudAnchorState.Success)
        {
            arDebugManager.LogError("Anchor successfully hosted");
            
            anchorUpdateInProgress = false;

            // keep track of cloud anchors added
            anchorToResolve = cloudAnchor.cloudAnchorId;
        }
        else if(cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            arDebugManager.LogError($"Fail to host anchor with state: {cloudAnchorState}");
            anchorUpdateInProgress = false;
        }
    }

    private void CheckResolveProgress()
    {
        CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;
        
        arDebugManager.LogInfo($"ResolveCloudAnchor state {cloudAnchorState}");

        if (cloudAnchorState == CloudAnchorState.Success)
        {
            arDebugManager.LogInfo($"CloudAnchorId: {cloudAnchor.cloudAnchorId} resolved");

            resolver.Invoke(cloudAnchor.transform);

            anchorResolveInProgress = false;
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            arDebugManager.LogError($"Fail to resolve Cloud Anchor with state: {cloudAnchorState}");

            anchorResolveInProgress = false;
        }
    }

#endregion

    void Update()
    {
        // check progress of new anchors created
        if(anchorUpdateInProgress)
        {
            CheckHostingProgress();
            return;
        }

        if(anchorResolveInProgress && safeToResolvePassed <= 0)
        {
            // check evey (resolveAnchorPassedTimeout)
            safeToResolvePassed = resolveAnchorPassedTimeout;

            if(!string.IsNullOrEmpty(anchorToResolve))
            {
                arDebugManager.LogInfo($"Resolving AnchorId: {anchorToResolve}");
                CheckResolveProgress();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
    }

}