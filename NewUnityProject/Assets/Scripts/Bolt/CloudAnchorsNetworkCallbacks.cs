using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class CloudAnchorsNetworkCallbacks : GlobalEventListener
{
    [SerializeField]
    public GameObject cloudAnchorsMetaBoltPrefab;

    public GameObject cloudAnchorsMetaBolt = null;

    public override void SceneLoadLocalDone(string scene)
    {
        if (BoltNetwork.IsServer)
        {
            cloudAnchorsMetaBolt = GameObject.FindWithTag("CloudAnchorsMetaBolt");
            if (cloudAnchorsMetaBolt == null)
            {
                cloudAnchorsMetaBolt = BoltNetwork.Instantiate(cloudAnchorsMetaBoltPrefab);
            }
        }
    }
}
