
using UnityEngine;
using System;
using Bolt;
using Bolt.Photon;
using Bolt.Matchmaking;
using UdpKit;
using UdpKit.Platform;

// [BoltGlobalBehaviour]
public class Menu : GlobalEventListener
{
    [SerializeField]
    private string sceneToLoad = "CloudAnchorBolt";

    // Start is called before the first frame update
    public void StartServer()
    {
        BoltLauncher.StartServer();
    }
    //
    // public override void BoltStartBegin()
    // {
    //   BoltNetwork.RegisterTokenClass<PhotonRoomProperties>();
    // }

    public override void BoltStartDone()
    {
      if (BoltNetwork.IsServer) 
      {
        BoltMatchmaking.CreateSession(sessionID: "test", sceneToLoad: sceneToLoad);
      }

      // BoltNetwork.EnableLanBroadcast();
    }

    public void StartClient()
    {
      //determine if the player is the server or the client
      BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        Debug.Log($"Session List Updated, found {sessionList.Count} session");

        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(photonSession);
            }
        }
        Debug.Log("The whole list is invalid");
    }

}
