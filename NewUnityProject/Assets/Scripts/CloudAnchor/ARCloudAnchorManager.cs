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
    private bool anchorUpdateInProgress = false;
    private List<string> anchorToResolveList = new List<string>();
    private int anchorResolveInProgress = 0;
    public int numOfQueued = 0;
    // public int numOfHosted = 0;
    public int numOfResolved = 0;
    private List<ARCloudAnchor> cloudAnchorList = new List<ARCloudAnchor>();
    private int i;

    private ARPlacementManager arPlacementManager = null;
    private ARDebugManager arDebugManager = null;

    private void Awake()
    {
        arPlacementManager = GetComponent<ARPlacementManager>();
        arDebugManager = GetComponent<ARDebugManager>();

        resolver = new UnityEvent<Transform, int>();
        resolver.AddListener((t, i) => arPlacementManager.ReCreatePlacement(t, i));

        for (i = 0; i < NUM_OF_ANCHOR; i++)
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
        for (i = 0; i < numOfQueued; i++)
        {
            HostAnchor(i);
        }
        anchorUpdateInProgress = true;
    }

    public void HostAnchor(int index)
    {
        arDebugManager.LogInfo($"HostAnchor executing #" + (index + 1).ToString());

        if (index == 0)
        {
            FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        }

        cloudAnchorList[index] = arAnchorManager.HostCloudAnchor(pendingHostAnchorList[index], 1);
        if (cloudAnchorList[index] == null)
        {
            arDebugManager.LogError($"Unable to host cloud anchor #{(index + 1).ToString()}");
            numOfQueued--;
        }
    }

    private int NumOfHosted()
    {
        int n = 0;
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (cloudAnchorList[i] != null
            && (cloudAnchorList[i].cloudAnchorState == CloudAnchorState.Success || cloudAnchorList[i].cloudAnchorState != CloudAnchorState.TaskInProgress))
            {
                n += 1;
            }
        }
        return n;
    }

    private void CheckHostingProgress()
    {
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (cloudAnchorList[i] != null)
            {
                CloudAnchorState cloudAnchorState = cloudAnchorList[i].cloudAnchorState;
                if (cloudAnchorState == CloudAnchorState.Success)
                {
                    arDebugManager.LogError($"Anchor #{(i + 1).ToString()} successfully hosted");

                    // keep track of cloud anchors added
                    anchorToResolveList[i] = cloudAnchorList[i].cloudAnchorId;
                }
                else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    arDebugManager.LogError($"Fail to host anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
                }
                else
                {
                    arDebugManager.LogInfo($"CheckHostingProgress #{(i + 1).ToString()}");
                    // arDebugManager.LogInfo($"#{(i + 1).ToString()}: NumOfHosted() = {NumOfHosted().ToString()}");
                }
            }
            else 
            {
                arDebugManager.LogInfo($"CheckHostingProgress #{(i + 1).ToString()} is null");
            }
        }
        if (NumOfHosted() >= numOfQueued)
            anchorUpdateInProgress = false;
    }

    public void StartResolve()
    {
        for (i = 0; i < NUM_OF_ANCHOR; i++)
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
        for (i = 0; i < NUM_OF_ANCHOR; i++)
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
        // before
        // if (0 < anchorUpdateInProgress && anchorUpdateInProgress <= NUM_OF_ANCHOR)
        // after
        if (anchorUpdateInProgress)
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