
using UnityEngine;
using System;
using Bolt;
using Bolt.Photon;
using Bolt.Matchmaking;
using UdpKit;
using UdpKit.Platform;

// hello form my window machine

public class Menu : GlobalEventListener
{
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
        BoltMatchmaking.CreateSession(sessionID: "test", sceneToLoad: "Game");
      }

      // BoltNetwork.EnableLanBroadcast();
    }


    // Update is called once per frame
    public void StartClient()
    {
      //determine if the player is the server or the client
      BoltLauncher.StartClient();
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
      foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(photonSession);
            }
        }
    }


}
