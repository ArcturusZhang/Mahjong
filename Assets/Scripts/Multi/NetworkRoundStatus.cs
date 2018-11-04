using System;
using Lobby;
using Single;
using Single.MahjongDataType;
using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Multi
{
    public class NetworkRoundStatus : NetworkBehaviour
    {
        [Header("UI Elements")] public Text FieldInfo;
        public Text RoundInfo;
        public Image[] Positions;

        [Header("UI Sprites")] public SpriteBundle PositionSprites;

        [Header("Round Data")] [SyncVar(hook = nameof(OnRoundData))]
        public RoundData RoundData;

        [SyncVar(hook = nameof(OnCurrentExtraRound))]
        public int CurrentExtraRound;

        [SyncVar] public int RichiSticks;
        [SyncVar] public int TotalPlayer;
        [SyncVar] public int TilesLeft;

        [Server]
        public void Initialize(int totalPlayer)
        {
            RoundData = new RoundData {RoundCount = 0, FieldCount = 1};
            CurrentExtraRound = 0;
            RichiSticks = 0;
            TotalPlayer = totalPlayer;
            TilesLeft = MahjongConstants.TotalTilesCount;
        }

        [Server]
        public void NextRound(bool newRound, bool extra)
        {
            if (newRound)
            {
                int newRoundCount = RoundData.RoundCount + 1;
                int newFieldCount = RoundData.FieldCount;
                if (newRoundCount > TotalPlayer)
                {
                    newRoundCount -= TotalPlayer;
                    newFieldCount++;
                }

                RoundData = new RoundData {RoundCount = newRoundCount, FieldCount = newFieldCount};

                if (extra) CurrentExtraRound++;
                else
                {
                    CurrentExtraRound = 0;
                    RichiSticks = 0;
                }
            }
            else
            {
                CurrentExtraRound++;
                RichiSticks = 0;
            }
        }

        [Server]
        public void RemoveTiles(int count)
        {
            TilesLeft -= count;
        }

        [Client]
        public Tile SelfWind(int playerIndex)
        {
            int index = playerIndex - (RoundData.RoundCount - 1);
            index = MahjongConstants.RepeatIndex(index, TotalPlayer);
            Assert.IsTrue(index >= 0 && index < TotalPlayer, "Self wind should be one of E, S, W, N");
            return new Tile(Suit.Z, index + 1);
        }

        [Client]
        public Tile PrevailingWind()
        {
            return new Tile(Suit.Z, RoundData.FieldCount);
        }

        #region Hooks

        private void OnRoundData(RoundData data)
        {
            Debug.Log($"{nameof(OnRoundData)} is called");
            RoundData = data;
            if (data.RoundCount == 0)
            {
                FieldInfo.gameObject.SetActive(false);
                return;
            }
            FieldInfo.gameObject.SetActive(true);
            RoundInfo.gameObject.SetActive(true);

            var field = MahjongConstants.PositionWinds[RoundData.FieldCount - 1];
            var round = MahjongConstants.NumberCharacters[RoundData.RoundCount];

            FieldInfo.text = $"{field}{round}局";
            int localPlayerIndex = LobbyManager.Instance.LocalPlayer.PlayerIndex;
            int localWindIndex = localPlayerIndex - RoundData.RoundCount + 1;
            Assert.AreEqual(Positions.Length, 4);
            for (int i = 0; i < Positions.Length; i++)
            {
                // todo -- side with no player should not be active
                Positions[i].gameObject.SetActive(true);
                var sprite = 
                    PositionSprites.Get(MahjongConstants.RepeatIndex(localWindIndex + i, Positions.Length));
                Positions[i].sprite = sprite;
            }
        }

        private void OnCurrentExtraRound(int extra)
        {
            Debug.Log($"{nameof(OnCurrentExtraRound)} is called");
            CurrentExtraRound = extra;
            RoundInfo.text = $"{CurrentExtraRound}本场";
        }

        #endregion

        #region Properties

        public int RoundCount => RoundData.RoundCount;

        public int FieldCount => RoundData.FieldCount;

        #endregion
    }

    [Serializable]
    public struct RoundData
    {
        public int RoundCount;
        public int FieldCount;
    }
}