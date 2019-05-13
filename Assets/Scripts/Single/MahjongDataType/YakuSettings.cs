using UnityEngine;

namespace Single.MahjongDataType
{
    [CreateAssetMenu(menuName = "Mahjong/YakuData")]
    public class YakuSettings : ScriptableObject
    {
        [Header("Yaku settings")] public const int YakumanBaseFan = 13;
        public bool OpenDuanYao = true;
        public bool HasOneShot = true;
        public bool 连风对子额外加符 = true;
        public bool AllowGswsRobConcealedKong = true;
        public YakumanLevel SiAnKe = YakumanLevel.Two;
        public YakumanLevel GuoShi = YakumanLevel.Two;
        public YakumanLevel JiuLian = YakumanLevel.Two;
        public YakumanLevel LvYiSe = YakumanLevel.One;
        public int 四暗刻单骑 => YakumanLevelToInt(SiAnKe);
        public int 国士无双十三面 => YakumanLevelToInt(GuoShi);
        public int 纯正九连宝灯 => YakumanLevelToInt(JiuLian);
        public int 纯绿一色 => YakumanLevelToInt(LvYiSe);

        private static int YakumanLevelToInt(YakumanLevel level)
        {
            switch (level)
            {
                case YakumanLevel.One:
                    return 1;
                case YakumanLevel.Two:
                    return 2;
                default:
                    throw new System.ArgumentException($"Unknown level {level}");
            }
        }
    }

    public enum YakumanLevel
    {
        One, Two
    }
}