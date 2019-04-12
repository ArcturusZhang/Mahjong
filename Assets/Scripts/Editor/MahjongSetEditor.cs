using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
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
                var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
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

        [MenuItem("Mahjong/Test/Test MahjongLogic")]
        public static void TestForDecompose()
        {
            var handTiles = new Tile[]
            {
                new Tile(Suit.M, 9), new Tile(Suit.M, 9), new Tile(Suit.P, 4), new Tile(Suit.P, 5, true),
                new Tile(Suit.P, 7), new Tile(Suit.P, 8), new Tile(Suit.P, 9), new Tile(Suit.S, 2),
                new Tile(Suit.S, 3), new Tile(Suit.S, 4), new Tile(Suit.S, 5), new Tile(Suit.S, 6),
                new Tile(Suit.S, 7),
            };
            var openMelds = new Meld[0];
            var winningTile = new Tile(Suit.P, 6);
            var handStatus = HandStatus.Menqing | HandStatus.Tsumo;
            var roundStatus = new RoundStatus
            {
                PlayerIndex = 1,
                RoundCount = 1,
                FieldCount = 1,
                CurrentExtraRound = 0,
                TilesLeft = 100,
                TotalPlayer = 2
            };
            var yakuSettings = AssetDatabase.LoadAssetAtPath<YakuSettings>("Assets/GameData/YakuSettings.asset");
            var pointInfo = MahjongLogic.GetPointInfo(handTiles, openMelds, winningTile, handStatus, roundStatus,
                yakuSettings);
            Debug.Log(pointInfo);
        }

        [MenuItem("Mahjong/Postprocess/Transparency background")]
        public static void Transparency()
        {
            var texture = new Texture2D(1, 1);
            var bytes = File.ReadAllBytes("Assets/Resources/Textures/UIElements/ui.png");
            Debug.Log(bytes.Length);
            texture.LoadImage(bytes);
            texture.Apply();
            Debug.Log($"Width: {texture.width}, height: {texture.height}");
            TransparencyPixel(texture, texture.width, texture.height);
            texture.Apply();
            bytes = texture.EncodeToPNG();
            File.WriteAllBytes("Assets/Resources/Textures/UIElements/ui_new.png", bytes);
        }

        private static void TransparencyPixel(Texture2D texture, int x, int y)
        {
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(x, y));
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                var color = texture.GetPixel(p.x, p.y);
                if (color == Color.white)
                {
                    texture.SetPixel(p.x, p.y, new Color(0, 0, 0, 0));
                    if (p.x + 1 <= texture.width) queue.Enqueue(new Vector2Int(p.x + 1, p.y));
                    if (p.x - 1 >= 0) queue.Enqueue(new Vector2Int(p.x - 1, p.y));
                    if (p.y + 1 <= texture.height) queue.Enqueue(new Vector2Int(p.x, p.y + 1));
                    if (p.y - 1 >= 0) queue.Enqueue(new Vector2Int(p.x, p.y - 1));
                }
            }
        }

        [MenuItem("Mahjong/Meld Creation/Create Meld (Left)")]
        public static void CreateMeldLeft()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("Meld (left)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2 - 2 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create MeldKong (Left)")]
        public static void CreateMeldLeftKong()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("MeldKong (left)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - 2 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2 - 3 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create Meld (Right)")]
        public static void CreateMeldRight()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("Meld (right)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create MeldKong (Right)")]
        public static void CreateMeldRightKong()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("MeldKong (right)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - 2 * MahjongConstants.TileWidth - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create Meld (Opposite)")]
        public static void CreateMeldOpposite()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("Meld (opposite)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2 - MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create MeldKong (Opposite)")]
        public static void CreateMeldOppositeKong()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("MeldKong (opposite)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileWidth / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileHeight / 2 - 2 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - 2 * MahjongConstants.TileWidth - MahjongConstants.TileHeight);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
        }

        [MenuItem("Mahjong/Meld Creation/Create MeldKong (Self)")]
        public static void CreateMeldSelf()
        {
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mahjong/tile.prefab");
            var meldObject = new GameObject("MeldKong (self)");
            meldObject.transform.position = Vector3.zero;
            meldObject.transform.rotation = Quaternion.identity;
            var tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2);
            tileObject.transform.localRotation = Quaternion.Euler(-180, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - 2 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(0, 270, -90);
            tileObject = Instantiate(tilePrefab, meldObject.transform);
            tileObject.transform.localPosition =
                new Vector3(-MahjongConstants.TileHeight / 2, MahjongConstants.TileThickness / 2,
                    -MahjongConstants.TileWidth / 2 - 3 * MahjongConstants.TileWidth);
            tileObject.transform.localRotation = Quaternion.Euler(-180, 270, -90);
        }
    }
}