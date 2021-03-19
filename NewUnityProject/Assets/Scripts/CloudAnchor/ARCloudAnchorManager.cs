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
    private GameObject worldOriginPrefab;

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
    public int numOfUnable = 0;
    public int numOfFailed = 0;
    public int numOfSuccess = 0;
    // public int NumOfCloudAnchor = 0;
    public int numOfToBeResolved = 0;
    private List<ARCloudAnchor> cloudAnchorList = new List<ARCloudAnchor>();
    private int i;
    private int numOfCloudAnchor;
    private GameObject worldOrigin = null;

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
        worldOrigin = new GameObject();
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

    public void StartHostAnchor()
    {
        arDebugManager.LogInfo($"Start Host Anchor, numOfQueued = {numOfQueued}");
        FeatureMapQuality quality = arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        for (i = 0; i < numOfQueued; i++)
        {
            anchorToResolveList[i] = null;
        }
        for (i = 0; i < numOfQueued; i++)
        {
            HostAnchor(i);
        }
        numOfToBeResolved = 0;
        anchorUpdateInProgress = true;
    }

    public void HostAnchor(int index)
    {
        arDebugManager.LogInfo($"HostAnchor executing #" + (index + 1).ToString());
        cloudAnchorList[index] = arAnchorManager.HostCloudAnchor(pendingHostAnchorList[index], 1);
        // // Test Cannot Host
        // if (index == 1)
        // {
        //     cloudAnchorList[index] = null;
        // }

        // if (cloudAnchorList[index] == null)
        // {
        //     numOfUnable++;
        //     arDebugManager.LogError($"Unable to host cloud anchor #{(index + 1).ToString()}, numOfUnable ={numOfUnable}");
        //     // TODO: Remove the broken anchor and allow ARPlacementManger to add one new anchor to pendingHostAnchorList
        //     if (numOfUnable * 1.0 >= (numOfQueued * 0.5))
        //     {
        //         arDebugManager.LogInfo($"Too much unable to host, please pick anchor again");
        //         anchorUpdateInProgress = false;
        //         arPlacementManager.RemovePlacements();
        //     }
        // }
    }

    private void CheckHostingProgress()
    {
        numOfSuccess = 0;
        numOfFailed = 0;
        numOfUnable = 0;
        for (i = 0; i < numOfQueued; i++)
        {
            if (cloudAnchorList[i] != null)
            {
                CloudAnchorState cloudAnchorState = cloudAnchorList[i].cloudAnchorState;
                if (cloudAnchorState == CloudAnchorState.Success)
                {
                    arDebugManager.LogError($"Anchor #{(i + 1).ToString()} successfully hosted");

                    // keep track of cloud anchors added
                    anchorToResolveList[i] = cloudAnchorList[i].cloudAnchorId;
                    numOfSuccess++;
                }
                else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    arDebugManager.LogError($"Fail to host anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
                    numOfFailed++;
                }
                else
                {
                    // arDebugManager.LogInfo($"CheckHostingProgress #{(i + 1).ToString()}");
                    // arDebugManager.LogInfo($"#{(i + 1).ToString()}: NumOfCloudAnchor() = {NumOfCloudAnchor().ToString()}");
                }
            }
            else
            {
                numOfUnable++;
                arDebugManager.LogError($"Unable to host cloud anchor #{(i + 1).ToString()}, numOfUnable ={numOfUnable}");
                // TODO: Remove the broken anchor and allow ARPlacementManger to add one new anchor to pendingHostAnchorList
            }

        }

        if (numOfFailed != 0 || numOfUnable != 0)
        {
            arDebugManager.LogInfo($"Too much unable/failed to host, please pick anchor again");
            anchorUpdateInProgress = false;
            arPlacementManager.RemovePlacements();
        }

        if (numOfSuccess == numOfQueued)
        {
            anchorUpdateInProgress = false;
            numOfToBeResolved = numOfSuccess;
        }
    }

    public void StartResolve()
    {
        // numOfToBeResolved = NUM_OF_ANCHOR;
        arDebugManager.LogInfo($"Start Resolve Anchor, numOfToBeResolved = {numOfToBeResolved}");
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            cloudAnchorList[i] = null;
        }
        for (i = 0; i < numOfToBeResolved; i++)
        {
            Resolve(i);
        }
        anchorResolveInProgress = true;
    }

    public void Resolve(int index)
    {
        arDebugManager.LogInfo($"Resolve executing #{index + 1} id #{anchorToResolveList[index]}");

        // cloudAnchorList[index] = arAnchorManager.ResolveCloudAnchorId(anchorToResolveList[index]);

        cloudAnchorList[index] = arAnchorManager.ResolveCloudAnchorId(anchorToResolveList[index]);

        // if (cloudAnchorList[index] == null)
        // {
        //     arDebugManager.LogError($"Failed to resolve cloud achor #{index + 1} id {cloudAnchorList[index].cloudAnchorId}");
        //     // TODO
        //     numOfToBeResolved--;
        // }
    }

    private void CheckResolveProgress()
    {
        numOfSuccess = 0;
        numOfFailed = 0;
        numOfUnable = 0;
        for (i = 0; i < numOfToBeResolved; i++)
        {
            arDebugManager.LogInfo($"#{i + 1}");
            try
            {
                if (cloudAnchorList[i])
                {
                    CloudAnchorState cloudAnchorState = cloudAnchorList[i].cloudAnchorState;

                    if (cloudAnchorState == CloudAnchorState.Success)
                    {
                        arDebugManager.LogInfo($"#{i + 1}, CloudAnchorId: {cloudAnchorList[i].cloudAnchorId} resolved");

                        resolver.Invoke(cloudAnchorList[i].transform, i);
                        // arPlacementManager.ReCreatePlacement(cloudAnchor.transform, i);
                        numOfSuccess++;
                    }
                    else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                    {
                        arDebugManager.LogError($"Fail to resolve Cloud Anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
                        numOfFailed++;
                    }
                    else
                    {
                        arDebugManager.LogInfo($"CheckResolveProgress #{(i + 1).ToString()}  id {cloudAnchorList[i].cloudAnchorId}");
                    }
                }
                else
                {
                    numOfUnable++;
                    arDebugManager.LogError($"Unable to resolve cloud achor #{i + 1} id {cloudAnchorList[i].cloudAnchorId}, numOfUnable = {numOfUnable}");
                }
            }
            catch (System.NullReferenceException e)
            {
                numOfUnable++;
                arDebugManager.LogError($"Unable to resolve cloud achor #{i + 1} id {cloudAnchorList[i].cloudAnchorId}, numOfUnable = {numOfUnable}");
            }
        }

        arDebugManager.LogInfo($"numOfSuccess = {numOfSuccess}, numOfFailed = {numOfFailed}, numOfUnable = {numOfUnable}");

        if (numOfUnable != 0 || numOfFailed != 0)
        {
            arDebugManager.LogInfo($"Too much unable/failed to resolve, please resolve again");
            anchorResolveInProgress = false;
            arPlacementManager.RemovePlacements();
        }

        if (numOfSuccess == numOfToBeResolved)
        {
            anchorResolveInProgress = false;
            AverageWorldOrigin();
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

    private float px, py, pz, qx, qy, qz, qw = 0;
    private GameObject worldOriginTmp;
    private void AverageWorldOrigin()
    {
        arDebugManager.LogInfo($"AverageWorldOrigin()");
        if (worldOrigin != new GameObject())
        {
            Destroy(worldOrigin);
            worldOrigin = new GameObject();
        }
        px = 0;
        py = 0;
        pz = 0;
        qx = 0;
        qy = 0;
        qz = 0;
        qw = 0;
        numOfCloudAnchor = NumOfCloudAnchor();
        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            if (cloudAnchorList[i] != null)
            {
                arDebugManager.LogInfo($"AverageWorldOrigin on anchor #{(i + 1)}");
                px += cloudAnchorList[i].transform.position.x;
                py += cloudAnchorList[i].transform.position.y;
                pz += cloudAnchorList[i].transform.position.z;
                qx += cloudAnchorList[i].transform.rotation.x;
                qy += cloudAnchorList[i].transform.rotation.y;
                qz += cloudAnchorList[i].transform.rotation.z;
                qw += cloudAnchorList[i].transform.rotation.w;
                arDebugManager.LogInfo($"{px}, {py}, {pz}, {qx}, {qy}, {qz}, {qw}");
            }
        }

        Vector3 worldOriginPostion = new Vector3(px / (float)numOfCloudAnchor, py / (float)numOfCloudAnchor, pz / (float)numOfCloudAnchor);
        Quaternion worldOriginRotation = new Quaternion(qx / (float)numOfCloudAnchor, qy / (float)numOfCloudAnchor, qz / (float)numOfCloudAnchor, qw / (float)numOfCloudAnchor);
        arDebugManager.LogInfo($"Position: {worldOriginPostion.ToString()}");
        arDebugManager.LogInfo($"Rotation: {worldOriginRotation.ToString()}");
        worldOrigin = Instantiate(worldOriginPrefab) as GameObject;
        worldOrigin.transform.position = worldOriginPostion;
        worldOrigin.transform.rotation = worldOriginRotation;
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