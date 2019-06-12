using System.Linq;
using GamePlay.Server.Model;
using GamePlay.Server.Model.Messages;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.Networking;


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
        private float firstSendTime;
        public override void OnServerStateEnter()
        {
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            MahjongSet.Reset();
            // throwing dice
            var dice = Random.Range(CurrentRoundStatus.GameSettings.DiceMin, CurrentRoundStatus.GameSettings.DiceMax + 1);
            CurrentRoundStatus.NextRound(dice, NextRound, ExtraRound, KeepSticks);
            // draw initial tiles
            DrawInitial();
            Debug.Log("[Server] Initial tiles distribution done");
            CurrentRoundStatus.SortHandTiles();
            responds = new bool[players.Count];
            for (int index = 0; index < players.Count; index++)
            {
                var tiles = CurrentRoundStatus.HandTiles(index);
                Debug.Log($"[Server] Hand tiles of player {index}: {string.Join("", tiles)}");
                var message = new ServerRoundStartMessage
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
                players[index].connectionToClient.Send(MessageIds.ServerRoundStartMessage, message);
                players[index].BonusTurnTime = CurrentRoundStatus.GameSettings.BonusTurnTime;
            }
            firstSendTime = Time.time;
        }

        public override void OnStateUpdate()
        {
            if (responds.All(r => r) || Time.time - firstSendTime >= ServerConstants.ServerTimeOut)
            {
                ServerNextState();
                return;
            }
        }

        private void ServerNextState()
        {
            ServerBehaviour.Instance.DrawTile(CurrentRoundStatus.OyaPlayerIndex);
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerRoundStartMessage)
            {
                Debug.LogError("Something is wrong, the received readiness meassage contains invalid content");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        public override void OnServerStateExit()
        {
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
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