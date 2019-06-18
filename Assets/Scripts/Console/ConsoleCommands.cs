using UnityEngine.SceneManagement;
using IngameDebugConsole;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Utils;
using Mahjong.Logic;
using Mahjong.Model;
using GamePlay.Server.Controller;
using Photon.Pun;

namespace Console
{
    public static class ConsoleCommands
    {
        [ConsoleMethod("change_scene", "Change to the given scene")]
        public static void ChangeScene(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        [ConsoleMethod("tiles", "Parse string of tiles")]
        public static IList<Tile> ParseTiles(string handString)
        {
            var rx = new Regex(@"((\d+)([mpsz]))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var tiles = new List<Tile>();
            var matches = rx.Matches(handString);
            foreach (Match match in matches)
            {
                var groups = match.Groups;
                var digits = groups[2].Value;
                var suitString = groups[3].Value;
                var suit = (Suit)Enum.Parse(typeof(Suit), suitString.ToUpper());
                for (int i = 0; i < digits.Length; i++)
                {
                    int rank = digits[i] - '0';
                    if (rank < 0 || rank > 9)
                    {
                        Debug.LogError($"Recognized rank {rank} is invalid.");
                        continue;
                    }
                    if (rank != 0) tiles.Add(new Tile(suit, rank));
                    else tiles.Add(new Tile(suit, 5, true));
                }
            }
            Debug.Log($"Parsed tiles: {string.Join("", tiles)}");
            return tiles;
        }

        [ConsoleMethod("point", "Point for the given tiles")]
        public static PointInfo GetPointInfo(string handString, string winningString, bool richi, bool tsumo, bool isQTJ)
        {
            var handStatus = HandStatus.Menqing;
            if (richi) handStatus |= HandStatus.Richi;
            if (tsumo) handStatus |= HandStatus.Tsumo;
            var roundStatus = new RoundStatus();
            var tiles = ParseTiles(handString);
            var winningTiles = ParseTiles(winningString);
            var gameSettings = new GameSetting();
            var point = MahjongLogic.GetPointInfo(tiles.ToArray(), new Meld[0], winningTiles[0],
                handStatus, roundStatus, gameSettings, isQTJ);
            return point;
        }

        [ConsoleMethod("alter_yama", "Set the upcoming tiles in current mahjong set. If game is not started, this command has no effect")]
        public static bool SetTiles(string tileString)
        {
            var tiles = ParseTiles(tileString);
            var fieldInfo = typeof(ServerBehaviour).GetField("mahjongSet", BindingFlags.NonPublic | BindingFlags.Instance);
            var server = ServerBehaviour.Instance;
            if (server == null)
            {
                Debug.LogError("Game is not started, or you are not host.");
                return false;
            }
            var set = fieldInfo.GetValue(server) as MahjongSet;
            if (set == null)
            {
                Debug.LogError("MahjongSet is null");
                return false;
            }
            var methodInfo = typeof(MahjongSet).GetMethod("SetTiles", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(set, new[] { tiles });
            return true;
        }

        [ConsoleMethod("alter_yama_reverse", "Set the upcoming lingshang tiles in current mahjong set. If game is not started, this command has no effect")]
        public static bool ReplaceLingShang(string tileString)
        {
            var tiles = ParseTiles(tileString);
            var fieldInfo = typeof(ServerBehaviour).GetField("mahjongSet", BindingFlags.NonPublic | BindingFlags.Instance);
            var server = ServerBehaviour.Instance;
            if (server == null)
            {
                Debug.LogError("Game is not started, or you are not host.");
                return false;
            }
            var set = fieldInfo.GetValue(server) as MahjongSet;
            if (set == null)
            {
                Debug.LogError("MahjongSet is null");
                return false;
            }
            var methodInfo = typeof(MahjongSet).GetMethod("SetTilesReverse", BindingFlags.NonPublic | BindingFlags.Instance);
            methodInfo.Invoke(set, new[] { tiles });
            return true;
        }

        [ConsoleMethod("alter_hand", "Alter hand tiles on server")]
        public static bool AlterHand(int playerIndex, string handString)
        {
            var tiles = ParseTiles(handString);
            var server = ServerBehaviour.Instance;
            if (server == null)
            {
                Debug.LogError("Game is not started, or you are not host.");
                return false;
            }
            var status = server.CurrentRoundStatus;
            if (status == null)
            {
                Debug.LogError("Game is not started, or you are not host.");
                return false;
            }
            var fieldInfo = status.GetType().GetField("handTiles", BindingFlags.NonPublic | BindingFlags.Instance);
            var handTileArray = (List<Tile>[])fieldInfo.GetValue(status);
            var handTiles = handTileArray[playerIndex];
            for (int i = 0; i < Math.Min(tiles.Count, handTiles.Count); i++)
            {
                handTiles[i] = tiles[i];
            }
            status.SortHandTiles();
            return true;
        }

        [ConsoleMethod("test_richi", "Given a hand of tiles, test if it can claim richi")]
        public static bool TestRichi(string tileString, string lastDraw)
        {
            var tiles = ParseTiles(tileString);
            var tile = ParseTiles(lastDraw)[0];
            IList<Tile> availables;
            var result = MahjongLogic.TestRichi(tiles, new List<Meld>(), tile, false, out availables);
            Debug.Log($"Candidates: {string.Join(",", availables)}");
            return result;
        }

        [ConsoleMethod("print_round", "Print round status to log")]
        public static void PrintRoundStatus()
        {
            var server = ServerBehaviour.Instance;
            if (server == null)
            {
                Debug.LogError("Game is not started, or you are not host.");
                return;
            }
            var currentRoundStatus = server.CurrentRoundStatus;
            if (currentRoundStatus == null)
            {
                Debug.LogError("Game is not started.");
                return;
            }
            Debug.Log($"Current round status: \n{currentRoundStatus}");
        }

        [ConsoleMethod("show_setting", "Show current setting")]
        public static void ShowSetting()
        {
            var server = ServerBehaviour.Instance;
            if (server == null)
            {
                Debug.Log("Server setting: null");
            }
            else
            {
                Debug.Log($"Server setting: {server.GameSettings.ToJson(true)}");
            }
        }

        [ConsoleMethod("ping", "Show ping to the photon server")]
        public static int Ping()
        {
            return PhotonNetwork.GetPing();
        }
    }
}
