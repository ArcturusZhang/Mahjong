using System;
using System.Linq;
using System.Reflection;
using ExitGames.Client.Photon;
using Mahjong.Model;
using Photon.Realtime;
using Utils;

namespace GamePlay.Server.Model.Events
{
    public static class EventMessages
    {
        public static readonly RaiseEventOptions ToMaster = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        public static readonly RaiseEventOptions ToAll = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        public static readonly RaiseEventOptions ToOther = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        public static readonly SendOptions SendReliable = SendOptions.SendReliable;
        public static readonly SendOptions SendUnreliable = SendOptions.SendUnreliable;
        public const byte LoadCompleteEvent = (byte)0;
        public const byte ClientReadyEvent = (byte)1;
        public const byte DiscardTileEvent = (byte)2;
        public const byte InTurnOperationEvent = (byte)3;
        public const byte OutTurnOperationEvent = (byte)4;
        public const byte NextRoundEvent = (byte)5;

        [Serializable]
        public struct GamePrepareInfo
        {
            public int PlayerIndex;
            public int[] Points;
            public string[] PlayerNames;
            public string GameSetting;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"Points: {string.Join(", ", Points)}\n"
                    + $"PlayerNames: {string.Join(", ", PlayerNames)}\n"
                    + $"GameSettings: {GameSetting}";
            }
        }

        [Serializable]
        public struct RoundStartInfo
        {
            public int PlayerIndex;
            public int Field;
            public int Dice;
            public int Extra;
            public int RichiSticks;
            public int OyaPlayerIndex;
            public int[] Points;
            public Tile[] InitialHandTiles;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"Field: {Field}\n"
                    + $"Dice: {Dice}\n"
                    + $"Extra: {Extra}\n"
                    + $"RichiSticks: {RichiSticks}\n"
                    + $"OyaPlayerIndex: {OyaPlayerIndex}\n"
                    + $"Points: {string.Join(", ", Points)}\n"
                    + $"InitialHandTiles: {string.Join("", InitialHandTiles)}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct DrawTileInfo
        {
            public int PlayerIndex;
            public int DrawPlayerIndex;
            public Tile Tile;
            public int BonusTurnTime;
            public bool Zhenting;
            public InTurnOperation[] Operations;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                var operationString = Operations == null ? "null" : string.Join(",", Operations);
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"DrawPlayerIndex: {DrawPlayerIndex}\n"
                    + $"Tile: {Tile}\n"
                    + $"BonusTurnTime: {BonusTurnTime}\n"
                    + $"Zhenting: {Zhenting}\n"
                    + $"Operations: {operationString}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct DiscardTileInfo
        {
            public int PlayerIndex;
            public bool IsRichiing;
            public bool DiscardingLastDraw;
            public Tile Tile;
            public int BonusTurnTime;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"IsRichiing: {IsRichiing}\n"
                    + $"DiscardingLastDraw: {DiscardingLastDraw}\n"
                    + $"Tile: {Tile}\n"
                    + $"BonusTurnTime: {BonusTurnTime}";
            }
        }

        [Serializable]
        public struct InTurnOperationInfo
        {
            public int PlayerIndex;
            public InTurnOperation Operation;
            public int BonusTurnTime;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"Operation: {Operation}\n"
                    + $"BonusTurnTime: {BonusTurnTime}";
            }
        }

        [Serializable]
        public struct OutTurnOperationInfo
        {
            public int PlayerIndex;
            public OutTurnOperation Operation;
            public int BonusTurnTime;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"Operation: {Operation}\n"
                    + $"BonusTurnTime: {BonusTurnTime}";
            }
        }

        [Serializable]
        public struct DiscardOperationInfo
        {
            public int PlayerIndex;
            public int CurrentTurnPlayerIndex;
            public bool IsRichiing;
            public bool DiscardingLastDraw;
            public Tile Tile;
            public int BonusTurnTime;
            public bool Zhenting;
            public OutTurnOperation[] Operations;
            public Tile[] HandTiles;
            public RiverData[] Rivers;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"CurrentTurnPlayerIndex: {CurrentTurnPlayerIndex}\n"
                    + $"IsRichiing: {IsRichiing}\n"
                    + $"DiscardingLastDraw: {DiscardingLastDraw}\n"
                    + $"Tile: {Tile}\n"
                    + $"BonusTurnTime: {BonusTurnTime}\n"
                    + $"Zhenting: {Zhenting}\n"
                    + $"Operations: {string.Join(",", Operations)}\n"
                    + $"HandTiles: {string.Join("", HandTiles)}";
            }
        }

        [Serializable]
        public struct TurnEndInfo
        {
            public int PlayerIndex;
            public OutTurnOperationType ChosenOperationType;
            public OutTurnOperation[] Operations;
            public int[] Points;
            public bool[] RichiStatus;
            public int RichiSticks;
            public bool Zhenting;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"ChosenOperationType: {ChosenOperationType}\n"
                    + $"Operations for each player: {string.Join(", ", Operations)}\n"
                    + $"Points: {string.Join(",", Points)}\n"
                    + $"RichiStatus: {string.Join(",", RichiStatus)}\n"
                    + $"RichiSticks: {RichiSticks}\n"
                    + $"Zhenting: {Zhenting}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct OperationPerformInfo
        {
            public int PlayerIndex;
            public int OperationPlayerIndex;
            public OutTurnOperation Operation;
            public PlayerHandData HandData;
            public int BonusTurnTime;
            public RiverData[] Rivers;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}\n"
                    + $"OperationPlayerIndex: {OperationPlayerIndex}\n"
                    + $"Operation: {Operation}\n"
                    + $"HandData: {HandData}\n"
                    + $"BonusTurnTime: {BonusTurnTime}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct TsumoInfo
        {
            public int TsumoPlayerIndex;
            public string TsumoPlayerName;
            public PlayerHandData TsumoHandData;
            public Tile WinningTile;
            public Tile[] DoraIndicators;
            public Tile[] UraDoraIndicators;
            public bool IsRichi;
            public NetworkPointInfo TsumoPointInfo;
            public int TotalPoints;

            public override string ToString()
            {
                return $"TsumoPlayerIndex: {TsumoPlayerIndex}\n"
                    + $"HandData: {TsumoHandData}\n"
                    + $"WinningTile: {WinningTile}\n"
                    + $"DoraIndicators: {string.Join("", DoraIndicators)}\n"
                    + $"UraDoraIndicators: {string.Join("", UraDoraIndicators)}\n"
                    + $"IsRichi: {IsRichi}\n"
                    + $"TsumoPointSummary: {TsumoPointInfo}\n"
                    + $"TotalPoints: {TotalPoints}\n";
            }
        }

        [Serializable]
        public struct RongInfo
        {
            public int[] RongPlayerIndices;
            public string[] RongPlayerNames;
            public PlayerHandData[] HandData;
            public Tile WinningTile;
            public Tile[] DoraIndicators;
            public Tile[] UraDoraIndicators;
            public bool[] RongPlayerRichiStatus;
            public NetworkPointInfo[] RongPointInfos;
            public int[] TotalPoints;

            public override string ToString()
            {
                return $"PlayerIndex: {string.Join(",", RongPlayerIndices)}\n"
                    + $"PlayerName: {string.Join(",", RongPlayerNames)}\n"
                    + $"HandData: {string.Join(",", HandData)}\n"
                    + $"WinningTile: {WinningTile}\n"
                    + $"DoraIndicators: {string.Join("", DoraIndicators)}\n"
                    + $"UraDoraIndicators: {string.Join("", UraDoraIndicators)}\n"
                    + $"RichiStatus: {string.Join(",", RongPlayerRichiStatus)}\n"
                    + $"PointSummaries: {string.Join(";", RongPointInfos)}\n"
                    + $"TotalPoints: {string.Join(",", TotalPoints)}";
            }
        }

        [Serializable]
        public struct KongInfo
        {
            public int PlayerIndex;
            public int KongPlayerIndex;
            public PlayerHandData HandData;
            public int BonusTurnTime;
            public OutTurnOperation[] Operations;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}, KongPlayerIndex: {KongPlayerIndex}\n"
                    + $"HandData: {HandData}, Operations: {string.Join("|", Operations)}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct RoundDrawInfo
        {
            public RoundDrawType RoundDrawType;
            public WaitingData[] WaitingData;

            public override string ToString()
            {
                if (WaitingData == null) return $"RoundDrawType: {RoundDrawType}";
                var list = WaitingData.Select((t, i) =>
                    $"Player {i}: \nHandTiles: {string.Join("", t.HandTiles)}, Waiting: {string.Join("", t.WaitingTiles)}");
                return string.Join("\n", list);
            }
        }

        [Serializable]
        public struct BeiDoraInfo
        {
            public int PlayerIndex;
            public int BeiDoraPlayerIndex;
            public int[] BeiDoras;
            public PlayerHandData HandData;
            public int BonusTurnTime;
            public OutTurnOperation[] Operations;
            public MahjongSetData MahjongSetData;

            public override string ToString()
            {
                return $"PlayerIndex: {PlayerIndex}, BeiDoraPlayerIndex: {BeiDoraPlayerIndex}\n"
                    + $"BeiDoras: {string.Join(",", BeiDoras)}\n"
                    + $"HandData: {HandData}, Operations: {string.Join("|", Operations)}\n"
                    + $"MahjongSetData: {MahjongSetData}";
            }
        }

        [Serializable]
        public struct PointTransferInfo
        {
            public string[] PlayerNames;
            public int[] Points;
            public PointTransfer[] PointTransfers;

            public override string ToString()
            {
                return $"PlayerNames: {string.Join(",", PlayerNames)}\n"
                    + $"Points: {string.Join(",", Points)}\n"
                    + $"Transfers: {string.Join("|", PointTransfers)}";
            }
        }

        [Serializable]
        public struct GameEndInfo
        {
            public string[] PlayerNames;
            public int[] Points;
            public int[] Places;

            public override string ToString()
            {
                return $"PlayerNames: {string.Join(",", PlayerNames)}\n"
                    + $"Points: {string.Join(",", Points)}\n"
                    + $"Places: {string.Join("|", Places)}";
            }
        }
    }
}