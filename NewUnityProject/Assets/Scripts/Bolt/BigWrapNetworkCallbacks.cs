using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class BigWrapNetworkCallbacks : Bolt.EntityBehaviour<ICubeState>
{
    [SerializeField]
    private GameObject gameBasePrefab;
    [SerializeField]
    private GameObject arCubePrefab;

    private GameObject tempGameBase;
    private GameObject tempArCube;

    private void Start() {
        
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            tempGameBase = BoltNetwork.Instantiate(
                gameBasePrefab, 
                transform.TransformPoint(Vector3.zero), 
                transform.localRotation
            );
            tempGameBase.transform.parent = transform;

            tempArCube = BoltNetwork.Instantiate(
                arCubePrefab,
                transform.TransformPoint(new Vector3(0.86f, 0.917f, 0.45f)),
                transform.localRotation
            );
            tempArCube.transform.parent = transform;
            tempArCube = BoltNetwork.Instantiate(
                arCubePrefab,
                transform.TransformPoint(new Vector3(-0.879f, 1.189f, -0.198f)), 
                transform.localRotation
            );
            tempArCube.transform.parent = transform;
            tempArCube = BoltNetwork.Instantiate(
                arCubePrefab, 
                transform.TransformPoint(new Vector3(0.273f, 1.55f, -0.253f)), 
                transform.localRotation
            );
            tempArCube.transform.parent = transform;
        }
    }
}
