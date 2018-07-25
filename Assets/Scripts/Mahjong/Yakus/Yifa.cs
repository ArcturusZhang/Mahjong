using System.Linq;

namespace Mahjong.Yakus
{
    public class Yifa : Yaku
    {
        public string Name
        {
            get { return "一发"; }
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
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Lizhi) &&
                   options.Contains(YakuOption.Yifa);
        }
    }
}