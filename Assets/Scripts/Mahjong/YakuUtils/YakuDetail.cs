using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mahjong.YakuUtils
{
    public struct YakuDetail
    {
        public bool Qingtianjing { get; }
        public IEnumerable<Yaku> Yakus { get; }
        public bool IsYakuMan { get; }
        public int TotalValue { get; }

        public YakuDetail(IEnumerable<Yaku> yakus, bool qingtianjing = false)
        {
            Qingtianjing = qingtianjing;
            var enumerable = yakus.ToList();
            IsYakuMan = enumerable.Any(yaku => yaku.IsYakuMan);
            var list = new List<Yaku>();
            int count = 0;
            foreach (var yaku in enumerable)
            {
                if (Qingtianjing) // 请天井规则
                {
                    count += yaku.Value;
                    list.Add(yaku);
                }
                else // 普通规则
                {
                    if (IsYakuMan && !yaku.IsYakuMan) continue; // 列表中含有役满役，忽略普通役
                    list.Add(yaku);
                    if (yaku.IsYakuMan)
                    {
                        var times = yaku.Value - YakuUtil.YakuManBasePoint + 1;
                        count += times;
                    }
                    else
                    {
                        count += yaku.Value;
                    }
                }
            }

            Yakus = list;
            TotalValue = count;
        }

        public bool IsStackedYakuMan
        {
            get
            {
                if (IsYakuMan) return false;
                return TotalValue >= 13;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var yaku in Yakus)
            {
                if (Qingtianjing) // 青天井
                {
                    builder.Append($"{yaku.Name}  {yaku.Value}翻\n");
                }
                else // 普通
                {
                    if (yaku.IsYakuMan)
                    {
                        var times = yaku.Value - YakuUtil.YakuManBasePoint + 1;
                        var value = times == 1 ? "役满" : $"{times}倍役满";
                        builder.Append($"{yaku.Name}  {value}\n");
                    }
                    else
                    {
                        builder.Append($"{yaku.Name}  {yaku.Value}翻\n");
                    }
                }
            }

            var result = IsYakuMan ? (TotalValue == 1 ? "役满" : $"{TotalValue}倍役满") : $"{TotalValue}翻";
            builder.Append($"合计：{result}");
            return builder.ToString();
        }
    }
}