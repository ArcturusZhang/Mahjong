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


namespace Multi
{
    /// <summary>
    /// This class only takes effect on server
    /// </summary>
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
            CurrentRoundStatus = new ServerRoundStatus(GameSettings, YakuSettings, LobbyManager.Instance.Players);
            mahjongSet = new MahjongSet(GameSettings, GameSettings.GetAllTiles(CurrentRoundStatus.TotalPlayers));
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
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
                CurrentRoundStatus = CurrentRoundStatus,
                MahjongSet = mahjongSet,
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
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet,
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(drawState);
        }

        public void DiscardTile(int playerIndex, Tile tile, bool isRichiing, bool discardLastDraw, int bonusTurnTime)
        {
            if (CurrentRoundStatus.CurrentPlayerIndex != playerIndex)
            {
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
            CurrentRoundStatus.SortHandTiles();
            var discardState = new PlayerDiscardTileState
            {
                CurrentPlayerIndex = playerIndex,
                DiscardTile = tile,
                IsRichiing = isRichiing,
                DiscardLastDraw = discardLastDraw,
                CurrentRoundStatus = CurrentRoundStatus,
                MahjongSetData = mahjongSet.Data
            };
            StateMachine.ChangeState(discardState);
        }

        // todo -- this method needs more info to work (operations other players take, etc)
        public void TurnEnd(int playerIndex, Tile discardingTile, bool isRichiing, OutTurnOperation[] operations)
        {
            var turnEndState = new TurnEndState
            {
                CurrentPlayerIndex = playerIndex,
                CurrentRoundStatus = CurrentRoundStatus,
                DiscardingTile = discardingTile,
                IsRichiing = isRichiing,
                Operations = operations,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(turnEndState);
        }

        public void PerformOutTurnOperation(int newPlayerIndex, OutTurnOperation operation)
        {
            var operationPerformState = new OperationPerformState
            {

            };
            StateMachine.ChangeState(operationPerformState);
        }

        public void Tsumo(int currentPlayerIndex, Tile winningTile, PointInfo pointInfo)
        {
            var tsumoState = new PlayerTsumoState
            {
                TsumoPlayerIndex = currentPlayerIndex,
                CurrentRoundStatus = CurrentRoundStatus,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                TsumoPointInfo = pointInfo
            };
            StateMachine.ChangeState(tsumoState);
        }

        public void Rong(int currentPlayerIndex, Tile winningTile, int[] rongPlayerIndices, PointInfo[] rongPointInfos)
        {
            var rongState = new PlayerRongState {
                CurrentPlayerIndex = currentPlayerIndex,
                CurrentRoundStatus = CurrentRoundStatus,
                RongPlayerIndices = rongPlayerIndices,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                RongPointInfos = rongPointInfos
            };
            StateMachine.ChangeState(rongState);
        }

        public void RoundDraw()
        {
            var drawState = new RoundDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(drawState);
        }

        public void Idle()
        {
            StateMachine.ChangeState(new IdleState());
        }
    }
}
