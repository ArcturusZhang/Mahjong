using System.Collections.Generic;
using System.Linq;
using Single;
using Single.MahjongDataType;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class MahjongLogicTest
{
    [MenuItem("MahjongLogic/Test GuoshiWushuang")]
    public static void TestGuoshiWushuang()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 9),
            new Tile(Suit.P, 1), new Tile(Suit.P, 9),
            new Tile(Suit.S, 1), new Tile(Suit.S, 9),
            new Tile(Suit.Z, 1), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 4), new Tile(Suit.Z, 3),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 7),
            new Tile(Suit.Z, 6)
        };
        Debug.Log($"Hand tiles: {string.Join("", handTiles)}\nWaiting tiles: {string.Join("", MahjongLogic.WinningTiles(handTiles, new List<Meld>()))}");
    }

    [MenuItem("MahjongLogic/Test 7pairs")]
    public static void Test7Pairs()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 1),
            new Tile(Suit.P, 9), new Tile(Suit.P, 9),
            new Tile(Suit.S, 1), new Tile(Suit.S, 9),
            new Tile(Suit.S, 9), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 5),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 6),
            new Tile(Suit.Z, 6)
        };
        Debug.Log($"Hand tiles: {string.Join("", handTiles)}\nWaiting tiles: {string.Join("", MahjongLogic.WinningTiles(handTiles, new List<Meld>()))}");
        handTiles = new List<Tile> {
            new Tile(Suit.S, 2), new Tile(Suit.S, 3),
            new Tile(Suit.S, 3), new Tile(Suit.S, 4),
            new Tile(Suit.S, 4), new Tile(Suit.S, 5),
            new Tile(Suit.S, 6), new Tile(Suit.S, 5),
            new Tile(Suit.S, 6), new Tile(Suit.S, 7),
            new Tile(Suit.S, 8), new Tile(Suit.S, 7),
            new Tile(Suit.S, 8)
        };
        Debug.Log($"Hand tiles: {string.Join("", handTiles)}\nWaiting tiles: {string.Join("", MahjongLogic.WinningTiles(handTiles, new List<Meld>()))}");
    }
}
