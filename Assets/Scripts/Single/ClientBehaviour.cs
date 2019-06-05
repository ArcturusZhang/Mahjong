using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Multi;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single.GameState;
using Single.MahjongDataType;
using Single.Managers;
using Single.UI;
using Single.UI.Controller;
using StateMachine.Interfaces;
using UnityEngine;


namespace Single
{
    public class ClientBehaviour : MonoBehaviour
    {
        public static ClientBehaviour Instance { get; private set; }
        private ClientRoundStatus CurrentRoundStatus;
        private ViewController controller;
        private Player localPlayer => CurrentRoundStatus.LocalPlayer;
        public IStateMachine StateMachine { get; private set; }

        private void OnEnable()
        {
            Debug.Log("ClientBehaviour.OnEnable() is called");
            Instance = this;
            StateMachine = new StateMachine.StateMachine();
        }

        private void Start()
        {
            controller = ViewController.Instance;
        }

        /// <summary>
        /// This method make the game preparation information takes effect 
        /// on the client. In this method, client sets the proper place to the 
        /// corresponding player, and gathering necessary information.
        /// </summary>
        /// <param name="message">The message received from server</param>
        public void GamePrepare(ServerGamePrepareMessage message)
        {
            CurrentRoundStatus = new ClientRoundStatus(
                message.TotalPlayers, message.PlayerIndex, message.GameSetting, message.YakuSetting);
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                Points = message.Points,
                Names = message.PlayerNames
            };
            StateMachine.ChangeState(prepareState);
        }

        /// <summary>
        /// This method is invoked when the client received a RoundStartMessage 
        /// when every round starts. In this method, client set initial hand tiles
        /// of the local player, and hand tiles count for every remote players.
        /// Dice is thrown on the server, as client receives the dice value then 
        /// applies it to the game. Same as other data (Extra, RichiSticks, etc).
        /// </summary>
        /// <param name="message">The message received from server</param>
        public void StartRound(ServerRoundStartMessage message)
        {
            var startState = new RoundStartState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                LocalPlayerHandTiles = message.InitialHandTiles,
                OyaPlayerIndex = message.OyaPlayerIndex,
                Dice = message.Dice,
                Field = message.Field,
                Extra = message.Extra,
                RichiSticks = message.RichiSticks,
                MahjongSetData = message.MahjongSetData,
                Points = message.Points
            };
            StateMachine.ChangeState(startState);
        }

        public void PlayerDrawTurn(ServerDrawTileMessage message)
        {
            var drawState = new PlayerDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = message.DrawPlayerIndex,
                Tile = message.Tile,
                BonusTurnTime = message.BonusTurnTime,
                Richied = message.Richied,
                Zhenting = message.Zhenting,
                MahjongSetData = message.MahjongSetData,
                Operations = message.Operations
            };
            StateMachine.ChangeState(drawState);
        }

        public void PlayerKong(ServerKongMessage message)
        {
            var kongState = new PlayerKongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                KongPlayerIndex = message.KongPlayerIndex,
                HandData = message.HandData,
                BonusTurnTime = message.BonusTurnTime,
                Operations = message.Operations,
                MahjongSetData = message.MahjongSetData
            };
            StateMachine.ChangeState(kongState);
        }

        public void PlayerDiscardOperation(ServerDiscardOperationMessage message)
        {
            var discardOperationState = new PlayerDiscardOperationState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = message.CurrentTurnPlayerIndex,
                IsRichiing = message.IsRichiing,
                DiscardingLastDraw = message.DiscardingLastDraw,
                Tile = message.Tile,
                BonusTurnTime = message.BonusTurnTime,
                Zhenting = message.Zhenting,
                Operations = message.Operations,
                HandTiles = message.HandTiles,
                Rivers = message.Rivers
            };
            StateMachine.ChangeState(discardOperationState);
        }

        public void PlayerTurnEnd(ServerTurnEndMessage message)
        {
            var turnEndState = new PlayerTurnEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = message.PlayerIndex,
                ChosenOperationType = message.ChosenOperationType,
                Operations = message.Operations,
                Points = message.Points,
                RichiStatus = message.RichiStatus,
                RichiSticks = message.RichiSticks,
                Zhenting = message.Zhenting,
                MahjongSetData = message.MahjongSetData
            };
            StateMachine.ChangeState(turnEndState);
        }

        public void OperationPerform(ServerOperationPerformMessage message)
        {
            var operationState = new PlayerOperationPerformState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = message.PlayerIndex,
                OperationPlayerIndex = message.OperationPlayerIndex,
                Operation = message.Operation,
                HandData = message.HandData,
                BonusTurnTime = message.BonusTurnTime,
                Rivers = message.Rivers,
                MahjongSetData = message.MahjongSetData
            };
            StateMachine.ChangeState(operationState);
        }

        public void PlayerTsumo(ServerPlayerTsumoMessage message)
        {
            var tsumoState = new PlayerTsumoState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                TsumoPlayerIndex = message.TsumoPlayerIndex,
                TsumoPlayerName = message.TsumoPlayerName,
                TsumoHandData = message.TsumoHandData,
                WinningTile = message.WinningTile,
                DoraIndicators = message.DoraIndicators,
                UraDoraIndicators = message.UraDoraIndicators,
                IsRichi = message.IsRichi,
                TsumoPointInfo = message.TsumoPointInfo,
                TotalPoints = message.TotalPoints
            };
            StateMachine.ChangeState(tsumoState);
        }

        public void PlayerRong(ServerPlayerRongMessage message)
        {
            var rongState = new PlayerRongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                RongPlayerIndices = message.RongPlayerIndices,
                RongPlayerNames = message.RongPlayerNames,
                HandData = message.HandData,
                WinningTile = message.WinningTile,
                DoraIndicators = message.DoraIndicators,
                UraDoraIndicators = message.UraDoraIndicators,
                RongPlayerRichiStatus = message.RongPlayerRichiStatus,
                RongPointInfos = message.RongPointInfos,
                TotalPoints = message.TotalPoints
            };
            StateMachine.ChangeState(rongState);
        }

        public void RoundDraw(ServerRoundDrawMessage message)
        {
            var roundDrawState = new RoundDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                RoundDrawType = message.RoundDrawType,
                WaitingData = message.WaitingData
            };
            StateMachine.ChangeState(roundDrawState);
        }

        public void PointTransfer(ServerPointTransferMessage message)
        {
            var transferState = new PointTransferState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerNames = message.PlayerNames,
                Points = message.Points,
                PointTransfers = message.PointTransfers
            };
            StateMachine.ChangeState(transferState);
        }

        public void GameEnd(ServerGameEndMessage message)
        {
            var endState = new GameEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerNames = message.PlayerNames,
                Points = message.Points,
                Places = message.Places
            };
            StateMachine.ChangeState(endState);
        }

        public void OnDiscardTile(Tile tile, bool isLastDraw)
        {
            Debug.Log($"Sending request of discarding tile {tile}");
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            localPlayer.DiscardTile(tile, CurrentRoundStatus.IsRichiing, isLastDraw, bonusTimeLeft);
            controller.InTurnPanelManager.Close();
            CurrentRoundStatus.SetRichiing(false);
            controller.HandPanelManager.RemoveCandidates();
        }

        public void OnInTurnSkipButtonClicked()
        {
            Debug.Log("In turn skip button clicked, hide buttons");
            controller.InTurnPanelManager.Close();
        }

        public void OnTsumoButtonClicked(InTurnOperation operation)
        {
            if (operation.Type != InTurnOperationType.Tsumo)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnTsumoButtonClicked method");
                return;
            }
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of tsumo operation with bonus turn time {bonusTimeLeft}");
            localPlayer.InTurnOperationTaken(operation, bonusTimeLeft);
            controller.InTurnPanelManager.Close();
        }

        public void OnRichiButtonClicked(InTurnOperation operation)
        {
            if (operation.Type != InTurnOperationType.Richi)
            {
                Debug.LogError($"Cannot send a operation with type {operation.Type} within OnRichiButtonClicked method");
                return;
            }
            // show richi selection panel
            Debug.Log($"Showing richi selection panel, candidates: {string.Join(",", operation.RichiAvailableTiles)}");
            CurrentRoundStatus.SetRichiing(true);
            controller.HandPanelManager.SetCandidates(operation.RichiAvailableTiles);
        }

        public void OnInTurnKongButtonClicked(InTurnOperation[] operationOptions)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnInTurnKongButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == InTurnOperationType.Kong))
            {
                Debug.LogError("There are incompatible type within OnInTurnKongButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                localPlayer.InTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                return;
            }
            // show kong selection panel here
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                localPlayer.InTurnOperationTaken(new InTurnOperation
                {
                    Type = InTurnOperationType.Kong,
                    Meld = meld
                }, bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                controller.MeldSelectionManager.Close();
            });
        }

        public void OnInTurnDrawButtonClicked(InTurnOperation operation)
        {
            Debug.Log($"Requesting round draw due to 9 kinds of orphans");
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            localPlayer.InTurnOperationTaken(operation, bonusTimeLeft);
            controller.InTurnPanelManager.Close();
        }

        public void OnInTurnBackButtonClicked(InTurnOperation[] operations)
        {
            controller.InTurnPanelManager.SetOperations(operations);
            controller.MeldSelectionManager.Close();
            controller.HandPanelManager.RemoveCandidates();
            CurrentRoundStatus.SetRichiing(false);
        }

        public void OnOutTurnBackButtonClicked(OutTurnOperation[] operations)
        {
            controller.OutTurnPanelManager.SetOperations(operations);
            controller.MeldSelectionManager.Close();
        }

        public void OnOutTurnButtonClicked(OutTurnOperation operation)
        {
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            Debug.Log($"Sending request of operation {operation} with bonus turn time {bonusTimeLeft}");
            localPlayer.OutTurnOperationTaken(operation, bonusTimeLeft);
            controller.OutTurnPanelManager.Close();
        }

        public void OnChowButtonClicked(OutTurnOperation[] operationOptions, OutTurnOperation[] originalOperations)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnChowButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == OutTurnOperationType.Chow))
            {
                Debug.LogError("There are incompatible type within OnChowButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of chow operation with bonus turn time {bonusTimeLeft}");
                localPlayer.OutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.OutTurnPanelManager.Close();
                return;
            }
            // chow selection logic here
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                localPlayer.OutTurnOperationTaken(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Chow,
                    Meld = meld
                }, bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                controller.MeldSelectionManager.Close();
            });
        }

        public void OnPongButtonClicked(OutTurnOperation[] operationOptions, OutTurnOperation[] originalOperations)
        {
            if (operationOptions == null || operationOptions.Length == 0)
            {
                Debug.LogError("The operations are null or empty in OnPongButtonClicked method, this should not happen.");
                return;
            }
            if (!operationOptions.All(op => op.Type == OutTurnOperationType.Pong))
            {
                Debug.LogError("There are incompatible type within OnPongButtonClicked method");
                return;
            }
            if (operationOptions.Length == 1)
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of kong operation with bonus turn time {bonusTimeLeft}");
                localPlayer.OutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.OutTurnPanelManager.Close();
                return;
            }
            // pong selection logic here
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                localPlayer.OutTurnOperationTaken(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Pong,
                    Meld = meld
                }, bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                controller.MeldSelectionManager.Close();
            });
        }
    }
}