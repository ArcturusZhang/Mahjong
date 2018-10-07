using System.Collections.Generic;
using Multi;
using Single.MahjongDataType;
using UnityEngine.Assertions;

namespace Single
{
    public static class HandAnalyst
    {
//        /// <summary>
//        /// A simple method used to test if a player wins by tsumo after drawing a tile.
//        /// </summary>
//        /// <param name="hand"></param>
//        /// <param name="seatWind"></param>
//        /// <param name="prevailingWind"></param>
//        /// <returns></returns>
//        public static bool TestForTsumo(MahjongHand hand, Tile seatWind, Tile prevailingWind)
//        {
//            if (!hand.HasWin) return false;
//            foreach (var meldList in hand.Decomposition)
//            {
//                
//            }
//
//            return false;
//        }
//
//        public static bool TestForTsumo(MahjongHand hand)
//        {
//            return false;
//        }
//
//        public static bool TestForRong(List<Tile> handTiles, List<OldMeld> openMelds, Tile discardTile)
//        {
//            return false;
//        }
//
////        private static List<Yaku> GetAvailableYakuList()
////        {
////            var yakuTypes = typeof(Yaku).Assembly.GetTypes()
////                .Where(clazz => !clazz.IsAbstract && !clazz.IsInterface && typeof(Yaku).IsAssignableFrom(clazz));
////
////            return yakuTypes.Select(type => (Yaku) Activator.CreateInstance(type)).ToList();
////        }
//
//        public static HashSet<OldMeld> TestForChow(List<Tile> handTiles, Tile discardTile)
//        {
//            var result = new HashSet<OldMeld>();
//            if (discardTile.Suit == Suit.Z) return result;
//
//            var tilesP2 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank - 2);
//            var tilesP1 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank - 1);
//            var tilesN1 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank + 1);
//            var tilesN2 = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank + 2);
//            foreach (var tile2 in tilesP2)
//            {
//                foreach (var tile1 in tilesP1)
//                {
//                    result.Add(new OldMeld(true, tile2, tile1, discardTile));
//                }
//            }
//
//            foreach (var tileP in tilesP1)
//            {
//                foreach (var tileN in tilesN1)
//                {
//                    result.Add(new OldMeld(true, tileP, discardTile, tileN));
//                }
//            }
//
//            foreach (var tile1 in tilesN1)
//            {
//                foreach (var tile2 in tilesN2)
//                {
//                    result.Add(new OldMeld(true, discardTile, tile1, tile2));
//                }
//            }
//
//            return result;
//        }
//
//        public static HashSet<OldMeld> TestForPong(List<Tile> handTiles, Tile discardTile)
//        {
//            var result = new HashSet<OldMeld>();
//            var tiles = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank);
//            if (tiles.Count < 2) return result;
//            if (tiles.Count == 2)
//            {
//                tiles.Add(discardTile);
//                tiles.Sort();
//                Assert.AreEqual(tiles.Count, 3, "In method [TestForPong], this should not happen");
//                result.Add(new OldMeld(true, tiles.ToArray()));
//                return result;
//            }
//
//            // tiles.Count == 3
//            tiles.Sort();
//            result.Add(new OldMeld(true, tiles[0], tiles[1], discardTile));
//            result.Add(new OldMeld(true, tiles[0], tiles[2], discardTile));
//            result.Add(new OldMeld(true, tiles[1], tiles[2], discardTile));
//            return result;
//        }
//
//        public static HashSet<OldMeld> TestForKong(List<Tile> handTiles, Tile discardTile)
//        {
//            var result = new HashSet<OldMeld>();
//            var tiles = handTiles.FindAll(tile => tile.Suit == discardTile.Suit && tile.Rank == discardTile.Rank);
//            if (tiles.Count < 3) return result;
//            tiles.Add(discardTile);
//            result.Add(new OldMeld(true, tiles.ToArray()));
//            return result;
//        }
    }
}