using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using Debug = Single.Debug;

namespace Multi.GameState
{
    public class RoundDrawState : IState
    {
        public GameSettings GameSettings;
        public List<Player> Players;
        public ServerRoundStatus CurrentRoundStatus;
        private ServerRoundDrawMessage[] messages;
        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            // Get waiting tiles for each player
            var waitingTiles = new WaitingData[Players.Count];
            for (int i = 0; i < Players.Count; i++) {
                var hand = CurrentRoundStatus.HandTiles(i);
                var open = CurrentRoundStatus.OpenMelds(i);
                waitingTiles[i] = new WaitingData {
                    HandTiles = hand,
                    WaitingTiles = MahjongLogic.WinningTiles(hand, open).ToArray()
                };
            }
            // Get messages and send
            messages = new ServerRoundDrawMessage[Players.Count];
            for (int i = 0; i < Players.Count; i++) {
                messages[i] = new ServerRoundDrawMessage{
                    WaitingData = waitingTiles
                };
                Players[i].connectionToClient.Send(MessageIds.ServerRoundDrawMessage, messages[i]);
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
        }

        public void OnStateUpdate()
        {
            Debug.Log($"Server is in {GetType().Name}", false);
        }
    }
}