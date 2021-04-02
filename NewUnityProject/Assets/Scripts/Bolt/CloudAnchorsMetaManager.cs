using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Google.XR.ARCoreExtensions;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[BoltGlobalBehaviour]
public class CloudAnchorsMetaManager : GlobalEventListener
{
    public List<string> localPlayerIdList = new List<string>();
    public List<int> localQualitiesList = new List<int>(); 
    public List<string> localCloudAnchorIdList = new List<string>();

    private int NUM_OF_ANCHOR = 0;
    private int NUM_OF_MAX_PLAYER = 4;
    private int numOfPlayer = 0;
    private int i;

    private ARCloudAnchorManagerBolt _arCloudAnchorManager = null;

    private void Awake() {
        _arCloudAnchorManager = GetComponent<ARCloudAnchorManagerBolt>();

        NUM_OF_ANCHOR = _arCloudAnchorManager.NUM_OF_ANCHOR;

        for (i = 0; i< NUM_OF_MAX_PLAYER; i++)
        {
            localPlayerIdList.Add(null);
            localQualitiesList.Add(-1);
        }

        for (i = 0; i< NUM_OF_ANCHOR; i++)
        {
            localCloudAnchorIdList.Add(null);
        }
    }

    public void SendJoin(int quality)
    {
        JoinEvent join = JoinEvent.Create();
        string id = "P" + (numOfPlayer + 1);
        join.Index = numOfPlayer; 
        join.PlayerID = id;
        join.Quality = quality;
        join.Send();
    }

    public override void OnEvent(JoinEvent evnt)
    {
        localPlayerIdList[evnt.Index] = evnt.PlayerID;
        localQualitiesList[evnt.Index] = evnt.Quality;
        numOfPlayer += 1;
    }

    public void SendUpdate(List<string> cloudAnchorIdList)
    {
        for (i =0; i < NUM_OF_ANCHOR; i++)
        {
            localCloudAnchorIdList[i] = cloudAnchorIdList[i];
            UpdateCloudAnchorIdEvent updateCloudAnchorId = UpdateCloudAnchorIdEvent.Create();
            updateCloudAnchorId.Index = i;
            updateCloudAnchorId.CloudAnchorID = cloudAnchorIdList[i];
            updateCloudAnchorId.Send();
        }
    }

    public override void OnEvent(UpdateCloudAnchorIdEvent evnt)
    {
        localCloudAnchorIdList[evnt.Index] = evnt.CloudAnchorID;
    }

    public List<string> getLocalCloudAnchorIdList()
    {
        List<string> localCloudAnchorIdListTmp = new List<string>(localCloudAnchorIdList.ToArray());

        return localCloudAnchorIdListTmp;
    }
}
