using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Yakus
{
    public class Yibeikou : Yaku
    {
        public override string Name
        {
            get { return "一杯口"; }
        }
        
        public override int Value
        {
            get { return 1; }
        }

        public override YakuType Type
        {
            get { return YakuType.Menqian; }
        }

        public override bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) return false;
            IDictionary<Mianzi, int> dict = new Dictionary<Mianzi, int>();
            foreach (Mianzi mianzi in hand)
            {
                if (dict.ContainsKey(mianzi)) dict[mianzi]++;
                else dict.Add(mianzi, 1);
                if (dict[mianzi] == 2) return true;
            }

            return false;
        }
    }
}