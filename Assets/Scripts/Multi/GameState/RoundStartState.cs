using System.Collections.Generic;
using System.Linq;
using Multi.Messages;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class RoundStartState : AbstractMahjongState
    {
        public bool NewRound;
        public GameSettings GameSettings;
        public GameStatus GameStatus;
        public MahjongSetManager MahjongSetManager;
        public UnityAction ServerCallback;
        private List<Player> players;
        private bool[] responseReceived;

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            NetworkServer.RegisterHandler(MessageConstants.ReadinessMessageId, OnReadinessMessageReceived);
            GameStatus.RoundStatus = GameStatus.RoundStatus.NextRound(NewRound);
            players = GameStatus.Players;
            // Throwing dice
            GameStatus.Dice = Random.Range(GameSettings.DiceMin, GameSettings.DiceMax + 1);
            int openIndex = MahjongSetManager.Open(GameStatus.Dice);
            Debug.Log($"[RoundStartState] Dice rolls {GameStatus.Dice}");
            // Draw tiles in turn
            int count = DrawInitialTiles();
            GameStatus.RoundStatus = GameStatus.RoundStatus.RemoveTiles(count);
            // Update data in players
            foreach (var player in players)
            {
                player.BonusTurnTime = GameSettings.BonusTurnTime;
                player.HandTilesCount = MahjongConstants.CompleteHandTilesCount;
                player.Richi = false;
                player.WRichi = false;
                player.FirstTurn = true;
                player.RoundStatus = GameStatus.RoundStatus;
            }
            var doraTiles = MahjongSetManager.DoraIndicators.ToArray();
            var doraIndices = MahjongSetManager.DoraIndicatorIndices.ToArray();
            // Sending initial tiles message
            for (int i = 0; i < players.Count; i++)
            {
                players[i].connectionToClient.Send(MessageConstants.InitialDrawingMessageId, new InitialDrawingMessage
                {
                    Dice = GameStatus.Dice,
                    TotalPlayers = players.Count,
                    MountainOpenIndex = openIndex,
                    Tiles = GameStatus.PlayerHandTiles[i].ToArray(),
                    DoraIndicators = doraTiles,
                    DoraIndicatorIndices = doraIndices
                });
                GameStatus.PlayerHandTiles[i].Sort();
            }

            // wait for all the client has done drawing initial tiles
            responseReceived = new bool[players.Count];
        }

        private int DrawInitialTiles()
        {
            int count = 0;
            for (int turn = 0; turn < players.Count; turn++)
            {
                var current = CurrentPlayerIndex(turn);
                GameStatus.PlayerHandTiles[current] = new List<Tile>();
                GameStatus.PlayerOpenMelds[current] = new List<Meld>();
            }

            // draw tiles
            for (int round = 0; round < GameSettings.InitialDrawRound; round++)
            for (int turn = 0; turn < players.Count; turn++)
            {
                var tiles = MahjongSetManager.DrawTiles(GameSettings.TilesEveryRound);
                count += tiles.Count;
                var current = CurrentPlayerIndex(turn);
                GameStatus.PlayerHandTiles[current].AddRange(tiles);
            }

            for (int turn = 0; turn < players.Count; turn++)
            {
                var tile = MahjongSetManager.DrawTile();
                count++;
                var current = CurrentPlayerIndex(turn);
                GameStatus.PlayerHandTiles[current].Add(tile);
            }

            Assert.AreEqual(
                MahjongConstants.RepeatIndex(MahjongSetManager.NextIndex - MahjongSetManager.OpenIndex,
                    MahjongConstants.TotalTilesCount), count,
                $"MahjongSetManager {MahjongSetManager.NextIndex - MahjongSetManager.OpenIndex}, count {count}");

            return count;
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ReadinessMessage>();
            Debug.Log($"[Server] Player {content.PlayerIndex} is ready.");
            responseReceived[content.PlayerIndex] = true;
            if (!responseReceived.All(received => received)) return;
            Debug.Log($"[Server] All players have done their initial drawing, entering next state");
            ServerCallback.Invoke();
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
            NetworkServer.UnregisterHandler(MessageConstants.ReadinessMessageId);
            GameStatus.SetCurrentPlayerIndex(GameStatus.RoundStatus.RoundCount - 1);
        }

        private int CurrentPlayerIndex(int turn)
        {
            return MahjongConstants.RepeatIndex(GameStatus.RoundStatus.RoundCount - 1 + turn, players.Count);
        }
    }
}