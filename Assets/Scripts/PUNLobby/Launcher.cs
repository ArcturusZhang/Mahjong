using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using Mahjong.Model;
using Utils;

namespace PUNLobby
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public static Launcher Instance { get; private set; }
        public SceneField roomScene;
        public PanelManager PanelManager;
        private string gameVersion = "1.0";
        private bool joiningRoom = false;

        public override void OnEnable()
        {
            base.OnEnable();
            Instance = this;
            //This makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
            if (!PhotonNetwork.IsConnected)
            {
                // Set the App version before connecting
                PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
                // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
                // PhotonNetwork.ConnectUsingSettings();
                PanelManager.ChangeTo(PanelManager.LoginPanel);
            }
            else
            {
                PanelManager.ChangeTo(PanelManager.LobbyPanel);
            }
        }

        public void SetPlayerName(string value)
        {
            PhotonNetwork.NickName = value;
        }

        public void CreateRoom()
        {
            PanelManager.ShowCreateRoomPanel();
        }

        internal void Connect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
                PanelManager.ChangeTo(PanelManager.LobbyPanel);
            }
            else
            {
                // todo -- already connected to photon server
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("OnDisconnected. StatusCode: " + cause.ToString() + " ServerAddress: " + PhotonNetwork.ServerAddress);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            // After we connected to Master server, join the Lobby
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("We have received the Room list");
            //After this callback, update the room list
            PanelManager.SetRoomList(roomList);
        }

        public void CreateRoom(string roomName, GameSetting setting)
        {
            if (!string.IsNullOrEmpty(roomName))
            {
                joiningRoom = true;
                RoomOptions roomOptions = new RoomOptions
                {
                    IsOpen = true,
                    IsVisible = true,
                    MaxPlayers = (byte)setting.MaxPlayer,
                    CleanupCacheOnLeave = true,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
                };
                roomOptions.CustomRoomProperties.Add(SettingKeys.SETTING, setting.ToString());
                // todo -- Prevent create room with same name
                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
            }
        }

        public void JoinRoom(string name)
        {
            joiningRoom = true;
            //Join the Room
            PhotonNetwork.JoinRoom(name);
        }

        public void Refresh()
        {
            if (PhotonNetwork.IsConnected)
            {
                //Re-join Lobby to get the latest Room list
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
            else
            {
                //We are not connected, establish a new connection
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
            joiningRoom = false;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
            joiningRoom = false;
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed.");
            joiningRoom = false;
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
            //Load the Scene called GameLevel (Make sure it's added to build settings)
            PhotonNetwork.LoadLevel(roomScene);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
        }
    }
}