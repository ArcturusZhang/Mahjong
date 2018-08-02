using System;
using System.Collections.Generic;

namespace Mahjong.YakuUtils
{
    public class GameStatus
    {
        public Tile Changfeng { get; private set; }
        public Tile Zifeng { get; private set; }
        public List<Tile> Dora { get; private set; }

        public GameStatus() : this(new Tile(Suit.Z, 1), new Tile(Suit.Z, 1))
        {
        }

        public GameStatus(Tile changfeng, Tile zifeng, params Tile[] dora)
        {
            if (changfeng.Suit != Suit.Z || changfeng.Index > 4) throw new ArgumentException("场风只能是东、南、西、北其中之一");
            if (zifeng.Suit != Suit.Z || zifeng.Index > 4) throw new ArgumentException("自风只能是东、南、西、北其中之一");
            Changfeng = changfeng;
            Zifeng = zifeng;
            Dora = new List<Tile>();
            if (dora != null)
                foreach (var tile in dora)
                {
                    Dora.Add(tile);
                }
        }
    }
}