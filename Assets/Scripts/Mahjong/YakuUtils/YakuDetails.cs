using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mahjong.YakuUtils
{
    // todo -- this needs more revision
    public struct YakuDetails
    {
        public bool Qingtianjing { get; }
        public IEnumerable<Yaku> Yakus { get; private set; }
        public int Points { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            int count = 0;
            bool hasYakuMan = Yakus.Any(yaku => yaku.IsYakuMan);
            foreach (var yaku in Yakus)
            {
                if (Qingtianjing) // 青天井
                {
                    count += yaku.Value;
                    builder.Append($"{yaku.Name}  {yaku.Value}翻\n");
                }
                else // 普通
                {
                    if (hasYakuMan && !yaku.IsYakuMan) continue;
                    if (yaku.IsYakuMan)
                    {
                        var value = yaku.Value == YakuUtil.YakuManBasePoint ? "役满" : $"{yaku.Value}倍役满";
                        builder.Append($"{yaku.Name}  {value}\n");
                    }
                    else
                    {
                        builder.Append($"{yaku.Name}  {yaku.Value}翻\n");
                    }
                }
            }

            builder.Append($"合计：{count}翻");
            return builder.ToString();
        }
    }
}