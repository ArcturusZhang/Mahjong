using System.Collections.Generic;
using Mahjong.Logic;
using Mahjong.Model;
using UnityEditor;
using UnityEngine;

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

    [MenuItem("MahjongLogic/Test 4winds")]
    public static void Test4Winds()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.S, 4), new Tile(Suit.S, 5), new Tile(Suit.S, 6),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 3), new Tile(Suit.Z, 3)
        };
        var openMelds = new List<Meld> {
            new Meld(false, new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4), new Tile(Suit.Z, 4)),
            new Meld(true, new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile(Suit.Z, 1))
        };
        var settings = new GameSetting();
        var handStatus = HandStatus.Tsumo;
        var roundStatus = new RoundStatus
        {
            PlayerIndex = 0,
            OyaPlayerIndex = 1,
            FieldCount = 0,
            TotalPlayer = 2
        };
        var pointInfo = MahjongLogic.GetPointInfo(handTiles.ToArray(), openMelds.ToArray(), new Tile(Suit.Z, 2), 
            handStatus, roundStatus, settings, false);
        Debug.Log($"Point: {pointInfo}");
    }

    [MenuItem("MahjongLogic/Test 3dragons")]
    public static void Test3Dragons()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.S, 4), new Tile(Suit.S, 5), new Tile(Suit.S, 6),
            new Tile(Suit.Z, 2), new Tile(Suit.Z, 2),
            new Tile(Suit.Z, 5), new Tile(Suit.Z, 5)
        };
        var openMelds = new List<Meld> {
            new Meld(false, new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6), new Tile(Suit.Z, 6)),
            new Meld(true, new Tile(Suit.Z, 7), new Tile(Suit.Z, 7), new Tile(Suit.Z, 7))
        };
        var settings = new GameSetting();
        var handStatus = HandStatus.Tsumo;
        var roundStatus = new RoundStatus
        {
            PlayerIndex = 0,
            OyaPlayerIndex = 1,
            FieldCount = 0,
            TotalPlayer = 2
        };
        var pointInfo = MahjongLogic.GetPointInfo(handTiles.ToArray(), openMelds.ToArray(), new Tile(Suit.Z, 2), 
            handStatus, roundStatus, settings, false);
        Debug.Log($"Point: {pointInfo}");
    }

    [MenuItem("MahjongLogic/Test dora")]
    public static void TestDora()
    {
        var allTiles = MahjongConstants.TwoPlayerTiles;
        var indicator = new Tile(Suit.Z, 2);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.Z, 7);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.Z, 4);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.S, 7);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.S, 9);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.S, 1);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.M, 1);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.M, 4);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
        indicator = new Tile(Suit.M, 9);
        Debug.Log($"When indicator is {indicator}, dora is {MahjongLogic.GetDoraTile(indicator, allTiles)}");
    }

    [MenuItem("MahjongLogic/Test pongs")]
    public static void TestPongs()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5, true)
        };
        var result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 4), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5)
        };
        result = MahjongLogic.GetPongs(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
    }

    [MenuItem("MahjongLogic/Test chows")]
    public static void TestChows()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 6)
        };
        var result = MahjongLogic.GetChows(handTiles, new Tile(Suit.M, 4), MeldSide.Left);
        Debug.Log($"Melds: {string.Join(",", result)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 5), new Tile(Suit.M, 6)
        };
        result = MahjongLogic.GetChows(handTiles, new Tile(Suit.M, 5, true), MeldSide.Opposite);
        Debug.Log($"Melds: {string.Join(",", result)}");
    }

    [MenuItem("MahjongLogic/Test combinations")]
    public static void TestCombinations()
    {
        var list = new List<int> {
            1, 2, 3, 4, 5, 6, 7, 8
        };
        var result = MahjongLogic.Combination(list, 1);
        Debug.Log($"Total results: {result.Count}");
        for (int i = 0; i < result.Count; i++)
        {
            Debug.Log($"{i}: {string.Join(",", result[i])}");
        }
    }

    [MenuItem("MahjongLogic/TestDiscard")]
    public static void TestDiscard()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true)
        };
        var dict = MahjongLogic.DiscardForReady(handTiles, new Tile(Suit.Z, 1));
        foreach (var item in dict)
        {
            Debug.Log($"{item.Key}, {string.Join(",", item.Value)}");
        }
    }

    [MenuItem("MahjongLogic/TestRichiKongs")]
    public static void TestRichiKongs()
    {
        var handTiles = new List<Tile> {
            new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
        };
        var kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 5));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
        handTiles = new List<Tile> {
            new Tile(Suit.M, 3), new Tile(Suit.M, 3), new Tile(Suit.M, 6), new Tile(Suit.M, 6),
            new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5)
        };
        kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 5));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
        kongs = MahjongLogic.GetRichiKongs(handTiles, new Tile(Suit.M, 3));
        Debug.Log($"Kongs: {string.Join(",", kongs)}");
    }
}
