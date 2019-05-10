using System.Collections;
using System.Linq;
using Multi;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;

namespace Single.GameState
{
    public class PlayerTurnEndState : ClientState
    {
        public int PlayerIndex;
        public OutTurnOperationType ChosenOperationType;
        public OutTurnOperation[] Operations;
        public int[] Points;
        public bool[] RichiStatus;
        public int RichiSticks;
        public MahjongSetData MahjongSetData;

        public override void OnClientStateEnter()
        {
            Debug.Log($"Turn ends, operation {string.Join(",", Operations)} is taking.");
            // update richi status
            CurrentRoundStatus.UpdateRichiStatus(RichiStatus);
            // update points
            CurrentRoundStatus.UpdatePoints(Points);
            // update richi sticks
            CurrentRoundStatus.RichiSticks = RichiSticks;
            // update mahjong set
            CurrentRoundStatus.MahjongSetData = MahjongSetData;
            // perform operation
            if (Operations.All(op => op.Type == OutTurnOperationType.Skip)) return;
            for (int playerIndex = 0; playerIndex < Operations.Length; playerIndex++)
            {
                int placeIndex = CurrentRoundStatus.GetPlaceIndex(playerIndex);
                var operation = Operations[playerIndex];
                switch (operation.Type)
                {
                    case OutTurnOperationType.Skip:
                        continue;
                    case OutTurnOperationType.Chow:
                    case OutTurnOperationType.Pong:
                    case OutTurnOperationType.Kong:
                        Debug.LogError("Under construction");
                        break;
                    case OutTurnOperationType.Rong:
                        controller.StartCoroutine(controller.ShowEffect(placeIndex, UI.PlayerEffectManager.GetAnimationType(operation.Type)));
                        controller.StartCoroutine(controller.RevealHandTiles(placeIndex, operation.HandData));
                        break;
                    case OutTurnOperationType.RoundDraw:
                        Debug.Log("Round is draw");
                        break;
                }
            }
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}