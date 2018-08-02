using System;
using System.Collections.Generic;

namespace Mahjong.YakuUtils
{
    public class GameStatus
    {
        public Tile Changfeng { get; }
        public Tile Zifeng { get; }
        public IEnumerable<Tile> Dora { get; }
        public IEnumerable<Tile> UraDora { get; }

        public GameStatus() : this(new Tile(Suit.Z, 1), new Tile(Suit.Z, 1), new Tile[0], new Tile[0])
        {
        }

        public GameStatus(Tile changfeng, Tile zifeng, Tile[] dora, Tile[] uraDora)
        {
            if (changfeng.Suit != Suit.Z || changfeng.Index > 4) throw new ArgumentException("场风只能是东、南、西、北其中之一");
            if (zifeng.Suit != Suit.Z || zifeng.Index > 4) throw new ArgumentException("自风只能是东、南、西、北其中之一");
            Changfeng = changfeng;
            Zifeng = zifeng;
            var list = new List<Tile>();
            if (dora != null) list.AddRange(dora);

            Dora = list;
            list = new List<Tile>();
            if (uraDora != null) list.AddRange(uraDora);

            UraDora = list;
        }
    }
}