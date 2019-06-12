using System.Collections.Generic;
using Common.Interfaces;
using GamePlay.Client.Model;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEngine;
using Utils;

namespace GamePlay.Client.View
{
    public class YamaManager : MonoBehaviour, IObserver<ClientRoundStatus>
    {
        public static readonly IDictionary<int, int> IndexToYama = new Dictionary<int, int> {
            {0, 0}, {1, 3}, {2, 2}, {3, 1}
        };
        private static readonly IDictionary<GamePlayers, int[]> TileCountOnWall = new Dictionary<GamePlayers, int[]> {
            {GamePlayers.Four, new int[] {34, 34, 34, 34}},
            {GamePlayers.Three, new int[] {28, 26, 28, 26}},
            {GamePlayers.Two, new int[] {20, 20, 20, 20}}
        };

        [SerializeField] private Transform[] Walls;

        private void UpdateTiles(ClientRoundStatus status)
        {
            HideUnusedTiles(status.GameSetting.GamePlayers);
            var yamaIndex = GetYamaIndex(status.Dice, status.OyaPlayerIndex, status.Places);
            UpdateYama(yamaIndex, status);
        }

        private void HideUnusedTiles(GamePlayers gamePlayers)
        {
            for (int yamaIndex = 0; yamaIndex < Walls.Length; yamaIndex++)
            {
                int count = GetYamaTotalTiles(yamaIndex, gamePlayers);
                for (int i = count; i < Walls[yamaIndex].childCount; i++)
                {
                    var t = Walls[yamaIndex].GetChild(i);
                    t.gameObject.SetActive(false);
                }
            }
        }

        private static int GetYamaIndex(int dice, int oya, int[] places)
        {
            if (dice <= 0 || oya < 0 || oya >= 4 || places == null || places.Length != 4)
                throw new System.ArgumentException();
            var openSideOffset = (dice - 1) % 4;
            var openIndex = (oya + openSideOffset) % 4;
            var index = System.Array.FindIndex(places, i => i == openIndex);
            return IndexToYama[index];
        }

        private static int GetYamaTotalTiles(int yamaIndex, GamePlayers gamePlayers)
        {
            return TileCountOnWall[gamePlayers][yamaIndex];
        }

        private void UpdateYama(int openYamaIndex, ClientRoundStatus status)
        {
            var dice = status.Dice;
            var lingShangTilesCount = status.GameSetting.LingshangTilesCount;
            var setData = status.MahjongSetData;
            // drawn tiles
            for (int i = 0; i < setData.TilesDrawn; i++)
            {
                var t = GetTileAt(openYamaIndex, dice * 2 + i, status);
                DrawTile(t);
            }
            // lingshang tiles
            for (int i = 0; i < setData.LingShangDrawn; i++)
            {
                var s = dice - i / 2;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2 + i % 2, status);
                DrawTile(t);
            }
            // dora tiles
            var length = setData.DoraIndicators == null ? 0 : setData.DoraIndicators.Length;
            for (int i = 0; i < length; i++)
            {
                var s = dice - lingShangTilesCount / 2 - i;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2, status);
                TurnTileFaceUp(t, setData.DoraIndicators[i]);
            }
        }

        private Transform GetTileAt(int openYamaIndex, int index, ClientRoundStatus status)
        {
            var totalTiles = status.MahjongSetData.TotalTiles;
            if (index < 0) index += totalTiles;
            int yamaIndex = openYamaIndex;
            var gamePlayers = status.GameSetting.GamePlayers;
            while (index >= GetYamaTotalTiles(yamaIndex, gamePlayers))
            {
                index -= GetYamaTotalTiles(yamaIndex, gamePlayers);
                yamaIndex++;
                if (yamaIndex >= 4) yamaIndex -= 4;
            }
            return Walls[yamaIndex].GetChild(index);
        }

        public void ResetAllTiles()
        {
            foreach (var wall in Walls)
            {
                wall.TraversalChildren(t =>
                {
                    t.gameObject.SetActive(true);
                    TurnTileFaceDown(t);
                });
            }
        }

        private static void DrawTile(Transform t)
        {
            t.gameObject.SetActive(false);
        }

        private static void TurnTileFaceUp(Transform t, Tile tile)
        {
            t.localRotation = MahjongConstants.FaceUpOnWall;
            var tileInstance = t.GetComponent<TileInstance>();
            tileInstance.SetTile(tile);
        }

        private static void TurnTileFaceDown(Transform t)
        {
            t.localRotation = MahjongConstants.FaceDownOnWall;
        }

        public void UpdateStatus(ClientRoundStatus subject)
        {
            if (subject == null || subject.Dice == 0 || subject.OyaPlayerIndex < 0) return;
            UpdateTiles(subject);
        }
    }
}
