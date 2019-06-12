using System.Collections;
using System.Linq;
using GamePlay.Client.View;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.Controller.GameState
{
    public class PlayerTurnEndState : ClientState
    {
        public int PlayerIndex;
        public OutTurnOperationType ChosenOperationType;
        public OutTurnOperation[] Operations;
        public int[] Points;
        public bool[] RichiStatus;
        public int RichiSticks;
        public bool Zhenting;
        public MahjongSetData MahjongSetData;

        public override void OnClientStateEnter()
        {
            Debug.Log($"Turn ends, operation {string.Join(",", Operations)} is taking.");
            // update current place index
            CurrentRoundStatus.SetCurrentPlaceIndex(PlayerIndex);
            // update richi status
            CurrentRoundStatus.UpdateRichiStatus(RichiStatus);
            // update points
            CurrentRoundStatus.UpdatePoints(Points);
            // update richi sticks
            CurrentRoundStatus.SetRichiSticks(RichiSticks);
            // update mahjong set
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            // update zhenting status
            CurrentRoundStatus.SetZhenting(Zhenting);
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
                        HandleOpen(placeIndex, operation);
                        break;
                    case OutTurnOperationType.Rong:
                        HandleRong(placeIndex, operation);
                        break;
                    case OutTurnOperationType.RoundDraw:
                        HandleRoundDraw(operation);
                        break;
                }
            }
        }

        private void HandleOpen(int placeIndex, OutTurnOperation operation)
        {
            Debug.Log($"Operation: {operation}");
            // show effect
            controller.ShowEffect(placeIndex, PlayerEffectManager.GetAnimationType(operation.Type));
        }

        private void HandleRong(int placeIndex, OutTurnOperation operation)
        {
            controller.ShowEffect(placeIndex, PlayerEffectManager.GetAnimationType(operation.Type));
            controller.StartCoroutine(controller.RevealHandTiles(placeIndex, operation.HandData));
        }

        private void HandleRoundDraw(OutTurnOperation operation)
        {
            // controller.RoundDrawManager.SetDrawType(operation.RoundDrawType);
            controller.StartCoroutine(ShowRoundDrawEffect(operation.RoundDrawType));
        }

        private IEnumerator ShowRoundDrawEffect(RoundDrawType type) {
            controller.RoundDrawManager.SetDrawType(type);
            yield return new WaitForSeconds(2);
            controller.RoundDrawManager.Fade(type);
        }

        public override void OnClientStateExit()
        {
        }

        public override void OnStateUpdate()
        {
        }
    }
}