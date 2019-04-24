using UnityEngine.SceneManagement;
using IngameDebugConsole;
using UnityEngine;
using Lobby;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Single.MahjongDataType;
using System;
using Single;
using System.Linq;
using Single.UI;
using System.IO;
using Multi;
using System.Reflection;

namespace Console
{
    public static class ConsoleCommands
    {
        [ConsoleMethod("change_scene", "Change to the given scene")]
        public static void ChangeScene(string scene)
        {
            SceneManager.LoadScene(scene);
        }

        [ConsoleMethod("close", "Set object inactive")]
        public static void Close(string target)
        {
            switch (target)
            {
                case "lobby":
                    CloseLobbyCanvas();
                    return;
                default:
                    var obj = GameObject.Find(target);
                    obj?.SetActive(false);
                    return;
            }
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
            var yakuSettings = LobbyManager.Instance.YakuSettings;
            var point = MahjongLogic.GetPointInfo(tiles.ToArray(), new Meld[0], winningTiles[0],
                handStatus, roundStatus, yakuSettings, isQTJ);
            return point;
        }

        [ConsoleMethod("summary", "Show summary panel")]
        public static void ShowSummary(string handString, string winningString, bool richi, bool tsumo, bool oya, bool isQTJ)
        {
            var handTiles = ParseTiles(handString);
            var winningTile = ParseTiles(winningString)[0];
            var info = new PlayerHandInfo
            {
                HandTiles = handTiles,
                OpenMelds = new List<Meld>(),
                WinningTile = winningTile,
                DoraIndicators = null,
                UraDoraIndicators = null,
                IsTsumo = tsumo
            };
            var pointInfo = GetPointInfo(handString, winningString, richi, tsumo, isQTJ);
            var pointSummaryPanelManager = GameObject.FindObjectOfType<PointSummaryPanelManager>();
            var data = new SummaryPanelData
            {
                HandInfo = info,
                PointInfo = pointInfo,
                Multiplier = oya ? 6 : 4,
                PlayerName = "Console"
            };
            pointSummaryPanelManager.ShowPanel(data, () => Debug.Log("Time is up!"));
        }

        [ConsoleMethod("save_settings", "Save game settings to file")]
        public static void SaveSettings(string which)
        {
            ScriptableObject settings;
            string filepath;
            Debug.Log($"Saving settings {which}");
            switch (which)
            {
                case "game":
                    settings = LobbyManager.Instance.GameSettings;
                    filepath = Application.persistentDataPath + "/GameSettings.json";
                    break;
                case "yaku":
                    settings = LobbyManager.Instance.YakuSettings;
                    filepath = Application.persistentDataPath + "/YakuSettings.json";
                    break;
                default:
                    Debug.LogError($"Unknown setting key: {which}");
                    return;
            }
            var json = JsonUtility.ToJson(settings, true);
            var writer = new StreamWriter(filepath);
            writer.WriteLine(json);
            writer.Close();
        }

        [ConsoleMethod("load_settings", "Read game settings from json")]
        public static void LoadSettings(string which)
        {
            ScriptableObject settings;
            string filepath;
            Debug.Log($"Loading settings {which}");
            switch (which)
            {
                case "game":
                    settings = LobbyManager.Instance.GameSettings;
                    filepath = Application.persistentDataPath + "/GameSettings.json";
                    break;
                case "yaku":
                    settings = LobbyManager.Instance.YakuSettings;
                    filepath = Application.persistentDataPath + "/YakuSettings.json";
                    break;
                default:
                    Debug.LogError($"Unknown setting key: {which}");
                    return;
            }
            var reader = new StreamReader(filepath);
            var json = reader.ReadToEnd();
            JsonUtility.FromJsonOverwrite(json, settings);
        }

        [ConsoleMethod("set_tiles", "Set the upcoming tiles in current mahjong set. If game is not started, this command has no effect")]
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

        [ConsoleMethod("set_tiles_reverse", "Set the upcoming lingshang tiles in current mahjong set. If game is not started, this command has no effect")]
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

        private static void CloseLobbyCanvas()
        {
            var lobby = LobbyManager.Instance;
            var canvas = lobby?.GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }
    }
}
