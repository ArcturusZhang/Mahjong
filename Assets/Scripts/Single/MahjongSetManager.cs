using System;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single
{
    public class MahjongSetManager : MonoBehaviour
    {
        private const int tileCount = 4;

        [Header("Game settings")]
        public int[] redCounts = {1, 1, 1, 0};
        public int doraCount = 1;

        private List<Tile> allTiles;
        private int openIndex = -1;
        private int nextIndex = -1;
        private int lingshangDrawn = 0;
        private int[] lingshangIndices;
        private int[] doraIndicatorIndices;

        private void InitializeTiles()
        {
            if (allTiles == null) allTiles = new List<Tile>();
            allTiles.Clear();
            for (int suit = 0; suit < 4; suit++)
            {
                for (int index = 1; index <= 9; index++)
                {
                    int redCount = index == 5 ? redCounts[suit] : 0;
                    if (suit == 3 && index > 7) break;
                    for (int count = 0; count < tileCount; count++)
                    {
                        var tile = new Tile((Suit) suit, index, redCount-- > 0);
                        allTiles.Add(tile);
                    }
                }
            }
            ShuffleSet();

            openIndex = -1;
            nextIndex = -1;
            lingshangDrawn = 0;
        }

        public int OpenIndex => openIndex;

        public int NextIndex => nextIndex;

        public int NextLingshangIndex => lingshangIndices[lingshangDrawn];

        public void ShuffleSet()
        {
            allTiles.Shuffle();
        }

        public int Open(int dice)
        {
            InitializeTiles();
            int index = (dice - 1) % 4;
            openIndex = -MahjongConstants.WallTilesCount * index + dice * 2;
            openIndex = MahjongConstants.RepeatIndex(openIndex, allTiles.Count);
            nextIndex = openIndex;
            lingshangDrawn = 0;
            lingshangIndices = new[] {openIndex - 2, openIndex - 1, openIndex - 4, openIndex - 3};
            doraIndicatorIndices = new[] {openIndex - 6, openIndex - 8, openIndex - 10, openIndex - 12, openIndex - 14};
            return openIndex;
        }

        public Tile DrawTile()
        {
            var tile = allTiles[nextIndex];
            nextIndex = MahjongConstants.RepeatIndex(nextIndex + 1, allTiles.Count);
            return tile;
        }

        public List<Tile> DrawTiles(int count)
        {
            if (count <= 0) throw new ArgumentException("Cannot draw negative number of tiles");
            var list = new List<Tile>(count);
            for (int i = 0; i < count; i++)
            {
                var tile = allTiles[nextIndex];
                nextIndex = MahjongConstants.RepeatIndex(nextIndex + 1, allTiles.Count);
                list.Add(tile);
            }

            return list;
        }

        public Tile DrawLingshang()
        {
            if (lingshangDrawn >= 4) throw new ArgumentException("No more tiles to draw");
            int index = MahjongConstants.RepeatIndex(lingshangIndices[lingshangDrawn++], allTiles.Count);
            var tile = allTiles[index];
            return tile;
        }

        public List<Tile> DoraIndicators
        {
            get
            {
                var list = new List<Tile>();
                for (int i = 0; i < doraCount; i++)
                {
                    int index = MahjongConstants.RepeatIndex(doraIndicatorIndices[i], allTiles.Count);
                    list.Add(allTiles[index]);
                }

                return list;
            }
        }

        public List<int> DoraIndicatorIndices
        {
            get
            {
                var list = new List<int>();
                for (int i = 0; i < doraCount; i++)
                {
                    int index = MahjongConstants.RepeatIndex(doraIndicatorIndices[i], allTiles.Count);
                    list.Add(index);
                }

                return list;
            }
        }

        public List<Tile> UraDoraIndicators
        {
            get
            {
                var list = new List<Tile>();
                for (int i = 0; i < doraCount; i++)
                {
                    int index = MahjongConstants.RepeatIndex(doraIndicatorIndices[i] + 1, allTiles.Count);
                    list.Add(allTiles[index]);
                }

                return list;
            }
        }
    }
}