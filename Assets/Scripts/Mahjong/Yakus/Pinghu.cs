using System.Linq;

namespace Mahjong.Yakus
{
    public class Pinghu : Yaku
    {
        public string Name
        {
            get { return "平胡"; }
        }

        public int Value
        {
            get { return 1; }
        }

        public bool IsYakuMan
        {
            get { return false; }
        }

        public bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options)
        {
            if (!options.Contains(YakuOption.Menqing)) return false;
            int shunziCount = 0;
            bool twoSide = false;
            foreach (Mianzi mianzi in hand)
            {
                if (mianzi.Type == MianziType.Shunzi)
                {
                    shunziCount++;
                    if (mianzi.First.Equals(rong) || mianzi.Last.Equals(rong)) twoSide = true;
                }
            }

            return shunziCount == 4 && twoSide;
        }
    }
}