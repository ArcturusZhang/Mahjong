using System.Collections.Generic;
using System.IO;
using Common.Interfaces;
using GamePlay.Server.Model;
using Mahjong.Model;
using Utils;

namespace GamePlay.Replay.Model
{
    [System.Serializable]
    public class Replay
    {
        private GameSetting setting;
        private int totalPlayers;
        private string[] playerNames;
        private List<Round> rounds;
        public Replay(GameSetting setting, string[] playerNames)
        {
            this.setting = setting;
            this.totalPlayers = setting.MaxPlayer;
            this.playerNames = playerNames;
            rounds = new List<Round>();
        }
        public void AddRound(Round round)
        {
            rounds.Add(round);
        }
        public void Save(string path)
        {
            Save(this, path);
        }
        public static void Save(Replay replay, string path)
        {
            var bytes = SerializeUtility.SerializeObject(replay);
            File.WriteAllBytes(path, bytes);
        }
        public static Replay Load(string path)
        {
            var bytes = File.ReadAllBytes(path);
            return (Replay)SerializeUtility.DeserializeObject(bytes);
        }
    }

    [System.Serializable]
    public class Round
    {
        public int totalPlayers { get; private set; }
        public int oya { get; private set; }
        public int field { get; private set; }
        public int extra { get; private set; }
        public int richiSticks { get; private set; }
        public int dice { get; private set; }
        public int kongClaimed { get; private set; }
        public IList<Tile> allTiles { get; private set; }
        public List<Turn> turns { get; private set; }
        public List<Tile>[] handTiles { get; private set; }
        public Round() { }
        public void SetHandTiles(int playerIndex, List<Tile> playerHandTiles)
        {
            handTiles[playerIndex] = playerHandTiles;
        }
        public void AddTurn(Turn turn)
        {
            turns.Add(turn);
        }
        public class Builder : IBuilder<Round>
        {
            private int totalPlayers;
            private int oya;
            private int field;
            private int extra;
            private int richiSticks;
            private int dice;
            private int kongClaimed;
            private IList<Tile> allTiles;
            public Builder SetTotalPlayers(int totalPlayers)
            {
                this.totalPlayers = totalPlayers;
                return this;
            }
            public Builder SetOya(int oya)
            {
                this.oya = oya;
                return this;
            }
            public Builder SetField(int field)
            {
                this.field = field;
                return this;
            }
            public Builder SetExtra(int extra)
            {
                this.extra = extra;
                return this;
            }
            public Builder SetRichiSticks(int richiSticks)
            {
                this.richiSticks = richiSticks;
                return this;
            }
            public Builder SetDice(int dice)
            {
                this.dice = dice;
                return this;
            }
            public Builder SetKongClaimed(int kongClaimed)
            {
                this.kongClaimed = kongClaimed;
                return this;
            }
            public Builder SetAllTiles(IList<Tile> allTiles)
            {
                this.allTiles = allTiles;
                return this;
            }
            public Round Build()
            {
                return new Round
                {
                    totalPlayers = totalPlayers,
                    oya = oya,
                    field = field,
                    extra = extra,
                    richiSticks = richiSticks,
                    dice = dice,
                    kongClaimed = kongClaimed,
                    allTiles = allTiles,
                    turns = new List<Turn>(),
                    handTiles = new List<Tile>[totalPlayers]
                };
            }
        }
    }

    [System.Serializable]
    public class Turn
    {
        public Type type { get; private set; }
        public Tile drawTile { get; private set; }
        public Tile discardTile { get; private set; }
        public InTurnOperation inTurnOperation { get; private set; }
        public OutTurnOperation outTurnOperation { get; private set; }
        public Turn() { }

        public class Builder : IBuilder<Turn>
        {
            private Type type;
            private Tile drawTile;
            private Tile discardTile;
            private InTurnOperation inTurnOperation;
            private OutTurnOperation outTurnOperation;
            public Builder SetType(Type type)
            {
                this.type = type;
                return this;
            }
            public Builder SetDrawTile(Tile drawTile)
            {
                this.drawTile = drawTile;
                return this;
            }
            public Builder SetDiscardTile(Tile discardTile)
            {
                this.discardTile = discardTile;
                return this;
            }
            public Builder SetInTurnOperation(InTurnOperation inTurnOperation)
            {
                this.inTurnOperation = inTurnOperation;
                return this;
            }
            public Builder SetOutTurnOperation(OutTurnOperation outTurnOperation)
            {
                this.outTurnOperation = outTurnOperation;
                return this;
            }
            public Turn Build()
            {
                return new Turn
                {
                    type = type,
                    drawTile = drawTile,
                    discardTile = discardTile,
                    inTurnOperation = inTurnOperation,
                    outTurnOperation = outTurnOperation
                };
            }
        }
        public enum Type
        {
            Normal, Extra, End
        }
    }
}