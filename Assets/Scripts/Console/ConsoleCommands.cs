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
        public static PointInfo GetPointInfo(string handString, string winningString, bool richi, bool tsumo)
        {
            var handStatus = HandStatus.Menqing;
            if (richi) handStatus |= HandStatus.Richi;
            if (tsumo) handStatus |= HandStatus.Tsumo;
            var roundStatus = new RoundStatus();
            var tiles = ParseTiles(handString);
            var winningTiles = ParseTiles(winningString);
            var yakuSettings = LobbyManager.Instance.YakuSettings;
            var point = MahjongLogic.GetPointInfo(tiles.ToArray(), new Meld[0], winningTiles[0],
                handStatus, roundStatus, yakuSettings);
            return point;
        }

        [ConsoleMethod("summary", "Show summary panel")]
        public static void ShowSummary(string handString, string winningString, bool richi, bool tsumo, bool oya)
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
            var pointInfo = GetPointInfo(handString, winningString, richi, tsumo);
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

        private static void CloseLobbyCanvas()
        {
            var lobby = LobbyManager.Instance;
            var canvas = lobby?.GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = false;
        }

        private static IDictionary<string, T> GetEnums<T>() where T : Enum
        {
            var dict = new Dictionary<string, T>();
            foreach (string name in Enum.GetNames(typeof(T)))
            {
                if (dict.ContainsKey(name)) continue;
                dict.Add(name, (T)Enum.Parse(typeof(T), name));
            }
            return dict;
        }
    }
}
