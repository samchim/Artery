using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class CloudAnchorsMetaManagerOld : Bolt.EntityBehaviour<ICloudAnchorsMeta>
{
    private int numOfPlayer = 0;

    private int NUM_OF_ANCHOR = 3;

    public List<string> players = new List<string>();
    public List<int> qualites = new List<int>();
    public List<string> anchors = new List<string>();

    private void Awake() {
        for (int i =0; i < 4; i++){
            players.Add(null);
            qualites.Add(0);
        }
        for (int i =0; i < 3; i++){
            anchors.Add(null);
        }
    }

    public string confirmimg(){
        Debug.Log("_cloudAnchorsMetaManager successfully hocked");
        return "_cloudAnchorsMetaManager successfully hocked";
    }

    // public string join(int quality)
    public string join()
    {
        string id = "P" + (numOfPlayer + 1);
        state.Players[numOfPlayer] = id;
        // state.Qualities[numOfPlayer] = quality;
        players[numOfPlayer] = id;
        // qualites[numOfPlayer] = quality;        
        numOfPlayer++;
        numOfPlayer = numOfPlayer % NUM_OF_ANCHOR;
        return (id);
    }

    public void uploadAnchorToResolveList(List<string> anchorToResolveList)
    {
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            state.CloudAnchorsID[i] = anchorToResolveList[i];
            anchors[i] = anchorToResolveList[i];
        }
    }

    public List<string> downloadAnchorToResolveList()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            output.Add(state.CloudAnchorsID[i]);
        }

        return output;
    }
}
