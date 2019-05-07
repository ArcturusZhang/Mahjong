using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace Multi.GameState
{
    public class RoundDrawState : IState
    {
        public ServerRoundStatus CurrentRoundStatus;
        public RoundDrawType RoundDrawType;
        private MessageBase[] messages;
        private GameSettings gameSettings;
        private IList<Player> players;
        private List<PointTransfer> transfers;
        private float firstTime;
        private bool next;
        private bool extra;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            gameSettings = CurrentRoundStatus.GameSettings;
            players = CurrentRoundStatus.Players;
            messages = new ServerRoundDrawMessage[players.Count];
            transfers = new List<PointTransfer>();
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
            // Send messages
            for (int i = 0; i < players.Count; i++)
                players[i].connectionToClient.Send(MessageIds.ServerRoundDrawMessage, messages[i]);
            firstTime = Time.time;
        }

        private void HandleNineOrphans()
        {
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerRoundDrawMessage
                {
                    RoundDrawType = RoundDrawType.NineOrphans
                };
            }
            next = false;
            extra = true;
        }

        private void HandleRoundDraw()
        {
            // Get waiting tiles for each player
            var waitingDataArray = new WaitingData[players.Count];
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                var hand = CurrentRoundStatus.HandTiles(playerIndex);
                var open = CurrentRoundStatus.Melds(playerIndex);
                waitingDataArray[playerIndex] = new WaitingData
                {
                    HandTiles = hand,
                    WaitingTiles = MahjongLogic.WinningTiles(hand, open).ToArray()
                };
            }
            // Get messages
            for (int i = 0; i < players.Count; i++)
            {
                messages[i] = new ServerRoundDrawMessage
                {
                    RoundDrawType = RoundDrawType,
                    WaitingData = waitingDataArray
                };
            }
            // Get point transfers
            // get player indices of those are ready and not
            var readyIndices = new List<int>();
            var notReadyIndices = new List<int>();
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                if (waitingDataArray[playerIndex].WaitingTiles.Length > 0)
                    readyIndices.Add(playerIndex);
                else
                    notReadyIndices.Add(playerIndex);
            }
            next = notReadyIndices.Contains(CurrentRoundStatus.OyaPlayerIndex);
            extra = true;
            // no one is ready or every one is ready
            if (readyIndices.Count == 0 || notReadyIndices.Count == 0) return;
            // get transfers according to total count of players
            switch (players.Count)
            {
                case 2:
                    GetTransfersFor2(readyIndices, notReadyIndices, transfers);
                    break;
                case 3:
                    GetTransfersFor3(readyIndices, notReadyIndices, transfers);
                    break;
                case 4:
                    GetTransfersFor4(readyIndices, notReadyIndices, transfers);
                    break;
                default:
                    Debug.LogError("This should not happen");
                    break;
            }
        }

        private void GetTransfersFor2(List<int> readyIndices, List<int> notReadyIndices, List<PointTransfer> transfers)
        {
            Assert.IsTrue(readyIndices.Count == 1 && notReadyIndices.Count == 1);
            int amount = CurrentRoundStatus.GameSettings.NotReadyPunishPerPlayer;
            transfers.Add(new PointTransfer
            {
                From = notReadyIndices[0],
                To = readyIndices[0],
                Amount = amount
            });
        }

        private void GetTransfersFor3(List<int> readyIndices, List<int> notReadyIndices, List<PointTransfer> transfers)
        {
            Assert.IsTrue(readyIndices.Count + notReadyIndices.Count == 3 && readyIndices.Count != 0 && readyIndices.Count != 0,
                $"ready: {readyIndices.Count}, not ready: {notReadyIndices.Count}");
            int amount = CurrentRoundStatus.GameSettings.NotReadyPunishPerPlayer;
            switch (readyIndices.Count)
            {
                case 1:
                    for (int i = 0; i < notReadyIndices.Count; i++)
                    {
                        transfers.Add(new PointTransfer
                        {
                            From = notReadyIndices[i],
                            To = readyIndices[0],
                            Amount = amount
                        });
                    }
                    break;
                case 2:
                    for (int i = 0; i < readyIndices.Count; i++)
                    {
                        transfers.Add(new PointTransfer
                        {
                            From = notReadyIndices[0],
                            To = readyIndices[i],
                            Amount = amount
                        });
                    }
                    break;
                default:
                    Debug.LogError("This should not happen");
                    break;
            }
        }

        private void GetTransfersFor4(List<int> readyIndices, List<int> notReadyIndices, List<PointTransfer> transfers)
        {
            Assert.IsTrue(readyIndices.Count + notReadyIndices.Count == 4 && readyIndices.Count != 0 && readyIndices.Count != 0,
                $"ready: {readyIndices.Count}, not ready: {notReadyIndices.Count}");
            int totalAmount = CurrentRoundStatus.GameSettings.NotReadyPunishPerPlayer * (players.Count - 1);
            switch (readyIndices.Count)
            {
                case 1:
                    for (int i = 0; i < notReadyIndices.Count; i++)
                    {
                        transfers.Add(new PointTransfer
                        {
                            From = notReadyIndices[i],
                            To = readyIndices[0],
                            Amount = totalAmount / 3
                        });
                    }
                    break;
                case 2:
                    for (int i = 0; i < readyIndices.Count; i++)
                    {
                        transfers.Add(new PointTransfer
                        {
                            From = notReadyIndices[i],
                            To = readyIndices[i],
                            Amount = totalAmount / 2
                        });
                    }
                    break;
                case 3:
                    for (int i = 0; i < readyIndices.Count; i++)
                    {
                        transfers.Add(new PointTransfer
                        {
                            From = notReadyIndices[0],
                            To = readyIndices[i],
                            Amount = totalAmount / 3
                        });
                    }
                    break;
                default:
                    Debug.LogError("This should not happen");
                    break;
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
            if (Time.time - firstTime > ServerConstants.ServerRoundDrawTimeOut)
            {
                ServerBehaviour.Instance.PointTransfer(transfers, next, extra, true);
                return;
            }
        }
    }
}