using System.Collections;
using System.Collections.Generic;
using Multi.Messages;
using Prototype.NetworkLobby;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Utils;

namespace Multi
{
    public class Player : NetworkBehaviour
    {
        [Header("UI Elements")] public PlayerHandPanel PlayerHandPanel;
//        public GameObject PlayerOpenPanel;

        [Header("Game Status")] [SyncVar] public int TotalPlayers;

        [SyncVar]
        public int PlayerIndex = -1; // Round order index -- 0: East, 1: South, 2: West, 3: North (This does not change)

        [SyncVar] public int BonusTurnTime = 20;
        // todo -- add more game status here, such as prevailing wind

        [Header("Player Public Data")] [SyncVar]
        public string PlayerName = "";

        [SyncVar(hook = nameof(OnPoints))] public int Points;
        [SyncVar] public bool Richi = false;
        [SyncVar] public int HandTilesCount = 0;
        public List<Meld> OpenMelds;

        [Header("Player Private Data")] public List<Tile> HandTiles;
        public Tile LastDraw;

        private bool discardMessageSent = false;

        public override void OnStartClient()
        {
            Debug.Log($"Player [{netId}] [name: {PlayerName}] OnStartClient is called");
            PlayerManager.Instance.AddPlayer(this);
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log($"Player [{netId}] [name: {PlayerName}] OnStartLocalPlayer is called");
            PlayerManager.Instance.LocalPlayer = this;
            RegisterHandlers();
        }

        public override void OnNetworkDestroy()
        {
            PlayerManager.Instance.RemovePlayer(this);
            // todo -- send message to server? is this possible?
        }

        [Client]
        private void RegisterHandlers()
        {
            LobbyManager.Instance.client.RegisterHandler(MessageConstants.DrawTileMessageId, OnDrawTileMessageReceived);
        }

        private void OnPoints(int amount)
        {
            Debug.Log("Player OnPoints is called");
            Points = amount;
            // todo -- change ui text
        }

        [Server] // todo -- change into message sending
        internal void ServerDrawTiles(int index, params Tile[] tiles)
        {
            HandTiles.AddRange(tiles);
            HandTilesCount += tiles.Length;
            RpcDrawTiles(index, tiles);
        }

        [ClientRpc]
        private void RpcDrawTiles(int index, params Tile[] tiles)
        {
            if (isServer) return; // if this is a server, the player on server has already done this piece of work
            if (isLocalPlayer)
            {
                HandTiles.AddRange(tiles);
                HandTilesCount += tiles.Length;
            }
            else
                HandTilesCount += tiles.Length;
        }

        [ClientRpc]
        internal void RpcYourTurnToDraw(int nextIndex)
        {
            ClientYourTurnToDraw(nextIndex);
        }

        [ClientRpc]
        internal void RpcDiscardTile(Tile tile, bool discardLastDraw, InTurnOperation operation)
        {
            ClientYourTurnToDiscard(tile, discardLastDraw, operation);
        }

        [Client]
        private void ClientYourTurnToDiscard(Tile tile, bool discardLastDraw, InTurnOperation operation)
        {
            // todo -- add effect for richi
            Debug.Log($"Player {PlayerIndex}'s turn, tile {tile} is discarded. DiscardLastDraw: {discardLastDraw}");
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.DiscardTile(tile, discardLastDraw, PlayerIndex);
            StartCoroutine(ClientUpdateTilesForSeconds(GameSettings.PlayerHandTilesSortDelay));
            if (!isLocalPlayer) return;
            if (discardLastDraw)
            {
                PlayerHandPanel.DiscardTile();
            }
            else
            {
                int index = HandTiles.FindIndex(tile, MahjongConstants.TileConcernColorEqualityComparer);
                HandTiles.RemoveAt(index);
                PlayerHandPanel.DiscardTile(index);
                HandTiles.Add(LastDraw);
                HandTiles.Sort();
                ClientUpdateTiles();
            }

            DisableInTurnPanel();
        }

        [ClientRpc]
        internal void RpcOutTurnOperation(Tile discardTile)
        {
            ClientHandleOutTurnOperation(discardTile);
        }

        [Client]
        private void ClientHandleOutTurnOperation(Tile discardTile)
        {
            if (!isLocalPlayer) return;
            // todo -- add out turn operation
            DisableOutTurnPanel();
            connectionToServer.Send(MessageConstants.OutTurnOperationMessageId, new OutTurnOperationMessage
            {
                PlayerIndex = PlayerIndex,
                Operation = OutTurnOperation.Skip
            });
        }

        [Client]
        internal void ClientDrawInitialTiles(int openIndex, params int[] playerIndices)
        {
            if (!isLocalPlayer) return;
            Debug.Log($"Player {PlayerIndex} ClientDrawInitialTiles is called");
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.DrawInitialTiles(this, openIndex, playerIndices);
        }

        [Client] // todo -- maybe need to move this method to MahjongSelector
        internal void ClientTurnDoraTiles(Tile[] tiles, int[] indices)
        {
            if (!isLocalPlayer) return;
            Debug.Log($"Player {PlayerIndex} ClientTurnDoraTiles is called");
            Assert.AreEqual(tiles.Length, indices.Length, "Something is very wrong in method ClientTurnDoraTiles");
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            for (int i = 0; i < tiles.Length; i++)
            {
                mahjongSelector.RevealTileAt(indices[i], tiles[i]);
            }
        }

        [Client]
        internal void ClientYourTurnToDraw(int nextIndex)
        {
            Debug.Log("Draw a tile from the mahjong set.");
            discardMessageSent = false;
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.DrawToPlayer(nextIndex, PlayerIndex);
        }

        [Client]
        private void OnDrawTileMessageReceived(NetworkMessage message)
        {
            var content = message.ReadMessage<DrawTileMessage>();
            var tile = content.Tile;
            Debug.Log($"Tile {tile} is drawn");
            LastDraw = tile;
            var operation = MahjongLogic.GetInTurnOperation(HandTiles, OpenMelds, tile);
            EnableInTurnPanel(operation);
            PlayerHandPanel.DrawTile(this, tile);
        }

        [Client]
        internal void ClientAddTiles(List<Tile> tiles)
        {
            if (!isLocalPlayer)
            {
                Debug.LogError("This method should only be called on local player");
                return;
            }

            PlayerHandPanel.AddTiles(this, tiles);
        }

        /// <summary>
        /// This method is only called on local player
        /// </summary>
        [Client]
        internal void ClientUpdateTiles()
        {
            if (!isLocalPlayer)
            {
                Debug.LogError("This method can only be called on local player!");
                return;
            }

            PlayerHandPanel.Refresh(this, HandTiles);
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.Refresh(HandTiles, PlayerIndex);
        }

        [Client]
        private IEnumerator ClientUpdateTilesForSeconds(float delay)
        {
            yield return new WaitForSeconds(delay);
            var mahjongSelector = MahjongManager.Instance.MahjongSelector;
            mahjongSelector.Refresh(HandTilesCount, PlayerIndex);
        }

        [Client]
        internal void InitialDrawComplete(bool complete)
        {
            Debug.Log($"Player {PlayerIndex} drawing complete, sending message!");
            connectionToServer.Send(MessageConstants.ReadinessMessageId, new ReadinessMessage
            {
                PlayerIndex = PlayerIndex
            });
        }

        [Client]
        internal void ClientDiscardTile(Tile tile, bool discardLastDraw)
        {
            if (discardMessageSent) return;
            discardMessageSent = true;
            Debug.Log($"Client attempts to discard tile {tile}");
            // todo -- richi status
            connectionToServer.Send(MessageConstants.DiscardTileMessageId, new DiscardTileMessage
            {
                DiscardTile = tile,
                Operation = InTurnOperation.Discard,
                PlayerIndex = PlayerIndex,
                DiscardLastDraw = discardLastDraw
            });
        }

        [Client]
        private void EnableInTurnPanel(InTurnOperation operation)
        {
            MahjongManager.Instance.InTurnOperationPanel.SetActive(true);
            MahjongManager.Instance.TsumoButton.gameObject.SetActive(operation.HasFlag(InTurnOperation.Tsumo));
            MahjongManager.Instance.RichiButton.gameObject.SetActive(operation.HasFlag(InTurnOperation.Richi));
            MahjongManager.Instance.InTurnKongButton.gameObject.SetActive(
                operation.HasFlag(InTurnOperation.ConcealedKong) | operation.HasFlag(InTurnOperation.AddedKong));
            // todo -- add button action listeners
        }

        [Client]
        private void DisableInTurnPanel()
        {
            MahjongManager.Instance.InTurnOperationPanel.SetActive(false);
        }

        [Client]
        private void EnableOutTurnPanel(HashSet<Meld> chows, HashSet<Meld> pongs, HashSet<Meld> kongs, bool rong)
        {
            MahjongManager.Instance.OutTurnOperationPanel.SetActive(true);
            MahjongManager.Instance.RongButton.gameObject.SetActive(rong);
            MahjongManager.Instance.ChowButton.gameObject.SetActive(chows.Count > 0);
            MahjongManager.Instance.PongButton.gameObject.SetActive(pongs.Count > 0);
            MahjongManager.Instance.OutTurnKongButton.gameObject.SetActive(kongs.Count > 0);
            // todo -- add listeners
        }

        [Client]
        private void DisableOutTurnPanel()
        {
            MahjongManager.Instance.OutTurnOperationPanel.SetActive(false);
        }
    }
}