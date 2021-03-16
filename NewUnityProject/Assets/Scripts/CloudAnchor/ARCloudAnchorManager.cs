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

    [SerializeField]
    public GameObject worldOrigin;

    [SerializeField]
    public int NUM_OF_ANCHOR = 3;

    private ARAnchorManager arAnchorManager = null;
    // private ARAnchor pendingHostAnchor = null;
    // private ARCloudAnchor cloudAnchor = null;
    // private string anchorToResolve;
    // private bool anchorUpdateInProgress = false;
    // private bool anchorResolveInProgress = false;
    private float safeToResolvePassed = 0;
    private UnityEvent<Transform, int> resolver = null;

    private List<ARAnchor> pendingHostAnchorList = new List<ARAnchor>();
    private int anchorUpdateInProgress = 0;
    private List<string> anchorToResolveList = new List<string>();
    private int anchorResolveInProgress = 0;
    public int numOfQueued = 0;
    // public int numOfHosted = 0;
    public int numOfResolved = 0;
    private List<ARCloudAnchor> cloudAnchorList = new List<ARCloudAnchor>();

    private ARPlacementManager arPlacementManager = null;
    private ARDebugManager arDebugManager = null;

    private void Awake()
    {
        arPlacementManager = GetComponent<ARPlacementManager>();
        arDebugManager = GetComponent<ARDebugManager>();

        resolver = new UnityEvent<Transform, int>();
        resolver.AddListener((t, i) => arPlacementManager.ReCreatePlacement(t, i));

        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            pendingHostAnchorList.Add(null);
            anchorToResolveList.Add("");
            cloudAnchorList.Add(null);
        }
    }

    private Pose GetCameraPose()
    {
        return new Pose(arCamera.transform.position,
            arCamera.transform.rotation);
    }

    #region Anchor Cycle

    public void QueueAnchor(ARAnchor arAnchor)
    {
        pendingHostAnchorList[numOfQueued] = arAnchor;
        numOfQueued++;
    }

    public void startHostAnchor()
    {
        anchorUpdateInProgress = 1;
        for (int i = 0; i < numOfQueued; i++)
        {
            HostAnchor();
            anchorUpdateInProgress++;
        }
        anchorUpdateInProgress = 1;
    }

    public void HostAnchor()
    {
        arDebugManager.LogInfo($"HostAnchor executing #" + anchorUpdateInProgress.ToString());

        if (anchorUpdateInProgress == 1)
        {
            FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        }

        cloudAnchorList[anchorUpdateInProgress - 1] = arAnchorManager.HostCloudAnchor(pendingHostAnchorList[anchorUpdateInProgress - 1], 1);
        if (cloudAnchorList[anchorUpdateInProgress - 1] == null)
        {
            arDebugManager.LogError($"Unable to host cloud anchor #{anchorUpdateInProgress}");
            numOfQueued--;
        }
    }

    private int NumOfHosted()
    {
        int n = 0;
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (cloudAnchorList[i] != null 
            && (cloudAnchorList[i].cloudAnchorState == CloudAnchorState.Success || cloudAnchorList[i].cloudAnchorState != CloudAnchorState.TaskInProgress)){
                n += 1;
            }
        }
        return n;
    }

    private void CheckHostingProgress()
    {

        CloudAnchorState cloudAnchorState = cloudAnchorList[anchorUpdateInProgress - 1].cloudAnchorState;
        if (cloudAnchorState == CloudAnchorState.Success)
        {
            arDebugManager.LogError($"Anchor #{anchorUpdateInProgress.ToString()} successfully hosted");

            // keep track of cloud anchors added
            anchorToResolveList[anchorUpdateInProgress - 1] = cloudAnchorList[anchorUpdateInProgress - 1].cloudAnchorId;
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            arDebugManager.LogError($"Fail to host anchor #{anchorUpdateInProgress.ToString()} with state: {cloudAnchorState}");
        }
        else
        {
            arDebugManager.LogInfo($"CheckHostingProgress #{(anchorUpdateInProgress).ToString()}");
            arDebugManager.LogInfo($"#{(anchorUpdateInProgress).ToString()}: NumOfHosted() = {NumOfHosted().ToString()}");
        }

        if (anchorUpdateInProgress < (NUM_OF_ANCHOR))
        {
            anchorUpdateInProgress++;
        }
        else
            anchorUpdateInProgress = 1;

        if (NumOfHosted() >= numOfQueued)
            anchorUpdateInProgress = 0;
    }

    public void StartResolve()
    {
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            cloudAnchorList[i] = new ARCloudAnchor();
        }
        anchorResolveInProgress = 1;
        Resolve();
    }

    public void Resolve()
    {
        arDebugManager.LogInfo("Resolve executing");

        cloudAnchorList[anchorResolveInProgress - 1] = arAnchorManager.ResolveCloudAnchorId(anchorToResolveList[anchorResolveInProgress - 1]);

        if (cloudAnchorList[anchorResolveInProgress - 1] == null)
        {
            arDebugManager.LogError($"Failed to resolve cloud achor #{anchorResolveInProgress} id {cloudAnchorList[anchorResolveInProgress].cloudAnchorId}");

            if (anchorResolveInProgress < NUM_OF_ANCHOR)
            {
                anchorResolveInProgress++;
                Resolve();
            }
            else
                anchorResolveInProgress = 0;
        }

        // ARDebugManager.Instance.LogInfo("Resolve executing");

        // cloudAnchor = arAnchorManager.ResolveCloudAnchorId(anchorToResolve);

        // if(cloudAnchor == null)
        // {
        //     ARDebugManager.Instance.LogError($"Failed to resolve cloud achor id {cloudAnchor.cloudAnchorId}");
        // }
        // else
        // {
        //     anchorResolveInProgress = true;
        // }
    }

    private void CheckResolveProgress()
    {

        CloudAnchorState cloudAnchorState = cloudAnchorList[anchorResolveInProgress - 1].cloudAnchorState;

        arDebugManager.LogInfo($"ResolveCloudAnchor #{anchorResolveInProgress} state {cloudAnchorState}");

        if (cloudAnchorState == CloudAnchorState.Success)
        {
            arDebugManager.LogInfo($"CloudAnchorId: {cloudAnchorList[anchorResolveInProgress - 1].cloudAnchorId} resolved");

            resolver.Invoke(cloudAnchorList[anchorResolveInProgress - 1].transform, anchorResolveInProgress);
            // arPlacementManager.ReCreatePlacement(cloudAnchor.transform, i);
        }
        else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        {
            arDebugManager.LogError($"Fail to resolve Cloud Anchor with state: {cloudAnchorState}");
        }
        else
        {
            arDebugManager.LogInfo($"CheckResolveProgress #{(anchorResolveInProgress).ToString()}");
        }

        if (anchorResolveInProgress < NUM_OF_ANCHOR)
        {
            anchorResolveInProgress++;
            Resolve();
            numOfResolved++;
        }
        else
        {
            anchorResolveInProgress = 1;
        }


        // CloudAnchorState cloudAnchorState = cloudAnchor.cloudAnchorState;

        // arDebugManager.LogInfo($"ResolveCloudAnchor state {cloudAnchorState}");

        // if (cloudAnchorState == CloudAnchorState.Success)
        // {
        //     arDebugManager.LogInfo($"CloudAnchorId: {cloudAnchor.cloudAnchorId} resolved");

        //     resolver.Invoke(cloudAnchor.transform);
        //     // arPlacementManager.ReCreatePlacement(cloudAnchor.transform, i);

        //     anchorResolveInProgress = false;
        // }
        // else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
        // {
        //     arDebugManager.LogError($"Fail to resolve Cloud Anchor with state: {cloudAnchorState}");

        //     anchorResolveInProgress = false;
        // }
    }


    private float px, py, pz, qx, qy, qz, qw = 0;
    private void AverageWorldOrigin()
    {
        px = 0;
        py = 0;
        pz = 0;
        qx = 0;
        qy = 0;
        qz = 0;
        qw = 0;
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (!cloudAnchorList[i].Equals(new ARCloudAnchor()))
            {
                px += cloudAnchorList[i].transform.position.x;
                py += cloudAnchorList[i].transform.position.y;
                pz += cloudAnchorList[i].transform.position.z;
                qx += cloudAnchorList[i].transform.rotation.x;
                qy += cloudAnchorList[i].transform.rotation.y;
                qz += cloudAnchorList[i].transform.rotation.z;
                qw += cloudAnchorList[i].transform.rotation.w;
            }
        }
        worldOrigin.transform.position = new Vector3(px / numOfResolved, py / numOfResolved, pz / numOfResolved);
        worldOrigin.transform.rotation = new Quaternion(qx / numOfResolved, qy / numOfResolved, qz / numOfResolved, qw / numOfResolved);
    }

    #endregion

    void Update()
    {
        // check progress of new anchors created
        if (0 < anchorUpdateInProgress && anchorUpdateInProgress <= NUM_OF_ANCHOR)
        {
            CheckHostingProgress();
            return;
        }

        if (safeToResolvePassed <= 0)
        {
            // check evey (resolveAnchorPassedTimeout)
            safeToResolvePassed = resolveAnchorPassedTimeout;

            if (0 < anchorResolveInProgress && anchorResolveInProgress <= NUM_OF_ANCHOR)
            {
                if (numOfResolved < 3)
                {
                    arDebugManager.LogInfo($"Resolving AnchorId #{anchorResolveInProgress}: {anchorToResolveList[anchorResolveInProgress]}");
                    CheckResolveProgress();
                }
            }
            else
            {
                AverageWorldOrigin();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
    }

}