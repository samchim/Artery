using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class CloudAnchorsMetaManager : Bolt.EntityBehaviour<ICloudAnchorsMeta>
{
    private int numOfPlayer = 0;

    private int NUM_OF_ANCHOR = 3;

    [SerializeField]
    private List<string> players = new List<string>();

    [SerializeField]
    private List<string> cloudAnchorsID = new List<string>();


    private void Awake() {
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            players.Add(null);
            cloudAnchorsID.Add(null);
        }
    }

    public string join(int quality)
    {
        string id = "P" + (numOfPlayer + 1);
        state.Players[numOfPlayer] = id;
        state.Qualities[numOfPlayer] = quality;
        players[numOfPlayer] = id;
        numOfPlayer++;
        numOfPlayer = numOfPlayer % NUM_OF_ANCHOR;
        return (id);
    }

    public void uploadAnchorToResolveList(List<string> anchorToResolveList)
    {
        for (int i = 0; i < NUM_OF_ANCHOR; i++)
        {
            cloudAnchorsID[i] = anchorToResolveList[i];
            state.CloudAnchorsID[i] = anchorToResolveList[i];
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
