using System.Linq;
using GamePlay.Client.Controller;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Events;
using Mahjong.Logic;
using Mahjong.Model;
using Photon.Pun;
using UnityEngine;


namespace GamePlay.Server.Controller.GameState
{
    /// <summary>
    /// When the server is in this state, the server will distribute initial tiles for every player, 
    /// and will determine the initial dora indicator(s) according to the settings.
    /// All the data such as initial tiles, initial dora indicators, and mahjongSetData.
    /// Transfers to PlayerDrawTileState. The state transfer will be done regardless whether enough client responds received.
    /// </summary>
    public class RoundStartState : ServerState
    {
        public MahjongSet MahjongSet;
        public bool NextRound;
        public bool ExtraRound;
        public bool KeepSticks;
        private bool[] responds;
        private float firstTime;
        public override void OnServerStateEnter()
        {
            MahjongSet.Reset();
            // throwing dice
            var dice = Random.Range(CurrentRoundStatus.GameSettings.DiceMin, CurrentRoundStatus.GameSettings.DiceMax + 1);
            CurrentRoundStatus.NextRound(dice, NextRound, ExtraRound, KeepSticks);
            // draw initial tiles
            DrawInitial();
            Debug.Log("[Server] Initial tiles distribution done");
            CurrentRoundStatus.SortHandTiles();
            CurrentRoundStatus.SetBonusTurnTime(gameSettings.BonusTurnTime);
            responds = new bool[players.Count];
            var room = PhotonNetwork.CurrentRoom;
            for (int index = 0; index < players.Count; index++)
            {
                var tiles = CurrentRoundStatus.HandTiles(index);
                Debug.Log($"[Server] Hand tiles of player {index}: {string.Join("", tiles)}");
                var info = new EventMessages.RoundStartInfo
                {
                    PlayerIndex = index,
                    Field = CurrentRoundStatus.Field,
                    Dice = CurrentRoundStatus.Dice,
                    Extra = CurrentRoundStatus.Extra,
                    RichiSticks = CurrentRoundStatus.RichiSticks,
                    OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex,
                    Points = CurrentRoundStatus.Points.ToArray(),
                    InitialHandTiles = tiles,
                    MahjongSetData = MahjongSet.Data
                };
                var player = CurrentRoundStatus.GetPlayer(index);
                ClientBehaviour.Instance.photonView.RPC("RpcRoundStart", player, info);
            }
            firstTime = Time.time;
        }

        public override void OnStateUpdate()
        {
            if (responds.All(r => r) || Time.time - firstTime >= ServerConstants.ServerTimeOut)
            {
                ServerNextState();
                return;
            }
        }

        private void ServerNextState()
        {
            ServerBehaviour.Instance.DrawTile(CurrentRoundStatus.OyaPlayerIndex);
        }

        public override void OnServerStateExit()
        {
        }

        private void DrawInitial()
        {
            for (int round = 0; round < MahjongConstants.InitialDrawRound; round++)
            {
                // Draw 4 tiles for each player
                for (int index = 0; index < players.Count; index++)
                {
                    for (int i = 0; i < MahjongConstants.TilesEveryRound; i++)
                    {
                        var tile = MahjongSet.DrawTile();
                        CurrentRoundStatus.AddTile(index, tile);
                    }
                }
            }
            // Last round, 1 tile for each player
            for (int index = 0; index < players.Count; index++)
            {
                for (int i = 0; i < MahjongConstants.TilesLastRound; i++)
                {
                    var tile = MahjongSet.DrawTile();
                    CurrentRoundStatus.AddTile(index, tile);
                }
            }
        }
    }
}