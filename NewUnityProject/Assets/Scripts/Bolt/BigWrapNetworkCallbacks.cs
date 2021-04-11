using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class BigWrapNetworkCallbacks : Bolt.EntityBehaviour<ICubeState>
{
    [SerializeField]
    private GameObject arCubePrefab;
    [SerializeField]
    private GameObject gameBasePrefab;



    public override void Attached()
    {
        if (entity.IsOwner)
        {
            BoltNetwork.Instantiate(gameBasePrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0)).transform.parent = transform;
            BoltNetwork.Instantiate(arCubePrefab, new Vector3((float)0.86, (float)0.917, (float)0.45), new Quaternion(0, 0, 0, 0)).transform.parent = transform;
            BoltNetwork.Instantiate(arCubePrefab, new Vector3((float)-0.879, (float)1.189, (float)-0.198), new Quaternion(0, 0, 0, 0)).transform.parent = transform;
            BoltNetwork.Instantiate(arCubePrefab, new Vector3((float)0.273, (float)1.55, (float)-0.253), new Quaternion(0, 0, 0, 0)).transform.parent = transform;
        }
    }
}
