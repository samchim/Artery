using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

public class ARCloudAnchorManagerBolt : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    private float resolveAnchorPassedTimeout = 10.0f;

    [SerializeField]
    private GameObject worldOriginPrefab;

    [SerializeField]
    public int NUM_OF_ANCHOR = 3;

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

    private GameObject[] _arPlanes;
    private GameObject worldOrigin = null;
    private float px, py, pz, qx, qy, qz, qw = 0;
    private GameObject worldOriginTmp;

    private ARAnchorManager _arAnchorManager = null;
    private ARPlacementManagerBolt _arPlacementManager = null;
    private ARDebugManager _arDebugManager = null;
    private CloudAnchorsMetaManager _arCloudAnchorsMetaManger = null;
    private ARPlaneManager _arPlaneManger = null;

    private void Awake()
    {
        _arPlacementManager = GetComponent<ARPlacementManagerBolt>();
        _arDebugManager = GetComponent<ARDebugManager>();
        _arCloudAnchorsMetaManger = GetComponent<CloudAnchorsMetaManager>();
        _arPlaneManger = GetComponent<ARPlaneManager>();

        resolver = new UnityEvent<Transform, int>();
        resolver.AddListener((t, i) => _arPlacementManager.ReCreatePlacement(t, i));

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

    public void hi()
    {
        _arDebugManager.LogInfo($"hi, this is ARCloudAnchorManagerBolt.cs");
    }

    public void QueueAnchor(ARAnchor arAnchor)
    {
        _arDebugManager.LogInfo($"new Anchor placed #{numOfQueued+1}");
        pendingHostAnchorList[numOfQueued] = arAnchor;
        numOfQueued++;
    }

    public void StartHostAnchor()
    {
        _arDebugManager.LogInfo($"Start Host Anchor, numOfQueued = {numOfQueued}");
        FeatureMapQuality quality = _arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
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
        _arDebugManager.LogInfo($"HostAnchor executing #" + (index + 1).ToString());
        cloudAnchorList[index] = _arAnchorManager.HostCloudAnchor(pendingHostAnchorList[index], 1);
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
                    // _arDebugManager.LogError($"Anchor #{(i + 1).ToString()} successfully hosted");

                    // keep track of cloud anchors added
                    anchorToResolveList[i] = cloudAnchorList[i].cloudAnchorId;
                    numOfSuccess++;
                }
                else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                {
                    _arDebugManager.LogError($"Fail to host anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
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
                _arDebugManager.LogError($"Unable to host cloud anchor #{(i + 1).ToString()}, numOfUnable ={numOfUnable}");
                // TODO: Remove the broken anchor and allow ARPlacementManger to add one new anchor to pendingHostAnchorList
            }

        }

        if (numOfFailed != 0 || numOfUnable != 0)
        {
            _arDebugManager.LogInfo($"Too much unable/failed to host, please pick anchor again");
            anchorUpdateInProgress = false;
            _arPlacementManager.RemovePlacements();
        }

        if (numOfSuccess == numOfQueued)
        {
            anchorUpdateInProgress = false;
            numOfToBeResolved = numOfSuccess;

            for (i = 0; i < numOfSuccess; i++)
            {
                _arDebugManager.LogError($"Anchor #{(i + 1).ToString()} successfully hosted");
            }

            _arCloudAnchorsMetaManger.SendUpdate(anchorToResolveList);
        }
    }

    public void StartResolve()
    {
        numOfToBeResolved = NUM_OF_ANCHOR;
        anchorToResolveList = _arCloudAnchorsMetaManger.getLocalCloudAnchorIdList();

        _arDebugManager.LogInfo($"Start Resolve Anchor, numOfToBeResolved = {numOfToBeResolved}");
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
        _arDebugManager.LogInfo($"Resolve executing #{index + 1} id #{anchorToResolveList[index]}");


        cloudAnchorList[index] = _arAnchorManager.ResolveCloudAnchorId(anchorToResolveList[index]);

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
            _arDebugManager.LogInfo($"#{i + 1}");
            try
            {
                if (cloudAnchorList[i])
                {
                    CloudAnchorState cloudAnchorState = cloudAnchorList[i].cloudAnchorState;

                    if (cloudAnchorState == CloudAnchorState.Success)
                    {
                        _arDebugManager.LogInfo($"#{i + 1}, CloudAnchorId: {cloudAnchorList[i].cloudAnchorId} resolved");

                        resolver.Invoke(cloudAnchorList[i].transform, i);
                        // arPlacementManager.ReCreatePlacement(cloudAnchor.transform, i);
                        numOfSuccess++;
                    }
                    else if (cloudAnchorState != CloudAnchorState.TaskInProgress)
                    {
                        _arDebugManager.LogError($"Fail to resolve Cloud Anchor #{(i + 1).ToString()} with state: {cloudAnchorState}");
                        numOfFailed++;
                    }
                    else
                    {
                        _arDebugManager.LogInfo($"CheckResolveProgress #{(i + 1).ToString()}  id {cloudAnchorList[i].cloudAnchorId}");
                    }
                }
                else
                {
                    numOfUnable++;
                    _arDebugManager.LogError($"Unable to resolve cloud achor #{i + 1} id {cloudAnchorList[i].cloudAnchorId}, numOfUnable = {numOfUnable}");
                }
            }
            catch (System.NullReferenceException e)
            {
                numOfUnable++;
                _arDebugManager.LogError($"Unable to resolve cloud achor #{i + 1} id {cloudAnchorList[i].cloudAnchorId}, numOfUnable = {numOfUnable}");
            }
        }

        _arDebugManager.LogInfo($"numOfSuccess = {numOfSuccess}, numOfFailed = {numOfFailed}, numOfUnable = {numOfUnable}");

        if (numOfUnable != 0 || numOfFailed != 0)
        {
            _arDebugManager.LogInfo($"Too much unable/failed to resolve, please resolve again");
            anchorResolveInProgress = false;
            _arPlacementManager.RemovePlacements();
        }

        if (numOfSuccess == numOfToBeResolved)
        {
            anchorResolveInProgress = false;
            StartGame();
        }
    }

    private void StartGame()
    {
        AverageWorldOrigin();
        _arPlaneManger.enabled = false;
        _arPlanes = GameObject.FindGameObjectsWithTag("ARPlane");
        foreach (GameObject ARPlane in _arPlanes) {
            ARPlane.SetActive(false);
        }
        _arPlacementManager.DeactivatePlacement();
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

    private void AverageWorldOrigin()
    {
        _arDebugManager.LogInfo($"AverageWorldOrigin()");
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
                _arDebugManager.LogInfo($"AverageWorldOrigin on anchor #{(i + 1)}");
                px += cloudAnchorList[i].transform.position.x;
                py += cloudAnchorList[i].transform.position.y;
                pz += cloudAnchorList[i].transform.position.z;
                qx += cloudAnchorList[i].transform.rotation.x;
                qy += cloudAnchorList[i].transform.rotation.y;
                qz += cloudAnchorList[i].transform.rotation.z;
                qw += cloudAnchorList[i].transform.rotation.w;
                _arDebugManager.LogInfo($"{px}, {py}, {pz}, {qx}, {qy}, {qz}, {qw}");
            }
        }

        Vector3 worldOriginPostion = new Vector3(px / (float)numOfCloudAnchor, py / (float)numOfCloudAnchor, pz / (float)numOfCloudAnchor);
        Quaternion worldOriginRotation = new Quaternion(qx / (float)numOfCloudAnchor, qy / (float)numOfCloudAnchor, qz / (float)numOfCloudAnchor, qw / (float)numOfCloudAnchor);
        _arDebugManager.LogInfo($"Position: {worldOriginPostion.ToString()}");
        _arDebugManager.LogInfo($"Rotation: {worldOriginRotation.ToString()}");
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