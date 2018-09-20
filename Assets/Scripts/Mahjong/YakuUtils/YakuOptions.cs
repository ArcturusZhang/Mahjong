using System;

namespace Mahjong.YakuUtils
{
    [Flags]
    public enum YakuOptions
    {
        Lizhi = 1 << 0,
        Menqing = 1 << 1,
        Yifa = 1 << 2,
        Zimo = 1 << 3,
        Haidi = 1 << 4,
        Lingshang = 1 << 5,
        Qianggang = 1 << 6,
        FirstRound = 1 << 7,
        Zhuangjia = 1 << 8,
        Qingtianjing = 1 << 9
    }
}