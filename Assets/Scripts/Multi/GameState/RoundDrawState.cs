using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Multi.GameState
{
    public class RoundDrawState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        public RoundDrawType RoundDrawType;
        private ServerRoundDrawMessage[] messages;
        private GameSettings gameSettings;
        private IList<Player> players;
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            players = CurrentRoundStatus.Players;
            messages = new ServerRoundDrawMessage[players.Count];
            switch (RoundDrawType)
            {
                case RoundDrawType.RoundDraw:
                    HandleRoundDraw();
                    break;
                case RoundDrawType.NineOrphans:
                    HandleNineOrphans();
                    break;
                default:
                    throw new System.Exception($"Type {RoundDrawType} not implemented");
            }
            for (int i = 0; i < players.Count; i++)
                players[i].connectionToClient.Send(MessageIds.ServerRoundDrawMessage, messages[i]);
        }

        private void HandleNineOrphans() {
            for (int i = 0; i < players.Count;i++) {
                messages[i] = new ServerRoundDrawMessage {
                    RoundDrawType = RoundDrawType.NineOrphans
                };
            }
        }

        private void HandleRoundDraw()
        {
            // Get waiting tiles for each player
            var waitingTiles = new WaitingData[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                var hand = CurrentRoundStatus.HandTiles(i);
                var open = CurrentRoundStatus.OpenMelds(i);
                waitingTiles[i] = new WaitingData
                {
                    HandTiles = hand,
                    WaitingTiles = MahjongLogic.WinningTiles(hand, open).ToArray()
                };
            }
            // Get messages and send
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerRoundDrawMessage
                {
                    RoundDrawType = RoundDrawType,
                    WaitingData = waitingTiles
                };
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
            // Debug.Log($"Server is in {GetType().Name}");
        }
    }
}