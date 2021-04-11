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

    private void Start() {

    }

    private void Update() {
        
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
            bigWrap = BoltNetwork.Instantiate(bigWrapPrefab, new Vector3(0, 0 , 0), new Quaternion(0, 0, 0, 0));
        }
        bigWrap.transform.parent = worldOrigin.transform;
        bigWrap.transform.position = new Vector3(0, 0 , 0);
        bigWrap.transform.rotation = new Quaternion(0, 0, 0, 0);

        arCubeList = GameObject.FindGameObjectsWithTag("ARCube");
        for (i = 0; i < arCubeList.Length; i++)
        {
            arCubeList[i].transform.parent = bigWrap.transform;
        }
        gameBase = GameObject.FindGameObjectWithTag("GameBase");
        gameBase.transform.parent = bigWrap.transform;

        handColliderList = GameObject.FindGameObjectsWithTag("Player");
        if (myHandCollider == null)
        {
            myHandCollider = BoltNetwork.Instantiate(handColliderPrefab, new Vector3(0, 10 , 0), new Quaternion(0, 0, 0, 0));
            myHandCollider.transform.parent = worldOrigin.transform;
        }
        SendConnectEvent();
    }

    public override void OnEvent(ConnectEvent evnt)
    {   
        for (i = 0; i < handColliderList.Length; i++){
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