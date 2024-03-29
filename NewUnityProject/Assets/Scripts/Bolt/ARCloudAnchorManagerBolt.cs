﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.XR.ARCoreExtensions;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using System;

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
    private Vector3 worldOriginPostion = new Vector3(0, 0, 0);
    private Quaternion worldOriginRotation = new Quaternion(0, 0, 0, 0);

    private ARAnchorManager _arAnchorManager = null;
    private ARPlacementManagerBolt _arPlacementManager = null;
    private ARDebugManager _arDebugManager = null;
    private CloudAnchorsMetaManager _arCloudAnchorsMetaManger = null;
    private ARPlaneManager _arPlaneManger = null;
    private WorldOriginBigWrapManager _worldOriginBigWrapManager = null;

    private void Awake()
    {
        _arPlacementManager = GetComponent<ARPlacementManagerBolt>();
        _arDebugManager = GetComponent<ARDebugManager>();
        _arCloudAnchorsMetaManger = GetComponent<CloudAnchorsMetaManager>();
        _arPlaneManger = GetComponent<ARPlaneManager>();
        _worldOriginBigWrapManager = GetComponent<WorldOriginBigWrapManager>();

        resolver = new UnityEvent<Transform, int>();
        resolver.AddListener((t, i) => _arPlacementManager.ReCreatePlacement(t, i));

        for (i = 0; i < NUM_OF_ANCHOR; i++)
        {
            pendingHostAnchorList.Add(null);
            anchorToResolveList.Add(null);
            cloudAnchorList.Add(null);
        }

        // FeatureMapQuality quality = _arAnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
        // if (quality != 0)
        // {
        //     _arCloudAnchorsMetaManger.SendJoin((int)quality);
        // } else
        // {
        //     _arCloudAnchorsMetaManger.SendJoin(0);
        // }
        // _arCloudAnchorsMetaManger.SendJoin(1);
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
        _arDebugManager.LogInfo($"new Anchor placed #{numOfQueued + 1}");
        pendingHostAnchorList[numOfQueued] = arAnchor;
        numOfQueued++;
    }

    public void StartHostAnchor()
    {
        _arDebugManager.LogInfo($"Start Host Anchor, numOfQueued = {numOfQueued}");
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
        foreach (GameObject ARPlane in _arPlanes)
        {
            ARPlane.SetActive(false);
        }
        _arPlacementManager.DeactivatePlacement();
        _arDebugManager.LogInfo($"StartGame");
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
    private Vector4 cumulative;
    private void AverageWorldOrigin()
    {
        _arDebugManager.LogInfo($"AverageWorldOrigin()");
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

                // qx += cloudAnchorList[i].transform.rotation.x;
                // qy += cloudAnchorList[i].transform.rotation.y;
                // qz += cloudAnchorList[i].transform.rotation.z;
                // qw += cloudAnchorList[i].transform.rotation.w;
                if (i == 0)
                {
                    worldOriginRotation = cloudAnchorList[i].transform.rotation;
                }
                else
                {
                    worldOriginRotation = AverageQuaternion(ref cumulative, cloudAnchorList[i].transform.rotation, cloudAnchorList[0].transform.rotation, i);
                }
                // _arDebugManager.LogInfo($"{px}, {py}, {pz}, {qx}, {qy}, {qz}, {qw}");
            }
        }

        // worldOriginPostion = new Vector3(px / (float)numOfCloudAnchor, py / (float)numOfCloudAnchor, pz / (float)numOfCloudAnchor);
        worldOriginPostion.x = px / (float)numOfCloudAnchor;
        worldOriginPostion.y = py / (float)numOfCloudAnchor;
        worldOriginPostion.z = pz / (float)numOfCloudAnchor;
        // worldOriginRotation = new Quaternion(qx / (float)numOfCloudAnchor, qy / (float)numOfCloudAnchor, qz / (float)numOfCloudAnchor, qw / (float)numOfCloudAnchor);
        // worldOriginRotation.x = qx / (float)numOfCloudAnchor;
        // worldOriginRotation.y = qy / (float)numOfCloudAnchor;
        // worldOriginRotation.z = qz / (float)numOfCloudAnchor;
        // worldOriginRotation.w = qw / (float)numOfCloudAnchor;
        _arDebugManager.LogInfo($"Position: {worldOriginPostion.ToString()}");
        _arDebugManager.LogInfo($"Rotation: {worldOriginRotation.ToString()}");
        // worldOrigin = Instantiate(worldOriginPrefab) as GameObject;
        // worldOrigin.transform.position = worldOriginPostion;
        // worldOrigin.transform.rotation = worldOriginRotation;
        _worldOriginBigWrapManager.ConnectBW2WO(worldOriginPostion, worldOriginRotation);
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



    //////////////////////////////////////////

    //Get an average (mean) from more then two quaternions (with two, slerp would be used).
    //Note: this only works if all the quaternions are relatively close together.
    //Usage: 
    //-Cumulative is an external Vector4 which holds all the added x y z and w components.
    //-newRotation is the next rotation to be added to the average pool
    //-firstRotation is the first quaternion of the array to be averaged
    //-addAmount holds the total amount of quaternions which are currently added
    //This function returns the current average quaternion
    public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
    {

        float w = 0.0f;
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        if (!AreQuaternionsClose(newRotation, firstRotation))
        {
            newRotation = InverseSignQuaternion(newRotation);
        }

        //Average the values
        float addDet = 1f / (float)addAmount;
        cumulative.w += newRotation.w;
        w = cumulative.w * addDet;
        cumulative.x += newRotation.x;
        x = cumulative.x * addDet;
        cumulative.y += newRotation.y;
        y = cumulative.y * addDet;
        cumulative.z += newRotation.z;
        z = cumulative.z * addDet;

        //note: if speed is an issue, you can skip the normalization step
        return NormalizeQuaternion(x, y, z, w);
    }

    public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
    {
        float lengthD = 1.0f / (w * w + x * x + y * y + z * z);
        w *= lengthD;
        x *= lengthD;
        y *= lengthD;
        z *= lengthD;
        return new Quaternion(x, y, z, w);
    }

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
    {
        float dot = Quaternion.Dot(q1, q2);
        if (dot < 0.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}