using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Yakus
{
    public class Erbeikou : Yaku
    {
        public override string Name
        {
            get { return "二杯口"; }
        }

        public override int Value
        {
            get { return 3; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) return false;
            if (hand.Count <= 4) return false;
            var dict = new Dictionary<Mianzi, int>();
            foreach (var mianzi in hand)
            {
                if (mianzi.Type != MianziType.Shunzi) continue;
                if (!dict.ContainsKey(mianzi)) dict.Add(mianzi, 0);
                dict[mianzi]++;
            }

            int count = 0;
            foreach (var entry in dict)
            {
                if (entry.Value == 4) return true;
                if (entry.Value >= 2) count++;
            }
            return count >= 2;
        }
    }
}