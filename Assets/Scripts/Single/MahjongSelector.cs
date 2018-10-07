using System.Collections;
using System.Collections.Generic;
using Multi;
using Single.MahjongDataType;
using UnityEngine;

namespace Single
{
    public class MahjongSelector : MonoBehaviour
    {
        public Transform[] Walls;
        public PlayerHandHolder[] Hands;
        public PlayerRiverHolder[] Rivers;

        public void DrawInitialTiles(Player self, int openIndex, params int[] playerIndices)
        {
            StartCoroutine(DrawInitialCoroutine(self, openIndex, playerIndices));
        }

        public void DrawToPlayer(int nextIndex, int playerIndex)
        {
            DrawTileAt(nextIndex);
            var hand = Hands[playerIndex];
            hand.DrawingTile();
        }

        public void DrawToPlayer(int nextIndex, int playerIndex, Tile tile)
        {
            DrawTileAt(nextIndex);
            var hand = Hands[playerIndex];
            hand.DrawingTile(tile);
        }

        public void DiscardTile(Tile tile, bool discardLastDraw, int selfWind, bool richi = false)
        {
            Hands[selfWind].DiscardTile(discardLastDraw);
            Rivers[selfWind].DiscardTile(tile, richi);
        }

        public void Refresh(int count, int selfWind)
        {
            Hands[selfWind].Refresh(count);
        }

        public void Refresh(List<Tile> tiles, int selfWind)
        {
            Hands[selfWind].Refresh(tiles);
        }

        private IEnumerator DrawInitialCoroutine(Player self, int openIndex, params int[] playerIndices)
        {
            int selfDrawn = 0;
            for (int round = 0; round < GameSettings.InitialDrawRound; round++)
            {
                foreach (int playerIndex in playerIndices)
                {
                    openIndex = DrawTilesAt(openIndex, GameSettings.TilesEveryRound);
                    if (self.PlayerIndex == playerIndex)
                    {
                        var tiles = self.HandTiles.GetRange(selfDrawn, GameSettings.TilesEveryRound);
                        selfDrawn += GameSettings.TilesEveryRound;
                        Hands[playerIndex].DrawTiles(tiles);
                        self.ClientAddTiles(tiles);
                    }
                    else
                        Hands[playerIndex].DrawTiles(GameSettings.TilesEveryRound);

                    yield return new WaitForSeconds(0.5f);
                }
            }

            foreach (int playerIndex in playerIndices)
            {
                openIndex = DrawTileAt(openIndex);
                if (self.PlayerIndex == playerIndex)
                {
                    var tiles = self.HandTiles.GetRange(selfDrawn, GameSettings.TilesLastRound);
                    selfDrawn += GameSettings.TilesLastRound;
                    Hands[playerIndex].DrawTiles(tiles);
                    self.ClientAddTiles(tiles);
                }
                else
                    Hands[playerIndex].DrawTile();

                yield return new WaitForSeconds(0.5f);
            }
            self.InitialDrawComplete(true);
            self.HandTiles.Sort();
            self.ClientUpdateTiles();
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
            return wall.GetChild(tileIndex).gameObject;
        }

        private static int Repeat(int index)
        {
            return MahjongConstants.RepeatIndex(index, MahjongConstants.TotalTilesCount);
        }
    }
}