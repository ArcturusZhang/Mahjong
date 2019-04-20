using System.Collections.Generic;
using System.Linq;
using Multi.MahjongMessages;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using StateMachine.Interfaces;
using UnityEngine;
using UnityEngine.Networking;


namespace Multi.GameState
{
    // todo -- draw lingshang tile
    public class PlayerDrawTileState : IState
    {
        public GameSettings GameSettings;
        public int CurrentPlayerIndex;
        public IList<Player> Players;
        public MahjongSet MahjongSet;
        public ServerRoundStatus CurrentRoundStatus;
        private MessageBase[] messages;
        private bool[] responds;
        private float lastSendTime;
        private float firstSendTime;
        private float serverTimeOut;

        public void OnStateEnter()
        {
            Debug.Log($"Server enters {GetType().Name}");
            NetworkServer.RegisterHandler(MessageIds.ClientReadinessMessage, OnReadinessMessageReceived);
            NetworkServer.RegisterHandler(MessageIds.ClientDiscardTileMessage, OnDiscardTileReceived);
            var tile = MahjongSet.DrawTile();
            Debug.Log($"[Server] Distribute a tile {tile} to current turn player {CurrentPlayerIndex}.");
            CurrentRoundStatus.CurrentPlayerIndex = CurrentPlayerIndex;
            CurrentRoundStatus.LastDraw = tile;
            messages = new MessageBase[Players.Count];
            responds = new bool[Players.Count];
            for (int i = 0; i < Players.Count; i++)
            {
                if (i == CurrentPlayerIndex) continue;
                messages[i] = new ServerOtherDrawTileMessage
                {
                    PlayerIndex = i,
                    CurrentTurnPlayerIndex = CurrentPlayerIndex,
                    MahjongSetData = MahjongSet.Data
                };
                Players[i].connectionToClient.Send(MessageIds.ServerOtherDrawTileMessage, messages[i]);
            }
            messages[CurrentPlayerIndex] = new ServerDrawTileMessage
            {
                PlayerIndex = CurrentPlayerIndex,
                Tile = tile,
                BonusTurnTime = Players[CurrentPlayerIndex].BonusTurnTime,
                Operations = GetOperations(CurrentPlayerIndex),
                MahjongSetData = MahjongSet.Data
            };
            Players[CurrentPlayerIndex].connectionToClient.Send(MessageIds.ServerDrawTileMessage, messages[CurrentPlayerIndex]);
            firstSendTime = Time.time;
            lastSendTime = Time.time;
            serverTimeOut = GameSettings.BaseTurnTime + Players[CurrentPlayerIndex].BonusTurnTime + ServerConstants.ServerTimeBuffer;
        }

        // todo -- complete this
        private InTurnOperationType GetOperations(int index)
        {
            return InTurnOperationType.Discard;
        }

        private void OnReadinessMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientReadinessMessage>();
            Debug.Log($"[Server] Received ClientReadinessMessage: {content}");
            if (content.Content != MessageIds.ServerDrawTileMessage)
            {
                Debug.LogError("Something is wrong, the received readiness message contains invalid content.");
                return;
            }
            responds[content.PlayerIndex] = true;
        }

        private void OnDiscardTileReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<ClientDiscardRequestMessage>();
            if (content.PlayerIndex != CurrentRoundStatus.CurrentPlayerIndex)
            {
                Debug.Log($"[Server] It is not player {content.PlayerIndex}'s turn to discard a tile, ignoring this message");
                return;
            }
            // handle message
            Debug.Log($"[Server] Received ClientDiscardRequestMessage {content}");
            // Change to discardTileState
            ServerBehaviour.Instance.DiscardTile(content.PlayerIndex, content.Tile, content.IsRichiing, content.DiscardingLastDraw, content.BonusTurnTime);
        }

        public void OnStateUpdate()
        {
            Debug.Log($"Server is in {GetType().Name}");
            // Sending messages until received all responds from all players
            if (Time.time - lastSendTime > ServerConstants.MessageResendInterval && !responds.All(r => r))
            {
                // resend message
                for (int i = 0; i < Players.Count; i++)
                {
                    if (responds[i]) continue;
                    Players[i].connectionToClient.Send(
                        i == CurrentPlayerIndex ? MessageIds.ServerDrawTileMessage : MessageIds.ServerOtherDrawTileMessage,
                        messages[i]);
                }
            }
            // Time out
            if (Time.time - firstSendTime > serverTimeOut)
            {
                // force auto discard
                ServerBehaviour.Instance.DiscardTile(CurrentPlayerIndex, (Tile)CurrentRoundStatus.LastDraw, false, true, 0);
            }
        }

        public void OnStateExit()
        {
            Debug.Log($"Server exits {GetType().Name}");
            NetworkServer.UnregisterHandler(MessageIds.ClientReadinessMessage);
            NetworkServer.UnregisterHandler(MessageIds.ClientDiscardTileMessage);
        }
    }
}