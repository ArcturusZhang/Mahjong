using System.Collections;
using System.Collections.Generic;
using Multi;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public class MahjongSelector : MonoBehaviour
    {
        [Header("Tiles & Holders")]
        public Transform[] Walls;
        public PlayerHandHolder[] Hands;
        public PlayerRiverHolder[] Rivers;
        public PlayerOpenHolder[] Opens;

        [Header("Prefabs")] public GameObject MeldLeft;
        public GameObject MeldOpposite;
        public GameObject MeldRight;
        public GameObject MeldLeftKong;
        public GameObject MeldOppositeKong;
        public GameObject MeldRightKong;
        public GameObject MeldSelfKong;

        public IDictionary<MeldInstanceType, GameObject> PrefabDict { get; private set; }

        private void Awake()
        {
            PrefabDict = new Dictionary<MeldInstanceType, GameObject>
            {
                {MeldInstanceType.Left, MeldLeft},
                {MeldInstanceType.Opposite, MeldOpposite},
                {MeldInstanceType.Right, MeldRight},
                {MeldInstanceType.LeftKong, MeldLeftKong},
                {MeldInstanceType.OppositeKong, MeldOppositeKong},
                {MeldInstanceType.RightKong, MeldRightKong},
                {MeldInstanceType.SelfKong, MeldSelfKong}
            };
        }

        public void DrawToPlayer(int nextIndex, int playerIndex)
        {
            DrawTileAt(nextIndex);
            Hands[playerIndex].DrawingTile();
        }

        public void DrawToPlayer(int nextIndex, int playerIndex, Tile tile)
        {
            DrawTileAt(nextIndex);
            Hands[playerIndex].DrawingTile(tile);
        }

        public void DiscardTile(Tile tile, bool discardLastDraw, int playerIndex, bool richi = false)
        {
            Hands[playerIndex].DiscardTile(discardLastDraw);
            Rivers[playerIndex].DiscardTile(tile, richi);
        }

        public void Refresh(int count, int playerIndex)
        {
            Hands[playerIndex].Refresh(count);
        }

        public void Refresh(List<Tile> tiles, int playerIndex)
        {
            Hands[playerIndex].Refresh(tiles);
        }

        public IEnumerator DrawInitialCoroutine(Player self, int openIndex, int totalPlayers)
        {
            int selfDrawn = 0;
            for (int round = 0; round < GameManager.Instance.GameSettings.InitialDrawRound; round++)
            {
                for (int playerIndex = 0; playerIndex < totalPlayers; playerIndex++)
                {
                    Debug.Log($"Drawing from {openIndex} to {openIndex + GameManager.Instance.GameSettings.TilesEveryRound}");
                    openIndex = DrawTilesAt(openIndex, GameManager.Instance.GameSettings.TilesEveryRound);
                    if (self.PlayerIndex == playerIndex)
                    {
                        var tiles = self.HandTiles.GetRange(selfDrawn, GameManager.Instance.GameSettings.TilesEveryRound);
                        selfDrawn += GameManager.Instance.GameSettings.TilesEveryRound;
                        self.ClientAddTiles(tiles);
                    }

                    Hands[playerIndex].DrawTiles(GameManager.Instance.GameSettings.TilesEveryRound);

                    yield return new WaitForSeconds(0.5f);
                }
            }

            for (int playerIndex = 0; playerIndex < totalPlayers; playerIndex++)
            {
                openIndex = DrawTileAt(openIndex);
                if (self.PlayerIndex == playerIndex)
                {
                    var tiles = self.HandTiles.GetRange(selfDrawn, GameManager.Instance.GameSettings.TilesLastRound);
                    selfDrawn += GameManager.Instance.GameSettings.TilesLastRound;
                    self.ClientAddTiles(tiles);
                }

                Hands[playerIndex].DrawTile();

                yield return new WaitForSeconds(0.5f);
            }

            self.InitialDrawComplete(true);
            self.HandTiles.Sort();
            self.ClientUpdateTiles();
        }
        
        public void OpenToPlayer(int playerIndex, Meld meld, Tile discardTile, MeldInstanceType instanceType)
        {
            Opens[playerIndex].Open(meld, discardTile, instanceType);
        }

        private int DrawTileAt(int index)
        {
            return DrawTilesAt(index, 1);
        }

        private int DrawTilesAt(int index, int count)
        {
            for (int i = 0; i < count; i++)
                GetTile(index + i).SetActive(false);
            return index + count;
        }

        public void RevealTileAt(int index, Tile tile)
        {
            var tileObject = GetTile(index);
            tileObject.transform.localRotation *= Quaternion.Euler(0, 180, 0);
            var tileInstance = tileObject.GetComponent<TileInstance>();
            tileInstance.SetTile(tile);
        }

        private GameObject GetTile(int index)
        {
            index = Repeat(index);
            var wallIndex = index / MahjongConstants.WallTilesCount;
            var tileIndex = index % MahjongConstants.WallTilesCount;
            var wall = Walls[wallIndex];
            if (wall == null) Debug.LogWarning($"Wall at side {wallIndex} = null???");
            if (wall.GetChild(tileIndex) == null)
                Debug.LogWarning($"Tile {tileIndex} (total index {index}) at wall {wallIndex} is null");
            return wall.GetChild(tileIndex).gameObject;
        }

        private static int Repeat(int index)
        {
            return MahjongConstants.RepeatIndex(index, MahjongConstants.TotalTilesCount);
        }
    }
}