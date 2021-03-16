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
    private bool anchorUpdateInProgress = false;
    private bool anchorResolveInProgress = false;
    private float safeToResolvePassed = 0;
    private UnityEvent<Transform, int> resolver = null;

    private List<ARAnchor> pendingHostAnchorList = new List<ARAnchor>();
    private List<string> anchorToResolveList = new List<string>();
    public int numOfQueued = 0;
    // public int NumOfCloudAnchor = 0;
    public int numOfToBeResolved = 0;
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
            anchorToResolveList.Add(null);
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
        FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        for (i = 0; i < numOfQueued; i++)
        {
            anchorToResolveList[i] = null;
        }
        for (i = 0; i < numOfQueued; i++)
        {
            HostAnchor(i);
        }
        anchorUpdateInProgress = true;
    }

    public void HostAnchor(int index)
    {
        arDebugManager.LogInfo($"HostAnchor executing #" + (index + 1).ToString());
        cloudAnchorList[index] = arAnchorManager.HostCloudAnchor(pendingHostAnchorList[index], 1);
        if (cloudAnchorList[index] == null)
        {
            arDebugManager.LogError($"Unable to host cloud anchor #{(index + 1).ToString()}");
            numOfQueued--;

            // TODO: Remove the broken anchor and allow ARPlacementManger to add one new anchor to pendingHostAnchorList
        }
    }

    private int NumOfCloudAnchor()
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
                    // arDebugManager.LogInfo($"CheckHostingProgress #{(i + 1).ToString()}");
                    // arDebugManager.LogInfo($"#{(i + 1).ToString()}: NumOfCloudAnchor() = {NumOfCloudAnchor().ToString()}");
                }
            }
            else
            {
                arDebugManager.LogInfo($"CheckHostingProgress #{(i + 1).ToString()} is null");
            }
        }
        if (NumOfCloudAnchor() == numOfQueued)
            anchorUpdateInProgress = false;
    }

    public void StartResolve()
    {
        numOfToBeResolved = NUM_OF_ANCHOR;
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            cloudAnchorList[i] = null;
        }
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            Resolve(i);
        }
        anchorResolveInProgress = true;
    }

    public void Resolve(int index)
    {
        arDebugManager.LogInfo($"Resolve executing #{index + 1} id #{anchorToResolveList[index]}");

        cloudAnchorList[index] = arAnchorManager.ResolveCloudAnchorId(anchorToResolveList[index]);

        if (cloudAnchorList[index] == null)
        {
            arDebugManager.LogError($"Failed to resolve cloud achor #{index + 1} id {cloudAnchorList[index].cloudAnchorId}");
            // TODO
            numOfToBeResolved--;
        }
    }

    private void CheckResolveProgress()
    {
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (cloudAnchorList[i] != null)
            {
                CloudAnchorState cloudAnchorState = cloudAnchorList[i].cloudAnchorState;

                if (cloudAnchorState == CloudAnchorState.Success)
                {
                    arDebugManager.LogInfo($"CloudAnchorId: {cloudAnchorList[i].cloudAnchorId} resolved");

                    resolver.Invoke(cloudAnchorList[i].transform, i);
                    // arPlacementManager.ReCreatePlacement(cloudAnchor.transform, i);
                }
                else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    arDebugManager.LogError($"Fail to resolve Cloud Anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
                }
                else
                {
                    arDebugManager.LogInfo($"CheckResolveProgress #{(i + 1).ToString()}  id {cloudAnchorList[i].cloudAnchorId}");
                }
            }
        }
        if (NumOfCloudAnchor() == numOfToBeResolved)
        {
            anchorResolveInProgress = false;
            AverageWorldOrigin();
        }
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
            if (!cloudAnchorList[i].Equals(null))
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
        worldOrigin.transform.position = new Vector3(px / numOfToBeResolved, py / numOfToBeResolved, pz / numOfToBeResolved);
        worldOrigin.transform.rotation = new Quaternion(qx / numOfToBeResolved, qy / numOfToBeResolved, qz / numOfToBeResolved, qw / numOfToBeResolved);
    }

    #endregion

    void Update()
    {
        // check progress of new anchors created
        if (anchorUpdateInProgress)
        {
            CheckHostingProgress();
            return;
        }

        if (safeToResolvePassed <= 0)
        {
            // check evey (resolveAnchorPassedTimeout)
            safeToResolvePassed = resolveAnchorPassedTimeout;

            if (anchorResolveInProgress)
            {
                CheckResolveProgress();
            }
        }
        else
        {
            safeToResolvePassed -= Time.deltaTime * 1.0f;
        }
    }

}