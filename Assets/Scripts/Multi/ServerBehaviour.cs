using System.Collections;
using System.Collections.Generic;
using Lobby;
using Multi.GameState;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;
using Debug = Single.Debug;

namespace Multi
{
    // This class only takes effect on server
    public class ServerBehaviour : NetworkBehaviour
    {
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;
        public IStateMachine StateMachine { get; private set; }
        private MahjongSet mahjongSet;
        private ServerRoundStatus CurrentRoundStatus = null;
        public static ServerBehaviour Instance { get; private set; }

        private void OnEnable()
        {
            Debug.Log("[Server] ServerBehaviour.OnEnable() is called");
            Instance = this;
            StateMachine = new StateMachine.StateMachine();
        }

        public override void OnStartClient()
        {
            Debug.Log("[Server] OnStartClient is called");
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log("[Server] OnStartLocalPlayer");
        }

        public override void OnStartServer()
        {
            Debug.Log("[Server] OnStartServer");
            GameSettings = LobbyManager.Instance.GameSettings;
            YakuSettings = LobbyManager.Instance.YakuSettings;
            mahjongSet = new MahjongSet(GameSettings);
            var waitingState = new WaitForLoadingState
            {
                TotalPlayers = LobbyManager.Instance._playerNumber,
                TimeOut = ServerConstants.ServerWaitForLoadingTimeOut
            };
            StateMachine.ChangeState(waitingState);
        }

        private void Update()
        {
            StateMachine.UpdateState();
        }

        public void GamePrepare()
        {
            CurrentRoundStatus = new ServerRoundStatus(LobbyManager.Instance.Players);
            var prepareState = new GamePrepareState
            {
                GameSettings = GameSettings,
                YakuSettings = YakuSettings,
                CurrentRoundStatus = CurrentRoundStatus,
                Players = LobbyManager.Instance.Players
            };
            StateMachine.ChangeState(prepareState);
        }

        public void GameAbort()
        {
            // todo -- implement abort logic here: at least one of the players cannot load into game, back to lobby scene
            StateMachine.ChangeState(new IdleState());
        }

        public void RoundStart(bool next = true, bool extra = false, bool keepSticks = false)
        {
            var startState = new RoundStartState
            {
                GameSettings = GameSettings,
                Players = LobbyManager.Instance.Players,
                MahjongSet = mahjongSet,
                CurrentRoundStatus = CurrentRoundStatus,
                NextRound = next,
                ExtraRound = extra,
                KeepSticks = keepSticks
            };
            StateMachine.ChangeState(startState);
        }

        public void DrawTile(int playerIndex)
        {
            var drawState = new PlayerDrawTileState
            {
                GameSettings = GameSettings,
                CurrentPlayerIndex = playerIndex,
                Players = LobbyManager.Instance.Players,
                MahjongSet = mahjongSet,
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(drawState);
        }

        public void DiscardTile(int playerIndex, Tile tile, bool isRichiing, bool discardLastDraw, int bonusTurnTime)
        {
            if (CurrentRoundStatus.CurrentPlayerIndex != playerIndex) {
                Debug.LogError("PlayerIndex does not match, this should not happen!");
            }
            if (CurrentRoundStatus.LastDraw == null)
            {
                Debug.LogError("LastDraw is null, this should not happen!");
            }
            var currentPlayer = LobbyManager.Instance.Players[playerIndex];
            currentPlayer.BonusTurnTime = bonusTurnTime;
            var lastDraw = (Tile)CurrentRoundStatus.LastDraw;
            CurrentRoundStatus.LastDraw = null;
            if (!discardLastDraw)
            {
                CurrentRoundStatus.RemoveTile(playerIndex, tile);
                CurrentRoundStatus.AddTile(playerIndex, lastDraw);
            }
            CurrentRoundStatus.AddToRiver(playerIndex, tile, isRichiing);
            var discardState = new PlayerDiscardTileState
            {
                GameSettings = GameSettings,
                CurrentPlayerIndex = playerIndex,
                Players = LobbyManager.Instance.Players,
                DiscardTile = tile,
                IsRichiing = isRichiing,
                DiscardLastDraw = discardLastDraw,
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(discardState);
        }

        // todo -- this method needs more info to work (operations other players take, etc)
        public void TurnEnd(int playerIndex, bool isRichiing, OutTurnOperation[] operations)
        {
            var turnEndState = new TurnEndState {
                GameSettings = GameSettings,
                CurrentPlayerIndex = playerIndex,
                Players = LobbyManager.Instance.Players,
                IsRichiing = isRichiing,
                Operations = operations,
                CurrentRoundStatus = CurrentRoundStatus,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(turnEndState);
        }

        public void PerformOperation(int playerIndex, OutTurnOperation operation) {
            var operationPerformState = new OperationPerformState {

            };
            StateMachine.ChangeState(operationPerformState);
        }

        public void RoundDraw() {
            StateMachine.ChangeState(new IdleState());
        }

        public void Idle()
        {
            StateMachine.ChangeState(new IdleState());
        }
    }
}
