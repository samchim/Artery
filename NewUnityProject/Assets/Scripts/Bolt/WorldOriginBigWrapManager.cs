using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class WorldOriginBigWrapManager : GlobalEventListener
{

    // private GameObject worldOriginPrefab = null;
    [SerializeField]
    private GameObject worldOrigin = null;

    [SerializeField]
    private GameObject bigWrapPrefab = null;
    private GameObject bigWrap = null;

    [SerializeField]
    private GameObject handColliderPrefab = null;
    private GameObject myHandCollider = null;
    private GameObject[] handColliderList = null;

    [SerializeField]
    private Vector3 worldOriginPosition = new Vector3(0, 0, 40);
    [SerializeField]
    private Quaternion worldOriginRotation = new Quaternion(0, 0, 0, 0);

    private GameObject[] arCubeList;
    private GameObject gameBase;

    private int i;

    private ARDebugManager _arDebugManager;

    private void Start()
    {
        _arDebugManager = gameObject.GetComponent<ARDebugManager>();
    }

    private void Update()
    {

    }

    public void ConnectBW2WO(Vector3 inputPosition, Quaternion inputRotation)
    // public void ConnectBW2WO(Transform inputTransform)
    {
        worldOriginPosition = inputPosition;
        worldOriginRotation = inputRotation;

        worldOrigin.transform.position = inputPosition;
        worldOrigin.transform.rotation = inputRotation;

        bigWrap = GameObject.FindGameObjectWithTag("BigWrap");
        if (bigWrap == null)
        {
            bigWrap = BoltNetwork.Instantiate(
                bigWrapPrefab, 
                inputPosition,
                inputRotation
            );
        }
        bigWrap.transform.parent = worldOrigin.transform;

        arCubeList = GameObject.FindGameObjectsWithTag("ARCube");
        for (i = 0; i < arCubeList.Length; i++)
        {
            // arCubeList[i].transform.position = arCubeList[i].GetComponent<ARCubeInteractionBolt>().state.ARCubeTransform.Position;
            // arCubeList[i].transform.rotation = arCubeList[i].GetComponent<ARCubeInteractionBolt>().state.ARCubeTransform.Rotation;
            arCubeList[i].transform.parent = bigWrap.transform;
        }
        gameBase = GameObject.FindGameObjectWithTag("GameBase");
        gameBase.transform.position = inputPosition;
        gameBase.transform.rotation = inputRotation;
        gameBase.transform.parent = bigWrap.transform;
        bigWrap.transform.position = inputPosition;
        bigWrap.transform.rotation = inputRotation;
        bigWrap.transform.parent = worldOrigin.transform;

        if (myHandCollider == null)
        {
            myHandCollider = BoltNetwork.Instantiate(handColliderPrefab, new Vector3(0, 10, 0), new Quaternion(0, 0, 0, 0));
            myHandCollider.transform.parent = worldOrigin.transform;
        }
        SendConnectEvent();
    }

    public override void OnEvent(ConnectEvent evnt)
    {
        _arDebugManager.LogInfo($"ConnectEvent is receviced");
        StartCoroutine(setCollidersParent());
    }

    IEnumerator setCollidersParent()
    {
        yield return new WaitForSeconds(0.5f);
        handColliderList = GameObject.FindGameObjectsWithTag("Player");
        for (i = 0; i < handColliderList.Length; i++)
        {
            handColliderList[i].transform.parent = worldOrigin.transform;
        }
    }

    public void SendConnectEvent()
    {
        ConnectEvent connect = ConnectEvent.Create();
        connect.Send();
    }

    public void PressToConnect()
    {
        ConnectBW2WO(worldOriginPosition, worldOriginRotation);
    }

}