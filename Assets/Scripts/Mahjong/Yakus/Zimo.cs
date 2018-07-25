using System.Linq;

namespace Mahjong.Yakus
{
    public class Zimo : Yaku
    {
        public string Name
        {
            get { return "门前清自摸和"; }
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
            return options.Contains(YakuOption.Menqing) && options.Contains(YakuOption.Zimo);
        }
    }
}