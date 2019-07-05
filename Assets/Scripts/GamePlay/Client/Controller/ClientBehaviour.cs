using System.Linq;
using Common.StateMachine;
using Common.StateMachine.Interfaces;
using GamePlay.Client.Controller.GameState;
using GamePlay.Client.Model;
using GamePlay.Server.Model;
using Mahjong.Model;
using Photon.Pun;
using Photon.Realtime;
using GamePlay.Server.Model.Events;
using UnityEngine;

namespace GamePlay.Client.Controller
{
    public class ClientBehaviour : MonoBehaviourPunCallbacks
    {
        public static ClientBehaviour Instance { get; private set; }
        private ClientRoundStatus CurrentRoundStatus;
        private ViewController controller;
        public IStateMachine StateMachine { get; private set; }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log("ClientBehaviour.OnEnable() is called");
            Instance = this;
            StateMachine = new StateMachine();
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                var player = PhotonNetwork.LocalPlayer;
                PhotonNetwork.RaiseEvent(
                    EventMessages.LoadCompleteEvent, player.ActorNumber,
                    EventMessages.ToMaster, EventMessages.SendReliable);
            }
            controller = ViewController.Instance;
        }

        [PunRPC]
        public void RpcGamePrepare(EventMessages.GamePrepareInfo info)
        {
            CurrentRoundStatus = new ClientRoundStatus(info.PlayerIndex, info.GameSetting);
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                Points = info.Points,
                Names = info.PlayerNames
            };
            StateMachine.ChangeState(prepareState);
        }

        [PunRPC]
        public void RpcRoundStart(EventMessages.RoundStartInfo info)
        {
            var startState = new RoundStartState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                LocalPlayerHandTiles = info.InitialHandTiles,
                OyaPlayerIndex = info.OyaPlayerIndex,
                Dice = info.Dice,
                Field = info.Field,
                Extra = info.Extra,
                RichiSticks = info.RichiSticks,
                MahjongSetData = info.MahjongSetData,
                Points = info.Points
            };
            StateMachine.ChangeState(startState);
        }

        [PunRPC]
        public void RpcDrawTile(EventMessages.DrawTileInfo info)
        {
            var drawState = new PlayerDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = info.DrawPlayerIndex,
                Tile = info.Tile,
                BonusTurnTime = info.BonusTurnTime,
                Zhenting = info.Zhenting,
                MahjongSetData = info.MahjongSetData,
                Operations = info.Operations
            };
            StateMachine.ChangeState(drawState);
        }

        [PunRPC]
        public void RpcKong(EventMessages.KongInfo message)
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

        [PunRPC]
        public void RpcBeiDora(EventMessages.BeiDoraInfo message)
        {
            var beiDoraState = new PlayerBeiDoraState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                BeiDoraPlayerIndex = message.BeiDoraPlayerIndex,
                BeiDoras = message.BeiDoras,
                HandData = message.HandData,
                BonusTurnTime = message.BonusTurnTime,
                Operations = message.Operations,
                MahjongSetData = message.MahjongSetData
            };
            StateMachine.ChangeState(beiDoraState);
        }

        [PunRPC]
        public void RpcDiscardOperation(EventMessages.DiscardOperationInfo info)
        {
            var discardOperationState = new PlayerDiscardOperationState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = info.CurrentTurnPlayerIndex,
                IsRichiing = info.IsRichiing,
                DiscardingLastDraw = info.DiscardingLastDraw,
                Tile = info.Tile,
                BonusTurnTime = info.BonusTurnTime,
                Zhenting = info.Zhenting,
                Operations = info.Operations,
                HandTiles = info.HandTiles,
                Rivers = info.Rivers
            };
            StateMachine.ChangeState(discardOperationState);
        }

        [PunRPC]
        public void RpcTurnEnd(EventMessages.TurnEndInfo info)
        {
            var turnEndState = new PlayerTurnEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = info.PlayerIndex,
                ChosenOperationType = info.ChosenOperationType,
                Operations = info.Operations,
                Points = info.Points,
                RichiStatus = info.RichiStatus,
                RichiSticks = info.RichiSticks,
                Zhenting = info.Zhenting,
                MahjongSetData = info.MahjongSetData
            };
            StateMachine.ChangeState(turnEndState);
        }

        [PunRPC]
        public void RpcOperationPerform(EventMessages.OperationPerformInfo info)
        {
            var operationState = new PlayerOperationPerformState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerIndex = info.PlayerIndex,
                OperationPlayerIndex = info.OperationPlayerIndex,
                Operation = info.Operation,
                HandData = info.HandData,
                BonusTurnTime = info.BonusTurnTime,
                Rivers = info.Rivers,
                MahjongSetData = info.MahjongSetData
            };
            StateMachine.ChangeState(operationState);
        }

        [PunRPC]
        public void RpcTsumo(EventMessages.TsumoInfo info)
        {
            var tsumoState = new PlayerTsumoState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                TsumoPlayerIndex = info.TsumoPlayerIndex,
                TsumoPlayerName = info.TsumoPlayerName,
                TsumoHandData = info.TsumoHandData,
                WinningTile = info.WinningTile,
                DoraIndicators = info.DoraIndicators,
                UraDoraIndicators = info.UraDoraIndicators,
                IsRichi = info.IsRichi,
                TsumoPointInfo = info.TsumoPointInfo,
                TotalPoints = info.TotalPoints
            };
            StateMachine.ChangeState(tsumoState);
        }

        [PunRPC]
        public void RpcRong(EventMessages.RongInfo message)
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

        [PunRPC]
        public void RpcRoundDraw(EventMessages.RoundDrawInfo info)
        {
            var roundDrawState = new RoundDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                RoundDrawType = info.RoundDrawType,
                WaitingData = info.WaitingData
            };
            StateMachine.ChangeState(roundDrawState);
        }

        [PunRPC]
        public void RpcPointTransfer(EventMessages.PointTransferInfo message)
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

        [PunRPC]
        public void RpcGameEnd(EventMessages.GameEndInfo message) {
            var endState = new GameEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                PlayerNames = message.PlayerNames,
                Points = message.Points,
                Places = message.Places
            };
            StateMachine.ChangeState(endState);
        }

        public void ClientReady()
        {
            PhotonNetwork.RaiseEvent(
                EventMessages.ClientReadyEvent, CurrentRoundStatus.LocalPlayerIndex,
                EventMessages.ToMaster, EventMessages.SendReliable);
        }

        public void NextRound()
        {
            PhotonNetwork.RaiseEvent(
                EventMessages.NextRoundEvent, CurrentRoundStatus.LocalPlayerIndex,
                EventMessages.ToMaster, EventMessages.SendReliable);
        }

        public void OnDiscardTile(Tile tile, bool isLastDraw)
        {
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            OnDiscardTile(tile, isLastDraw, bonusTimeLeft);
        }

        public void OnDiscardTile(Tile tile, bool isLastDraw, int bonusTimeLeft)
        {
            Debug.Log($"Sending request of discarding tile {tile}");
            var info = new EventMessages.DiscardTileInfo
            {
                PlayerIndex = CurrentRoundStatus.LocalPlayerIndex,
                IsRichiing = CurrentRoundStatus.IsRichiing,
                DiscardingLastDraw = isLastDraw,
                Tile = tile,
                BonusTurnTime = bonusTimeLeft
            };
            PhotonNetwork.RaiseEvent(
                EventMessages.DiscardTileEvent, info,
                EventMessages.ToMaster, EventMessages.SendReliable);
            var localDiscardState = new LocalDiscardState {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = CurrentRoundStatus.LocalPlayerIndex,
                IsRichiing = CurrentRoundStatus.IsRichiing,
                DiscardingLastDraw = isLastDraw,
                Tile = tile
            };
            StateMachine.ChangeState(localDiscardState);
        }

        private void OnInTurnOperationTaken(InTurnOperation operation, int bonusTurnTime)
        {
            var info = new EventMessages.InTurnOperationInfo
            {
                PlayerIndex = CurrentRoundStatus.LocalPlayerIndex,
                Operation = operation,
                BonusTurnTime = bonusTurnTime
            };
            PhotonNetwork.RaiseEvent(
                EventMessages.InTurnOperationEvent, info,
                EventMessages.ToMaster, EventMessages.SendReliable);
        }

        public void OnSkipOutTurnOperation(int bonusTurnTime)
        {
            OnOutTurnOperationTaken(new OutTurnOperation { Type = OutTurnOperationType.Skip }, bonusTurnTime);
        }

        public void OnOutTurnOperationTaken(OutTurnOperation operation, int bonusTurnTime)
        {
            var info = new EventMessages.OutTurnOperationInfo
            {
                PlayerIndex = CurrentRoundStatus.LocalPlayerIndex,
                Operation = operation,
                BonusTurnTime = bonusTurnTime
            };
            PhotonNetwork.RaiseEvent(
                EventMessages.OutTurnOperationEvent, info,
                EventMessages.ToMaster, EventMessages.SendReliable);
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
            OnInTurnOperationTaken(operation, bonusTimeLeft);
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
                OnInTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                return;
            }
            // show kong selection panel here
            controller.InTurnPanelManager.ShowBackButton();
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                OnInTurnOperationTaken(new InTurnOperation
                {
                    Type = InTurnOperationType.Kong,
                    Meld = meld
                }, bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                controller.MeldSelectionManager.Close();
            });
        }

        public void OnInTurnButtonClicked(InTurnOperation operation)
        {
            Debug.Log($"Requesting to proceed operation: {operation}");
            int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
            OnInTurnOperationTaken(operation, bonusTimeLeft);
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
            OnOutTurnOperationTaken(operation, bonusTimeLeft);
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
                OnOutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.OutTurnPanelManager.Close();
                return;
            }
            // chow selection logic here
            controller.OutTurnPanelManager.ShowBackButton();
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                OnOutTurnOperationTaken(new OutTurnOperation
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
                OnOutTurnOperationTaken(operationOptions[0], bonusTimeLeft);
                controller.OutTurnPanelManager.Close();
                return;
            }
            // pong selection logic here
            controller.OutTurnPanelManager.ShowBackButton();
            var meldOptions = operationOptions.Select(op => op.Meld);
            controller.MeldSelectionManager.SetMeldOptions(meldOptions.ToArray(), meld =>
            {
                int bonusTimeLeft = controller.TurnTimeController.StopCountDown();
                Debug.Log($"Sending request of in turn kong operation with bonus turn time {bonusTimeLeft}");
                OnOutTurnOperationTaken(new OutTurnOperation
                {
                    Type = OutTurnOperationType.Pong,
                    Meld = meld
                }, bonusTimeLeft);
                controller.InTurnPanelManager.Close();
                controller.MeldSelectionManager.Close();
            });
        }

        public override void OnLeftRoom()
        {
            // todo
        }

        public override void OnLeftLobby()
        {
            // todo
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            // todo
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            // todo
        }
    }
}