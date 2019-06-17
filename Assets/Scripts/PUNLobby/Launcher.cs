using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using Mahjong.Model;
using Utils;
using GamePlay.Server.Model.Events;
using System.Reflection;
using ExitGames.Client.Photon;

namespace PUNLobby
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public static Launcher Instance { get; private set; }
        public SceneField roomScene;
        public PanelManager PanelManager;
        public RulePanel rulePanel;
        private string gameVersion = "1.0";

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

        private void Start()
        {
            RegisterTypes();
        }

        private static void RegisterTypes()
        {
            var types = typeof(EventMessages).GetNestedTypes(BindingFlags.Public);
            for (byte i = 0; i < types.Length; i++)
            {
                PhotonPeer.RegisterType(types[i], i, SerializeUtility.SerializeObject, SerializeUtility.DeserializeObject);
            }
            PhotonPeer.RegisterType(typeof(GameSetting), (byte)types.Length, SerializeUtility.SerializeObject, SerializeUtility.DeserializeObject);
        }

        public void SetPlayerName(string value)
        {
            PhotonNetwork.NickName = value;
        }

        public void CreateRoom()
        {
            PanelManager.ShowCreateRoomPanel();
        }

        internal void Connect(string playerName)
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
            PhotonNetwork.NickName = playerName;
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
                RoomOptions roomOptions = new RoomOptions
                {
                    IsOpen = true,
                    IsVisible = true,
                    MaxPlayers = (byte)setting.MaxPlayer,
                    CleanupCacheOnLeave = true,
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable(),
                    CustomRoomPropertiesForLobby = new string[] { SettingKeys.SETTING }
                };
                roomOptions.CustomRoomProperties.Add(SettingKeys.SETTING, setting);
                PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
            }
        }

        public void JoinRoom(string name)
        {
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
            Debug.Log($"OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Message: {message}.");
            PanelManager.warningPanel.Show(400, 200, "The room with same name already exists, please try again with another name.");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed. Message: {message}");
            PanelManager.warningPanel.Show(400, 200, $"Room does not exist.");
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed. Message: {message}");
        }

        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
            PhotonNetwork.LoadLevel(roomScene);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
        }

        public override void OnLeftLobby()
        {
            Debug.Log("OnLeftLobby");
        }

        public void ShowRulePanel(GameSetting gameSetting)
        {
            rulePanel.Show(gameSetting);
        }
    }
}