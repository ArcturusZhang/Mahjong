using System.Collections.Generic;
using Common.StateMachine;
using Common.StateMachine.Interfaces;
using GamePlay.Server.Controller.GameState;
using GamePlay.Server.Model;
using Mahjong.Model;
using Photon.Pun;
using PUNLobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace GamePlay.Server.Controller
{
    /// <summary>
    /// This class only takes effect on server
    /// </summary>
    public class ServerBehaviour : MonoBehaviourPun
    {
        public SceneField lobbyScene;
        [HideInInspector] public GameSetting GameSettings;
        public IStateMachine StateMachine { get; private set; }
        private MahjongSet mahjongSet;
        public ServerRoundStatus CurrentRoundStatus = null;
        public static ServerBehaviour Instance { get; private set; }

        private void OnEnable()
        {
            Debug.Log("[Server] ServerBehaviour.OnEnable() is called");
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(lobbyScene);
                return;
            }
            if (!PhotonNetwork.IsMasterClient) return;
            Instance = this;
            StateMachine = new StateMachine();
            ReadSetting();
            WaitForOthersLoading();
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
                Destroy(gameObject);
        }

        private void Update()
        {
            StateMachine.UpdateState();
        }

        private void ReadSetting()
        {
            var room = PhotonNetwork.CurrentRoom;
            // var setting = (string)room.CustomProperties[SettingKeys.SETTING];
            // GameSettings = JsonUtility.FromJson<GameSetting>(setting);
            GameSettings = (GameSetting)room.CustomProperties[SettingKeys.SETTING];
        }

        private void WaitForOthersLoading()
        {
            var waitingState = new WaitForLoadingState
            {
                TotalPlayers = GameSettings.MaxPlayer
            };
            StateMachine.ChangeState(waitingState);
        }

        public void GamePrepare()
        {
            CurrentRoundStatus = new ServerRoundStatus(GameSettings, PhotonNetwork.PlayerList);
            mahjongSet = new MahjongSet(GameSettings, GameSettings.GetAllTiles());
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
            };
            StateMachine.ChangeState(prepareState);
        }

        public void GameAbort()
        {
            // todo -- implement abort logic here: at least one of the players cannot load into game, back to lobby scene
            Debug.LogError("The game aborted, this part is still under construction");
        }

        public void RoundStart(bool next, bool extra, bool keepSticks)
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

        public void DrawTile(int playerIndex, bool isLingShang = false, bool turnDoraAfterDiscard = false)
        {
            var drawState = new PlayerDrawTileState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet,
                IsLingShang = isLingShang,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(drawState);
        }

        public void DiscardTile(int playerIndex, Tile tile, bool isRichiing, bool discardLastDraw, int bonusTurnTime, bool turnDoraAfterDiscard)
        {
            var discardState = new PlayerDiscardTileState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                DiscardTile = tile,
                IsRichiing = isRichiing,
                DiscardLastDraw = discardLastDraw,
                BonusTurnTime = bonusTurnTime,
                MahjongSet = mahjongSet,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(discardState);
        }

        public void TurnEnd(int playerIndex, Tile discardingTile, bool isRichiing, OutTurnOperation[] operations,
            bool isRobKong, bool turnDoraAfterDiscard)
        {
            var turnEndState = new TurnEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                DiscardingTile = discardingTile,
                IsRichiing = isRichiing,
                Operations = operations,
                MahjongSet = mahjongSet,
                IsRobKong = isRobKong,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(turnEndState);
        }

        public void PerformOutTurnOperation(int newPlayerIndex, OutTurnOperation operation)
        {
            var operationPerformState = new OperationPerformState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = newPlayerIndex,
                DiscardPlayerIndex = CurrentRoundStatus.CurrentPlayerIndex,
                Operation = operation,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(operationPerformState);
        }

        public void Tsumo(int currentPlayerIndex, Tile winningTile, PointInfo pointInfo)
        {
            var tsumoState = new PlayerTsumoState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                TsumoPlayerIndex = currentPlayerIndex,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                TsumoPointInfo = pointInfo
            };
            StateMachine.ChangeState(tsumoState);
        }

        public void Rong(int currentPlayerIndex, Tile winningTile, int[] rongPlayerIndices, PointInfo[] rongPointInfos)
        {
            var rongState = new PlayerRongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = currentPlayerIndex,
                RongPlayerIndices = rongPlayerIndices,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                RongPointInfos = rongPointInfos
            };
            StateMachine.ChangeState(rongState);
        }

        public void Kong(int playerIndex, OpenMeld kong)
        {
            var kongState = new PlayerKongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet,
                Kong = kong
            };
            StateMachine.ChangeState(kongState);
        }

        public void RoundDraw(RoundDrawType type)
        {
            var drawState = new RoundDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                RoundDrawType = type
            };
            StateMachine.ChangeState(drawState);
        }

        public void BeiDora(int playerIndex)
        {
            var beiState = new PlayerBeiDoraState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(beiState);
        }

        public void PointTransfer(IList<PointTransfer> transfers, bool next, bool extra, bool keepSticks)
        {
            var transferState = new PointTransferState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                NextRound = next,
                ExtraRound = extra,
                KeepSticks = keepSticks,
                PointTransferList = transfers
            };
            StateMachine.ChangeState(transferState);
        }

        public void GameEnd()
        {
            var gameEndState = new GameEndState
            {
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(gameEndState);
        }
    }
}
