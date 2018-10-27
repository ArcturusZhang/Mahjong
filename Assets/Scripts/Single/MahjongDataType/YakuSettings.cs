using UnityEngine;

namespace Single.MahjongDataType
{
    [CreateAssetMenu(menuName = "Mahjong/YakuData")]
    public class YakuSettings : ScriptableObject
    {
        [Header("Yaku settings")] public const int YakumanBaseFan = 13;
        public bool 允许食断 = true;
        public bool 连风对子额外加符 = true;
        [Range(1, 2)] public int 四暗刻单骑 = 2;
        [Range(1, 2)] public int 国士无双十三面 = 2;
        [Range(1, 2)] public int 纯正九连宝灯 = 2;
        [Range(1, 2)] public int 纯绿一色 = 2;
        public bool 青天井 = false;
    }
}