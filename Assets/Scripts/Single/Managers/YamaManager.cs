using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single.Managers
{
    public class YamaManager : ManagerBase
    {
        public static readonly IDictionary<int, int> IndexToYama = new Dictionary<int, int> {
            {0, 0}, {1, 3}, {2, 2}, {3, 1}
        };
        [SerializeField] private Transform[] Walls;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            if (CurrentRoundStatus.Dice == 0 || CurrentRoundStatus.OyaPlayerIndex < 0) return;
            UpdateTiles();
        }

        private void UpdateTiles()
        {
            var yamaIndex = GetYamaIndex(CurrentRoundStatus.Dice, CurrentRoundStatus.OyaPlayerIndex, CurrentRoundStatus.Places);
            UpdateYama(yamaIndex);
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

        private void UpdateYama(int openYamaIndex)
        {
            var dice = CurrentRoundStatus.Dice;
            var lingShangTilesCount = CurrentRoundStatus.Settings.LingShangTilesCount;
            var setData = CurrentRoundStatus.MahjongSetData;
            // drawn tiles
            for (int i = 0; i < setData.TilesDrawn; i++)
            {
                var t = GetTileAt(openYamaIndex, dice * 2 + i);
                DrawTile(t);
            }
            // lingshang tiles
            for (int i = 0; i < setData.LingShangDrawn; i++)
            {
                var s = dice - i / 2;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2 + i % 2);
                DrawTile(t);
            }
            // dora tiles
            for (int i = 0; i < setData.DoraIndicators.Length; i++)
            {
                var s = dice - lingShangTilesCount / 2 - i;
                var t = GetTileAt(openYamaIndex, (s - 1) * 2);
                TurnTileFaceUp(t, setData.DoraIndicators[i]);
            }
        }

        private Transform GetTileAt(int openYamaIndex, int index)
        {
            var totalTiles = CurrentRoundStatus.MahjongSetData.TotalTiles;
            if (index < 0) index += totalTiles;
            int yamaIndex = openYamaIndex;
            while (index >= Walls[yamaIndex].childCount)
            {
                index -= Walls[yamaIndex].childCount;
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
    }
}
