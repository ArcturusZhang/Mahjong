using System.Collections.Generic;
using System.Text;
using Multi;
using Single;
using Single.MahjongDataType;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MahjongSetEditor : EditorWindow
    {
        [MenuItem("Mahjong/Create Mahjong Set")]
        public static void CreateMahjongSet()
        {
            var mahjongSetGameObject = GameObject.Find("MahjongTiles");
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/tile.prefab");
            var mahjongSetTransform = mahjongSetGameObject.transform;
            int corner = 0;
            int count = 0;
            for (int i = 0; i < MahjongConstants.TotalTilesCount; i++)
            {
                int x = count / 2;
                int y = count % 2;
                var position = new Vector3(x * MahjongConstants.TileWidth + x * MahjongConstants.Gap,
                    (1 - y) * MahjongConstants.TileThickness + MahjongConstants.TileThickness / 2, 0);
                var rotation = Quaternion.Euler(180, 180, -90);
                var tileObject = Instantiate(tilePrefab, mahjongSetTransform.GetChild(corner));
                tileObject.transform.localPosition = position;
                tileObject.transform.localRotation = rotation;
                tileObject.name = $"tile{i}";
                count++;
                if (count >= MahjongConstants.WallTilesCount)
                {
                    count = 0;
                    corner++;
                }
            }
        }

        [MenuItem("Mahjong/Destroy Mahjong Set")]
        public static void DestroyMahjongSet()
        {
            for (int corner = 0; corner < MahjongConstants.WallCount; corner++)
            {
                var yama = GameObject.Find("Yama" + corner);
                for (int i = yama.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(yama.transform.GetChild(i).gameObject);
                }
            }
        }

        [MenuItem("Mahjong/Build Player Hand Tiles")]
        public static void BuildPlayerHandTiles()
        {
            for (int i = 0; i < 4; i++)
            {
                var handTileHolder = GameObject.Find($"playerHand{i}");
                var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/tile.prefab");
                for (int count = 0; count < MahjongConstants.CompleteHandTilesCount; count++)
                {
                    var position = new Vector3(count * (MahjongConstants.TileWidth + MahjongConstants.Gap),
                        MahjongConstants.TileHeight / 2, 0);
                    var rotation = Quaternion.Euler(270, 0, -90);
                    var tileObject = Instantiate(tilePrefab, handTileHolder.transform);
                    tileObject.transform.localPosition = position;
                    tileObject.transform.localRotation = rotation;
                    tileObject.name = $"handTile{count}";
                }
            }
        }

        [MenuItem("Mahjong/Delete Player Hand Tiles")]
        public static void DeletePlayerHandTiles()
        {
            for (int i = 0; i < 4; i++)
            {
                var handTileHolder = GameObject.Find($"playerHand{i}");
                for (int count = handTileHolder.transform.childCount - 1; count >= 0; count--)
                {
                    DestroyImmediate(handTileHolder.transform.GetChild(count).gameObject);
                }
            }
        }

        [MenuItem("Mahjong/Test/Test for decompose")]
        public static void TestForDecompose()
        {
            var handTiles = new List<Tile>
            {
                new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 2),
                new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 3),
                new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
                new Tile(Suit.M, 5),
            };
            var decompose = MahjongLogic.Decompose(handTiles, new List<Meld>(), new Tile(Suit.M, 5));
            var builder = new StringBuilder();
            foreach (var sub in decompose)
            {
                foreach (var meld in sub)
                {
                    builder.Append(meld).Append(", ");
                }

                builder.Append("\n");
            }
            Debug.Log(builder);
        }

//        [MenuItem("Mahjong/Test/Test for chow")]
//        public static void TestForChow()
//        {
//            var list = new List<Tile>
//            {
//                new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 4),
//                new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
//            };
//            var result = HandAnalyst.TestForChow(list, new Tile(Suit.M, 4));
//            var builder = new StringBuilder();
//            foreach (var meld in result)
//            {
//                builder.Append(meld).Append(", ");
//            }
//            Debug.Log(builder);
//        }
//
//        [MenuItem("Mahjong/Test/Test for pong")]
//        public static void TestForPong()
//        {
//            var list = new List<Tile>
//            {
//                new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 4),
//                new Tile(Suit.M, 5, true), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
//            };
//            var result = HandAnalyst.TestForPong(list, new Tile(Suit.M, 5));
//            var builder = new StringBuilder();
//            foreach (var meld in result)
//            {
//                builder.Append(meld).Append(", ");
//            }
//            Debug.Log(builder);
//        }
//
//        [MenuItem("Mahjong/Test/Test for kong")]
//        public static void TestForKong()
//        {
//            var list = new List<Tile>
//            {
//                new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 4),
//                new Tile(Suit.M, 5, true), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
//            };
//            var result = HandAnalyst.TestForKong(list, new Tile(Suit.M, 5));
//            var builder = new StringBuilder();
//            foreach (var meld in result)
//            {
//                builder.Append(meld).Append(", ");
//            }
//            Debug.Log(builder);
//        }
    }
}