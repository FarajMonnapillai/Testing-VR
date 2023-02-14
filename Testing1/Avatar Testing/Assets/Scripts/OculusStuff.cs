using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Avatar2;
using System;
using Photon.Pun;
using Photon.Realtime;
using CAPI = Oculus.Avatar2.CAPI;
using UnityEditor;

public class OculusStuff : MonoBehaviourPunCallbacks
{
    public UInt64 _userId = 0;
    public string appID;
    public string sampleAvatar = "sampleAvatar";
    //public StreamingAvatar _streamingAvatar;
    
    public GameObject playerGo, playerPrefab;
    public Transform StartRigGo;
    public int playerNum;
    public Transform [] playerSpwnPnts = new Transform[2];
    public bool ServerBool;
    private void Awake()
    {
        try
        {
            Core.AsyncInitialize();
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
        catch(UnityException e)
        {
            Debug.LogException(e);
            // Immediately quit the application.
            UnityEngine.Application.Quit();
        }
    
    void EntitlementCallback (Message msg)
    {
        if (msg.IsError) // User failed entitlement check
        {
        UnityEngine.Application.Quit();
        }
        else // User passed entitlement check
        {
        // Log the succeeded entitlement check for debugging.
        Debug.Log("You are entitled to use this app.");
        StartCoroutine(StartOvrPlatform());
        }
    }
  }  
  IEnumerator StartOvrPlatform()
  {
    if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", sampleAvatar);
                //LoadLocalAvatar();
                yield break;
            }

            yield return null;
        }

            //Get user ID
            Users.GetLoggedInUser().OnComplete(message =>
            {
                if (message.IsError)
                {
                    var e = message.GetError();
                    OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. Falling back to local avatar", sampleAvatar);
                }
                else
                {
                    _userId = message.Data.ID;
                    //build multiplayer login room

                    ConnectToServer();
                    //_streamingAvatar.gameObject.SetActive(true);
                    //_streamingAvatar.StartAvatar(this);
                }
            });
        }

        public void ConnectToServer()
        {
            PhotonNetwork.SendRate = 30;
            PhotonNetwork.SerializationRate = 20;
            PhotonNetwork.AutomaticallySyncScene = true;

            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions
            {
                MaxPlayers = 2,
                IsVisible = true,
                IsOpen = true,
            };

            PhotonNetwork.JoinOrCreateRoom("GameRoom", roomOptions, TypedLobby.Default);
        }
            public override void OnCreatedRoom()
            {
                base.OnCreatedRoom();
                ServerBool = true;
            }

            public override void OnJoinedRoom()
            {
                base.OnJoinedRoom();
                
                playerNum = PhotonNetwork.CurrentRoom.PlayerCount - 1;
                //only 2 players in room options

                if(playerNum <= playerSpwnPnts.Length)
                {
                    StartRigGo = playerSpwnPnts[playerNum].transform;
                }
                else
                {
                    StartRigGo = playerSpwnPnts[0];
                }

                SpawnPlayer();
            }

            private void SpawnPlayer()
            {
                object[] userID0 = new object[1] {Convert.ToInt64(_userId)};
                playerGo = PhotonNetwork.Instantiate(playerPrefab.name, StartRigGo.position, StartRigGo.rotation, 0, userID0);
                StartRigGo.gameObject.SetActive(false);
            }
}
  
