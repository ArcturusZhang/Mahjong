using System.Text;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine.Networking;
using System.Linq;

namespace Multi.MahjongMessages
{
    // Server to client messages
    public class ServerGamePrepareMessage : MessageBase
    {
        public int TotalPlayers;
        public int PlayerIndex;
        public int[] Points;
        public string[] PlayerNames;
        public NetworkSettings Settings;

        public override string ToString()
        {
            return $"TotalPlayers: {TotalPlayers}\n"
                + $"PlayerIndex: {PlayerIndex}\n"
                + $"Points: {string.Join(", ", Points)}\n"
                + $"PlayerNames: {string.Join(", ", PlayerNames)}";
        }
    }

    public class ServerRoundStartMessage : MessageBase
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

    public class ServerDrawTileMessage : MessageBase
    {
        public int PlayerIndex;
        public Tile Tile;
        public int BonusTurnTime;
        public bool Richied;
        public InTurnOperation[] Operations;
        public MahjongSetData MahjongSetData;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"Tile: {Tile}\n"
                + $"BonusTurnTime: {BonusTurnTime}\n"
                + $"Richied: {Richied}\n"
                + $"Operations: {string.Join(",", Operations)}\n"
                + $"MahjongSetData: {MahjongSetData}";
        }
    }

    public class ServerOtherDrawTileMessage : MessageBase
    {
        public int PlayerIndex;
        public int CurrentTurnPlayerIndex;
        public MahjongSetData MahjongSetData;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"CurrentTurnPlayerIndex: {CurrentTurnPlayerIndex}\n"
                + $"MahjongSetData: {MahjongSetData}";
        }
    }

    public class ServerKongMessage : MessageBase
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

    public class ServerDiscardOperationMessage : MessageBase
    {
        public int PlayerIndex;
        public int CurrentTurnPlayerIndex;
        public bool IsRichiing;
        public bool DiscardingLastDraw;
        public Tile Tile;
        public int BonusTurnTime;
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
                + $"Operations: {string.Join(",", Operations)}\n"
                + $"HandTiles: {string.Join("", HandTiles)}";
        }
    }

    public class ServerTurnEndMessage : MessageBase
    {
        public int PlayerIndex;
        public OutTurnOperationType ChosenOperationType;
        public OutTurnOperation[] Operations;
        public int[] Points;
        public bool[] RichiStatus;
        public int RichiSticks;
        public MahjongSetData MahjongSetData;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"ChosenOperationType: {ChosenOperationType}\n"
                + $"Operations for each player: {string.Join(", ", Operations)}\n"
                + $"Points: {string.Join(",", Points)}\n"
                + $"RichiStatus: {string.Join(",", RichiStatus)}\n"
                + $"RichiSticks: {RichiSticks}\n"
                + $"MahjongSetData: {MahjongSetData}";
        }
    }

    public class ServerRoundDrawMessage : MessageBase
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
    public class ServerPlayerTsumoMessage : MessageBase
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

    public class ServerPlayerRongMessage : MessageBase
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

    public class ServerPointTransferMessage : MessageBase
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

    public class ServerGameEndMessage : MessageBase
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

    // Client to server messages
    public class ClientReadinessMessage : MessageBase
    {
        public int PlayerIndex;
        public int Content;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"Content: {Content}";
        }
    }

    public class ClientDiscardRequestMessage : MessageBase
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

    public class ClientInTurnOperationMessage : MessageBase
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

    public class ClientOutTurnOperationMessage : MessageBase
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

    public class ClientRoundDrawMessage : MessageBase
    {
        public int PlayerIndex;
        public RoundDrawType Type;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}, Type: {Type}";
        }
    }

    public class ClientNextRoundMessage : MessageBase
    {
        public int PlayerIndex;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}";
        }
    }
}