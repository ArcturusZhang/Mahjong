using System.Text;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine.Networking;
using System.Linq;

namespace Multi.MahjongMessages
{
    // Server to client messages
    // public class ServerGameSyncMessage : MessageBase
    // {
    //     // todo
    // }
    public class ServerGamePrepareMessage : MessageBase
    {
        public int TotalPlayers;
        public int PlayerIndex;
        public int[] Points;
        public string[] PlayerNames;
        public GameSettings GameSettings;
        public YakuSettings YakuSettings;

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
        public InTurnOperationType Operations;
        public MahjongSetData MahjongSetData;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"Tile: {Tile}\n"
                + $"BonusTurnTime: {BonusTurnTime}\n"
                + $"Operations: {Operations}\n"
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

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}\n"
                + $"ChosenOperationType: {ChosenOperationType}\n"
                + $"Operations for each player: {string.Join(", ", Operations)}";
        }
    }

    public class ServerRoundDrawMessage : MessageBase
    {
        public WaitingData[] WaitingData;

        public override string ToString()
        {
            var list = WaitingData.Select((t, i) =>
                $"Player {i}: \nHandTiles: {string.Join("", t.HandTiles)}, Waiting: {string.Join("", t.WaitingTiles)}");
            return string.Join("\n", list);
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

    public class ClientOperationMessage : MessageBase
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
}