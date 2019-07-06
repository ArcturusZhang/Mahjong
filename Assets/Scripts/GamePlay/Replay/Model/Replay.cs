using System.Collections.Generic;
using Mahjong.Model;

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

        public void NewRound(int oya, int field, int extra, int richiSticks, int dice)
        {
            var round = new Round.Builder().SetTotalPlayers(totalPlayers).SetOya(oya).SetField(field)
                .SetExtra(extra).SetRichiSticks(richiSticks).SetDice(dice)
                .Build();
            rounds.Add(round);
        }
    }

    [System.Serializable]
    public class Round
    {
        private int totalPlayers;
        private int oya;
        private int field;
        private int extra;
        private int richiSticks;
        private int dice;
        private int kongClaimed;
        private List<Turn> turns;
        private List<Tile>[] handTiles;
        public Round()
        {
            kongClaimed = 0;
        }

        public class Builder
        {
            private int totalPlayers;
            private int oya;
            private int field;
            private int extra;
            private int richiSticks;
            private int dice;
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
            public Round Build()
            {
                var round = new Round();
                round.totalPlayers = this.totalPlayers;
                round.oya = this.oya;
                round.field = this.field;
                round.extra = this.extra;
                round.richiSticks = this.richiSticks;
                round.dice = this.dice;
                return round;
            }
        }
    }

    [System.Serializable]
    public class Turn
    {

    }
}