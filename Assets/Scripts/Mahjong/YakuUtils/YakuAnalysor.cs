using System;
using System.Collections.Generic;
using System.Linq;
using Mahjong.Yakus;

namespace Mahjong.YakuUtils
{
    public static class YakuAnalysor
    {
        private static List<Yaku> GetAvailableYakuList()
        {
            var yakuTypes = typeof(Yaku).Assembly.GetTypes()
                .Where(clazz => !clazz.IsAbstract && !clazz.IsInterface && typeof(Yaku).IsAssignableFrom(clazz));

            return yakuTypes.Select(type => (Yaku) Activator.CreateInstance(type)).ToList();
        }

        public static IDictionary<Tile, PointResult> Analyze(MahjongHand hand, GameStatus status,
            params YakuOption[] options)
        {
            if (!hand.HasTing) throw new ArgumentException("手牌未听牌");
            var yakuList = GetAvailableYakuList();
            var dict = new Dictionary<Tile, PointResult>();
            bool qingtianjing = options.Contains(YakuOption.Qingtianjing);
            foreach (var rong in hand.TingList)
            {
                var newHand = hand.Add(rong);
                var point = new PointResult();
                foreach (var mianziSet in newHand.Decomposition)
                {
                    var yakus = new List<Yaku>();
                    Yaku.PreTest(mianziSet, rong, options);
                    yakuList.ForEach(yaku =>
                    {
                        if (yaku.Test(mianziSet, rong, status, options)) yakus.Add(yaku);
                    });
                    var currentFu = Fu(yakus, mianziSet, rong, status, options);
                    var detail = new YakuDetail(yakus, qingtianjing);
                    var currentPoint = new PointResult(currentFu, detail);
                    if (currentPoint > point) // 取最高得点作为结果
                    {
                        point = currentPoint;
                    }
                }
                dict.Add(rong, point);
            }

            return dict;
        }

        private static int Fu(ICollection<Yaku> yakus, MianziSet hand, Tile rong, GameStatus status,
            params YakuOption[] options)
        {
            if (yakus.Contains(new 七对子())) return 25;
            if (yakus.Contains(new 国士无双())) return 30;
            int fu = 20;
            // 门清荣胡加符
            if (!options.Contains(YakuOption.Zimo) && options.Contains(YakuOption.Menqing)) fu += 10;
            // 自摸加符
            if (options.Contains(YakuOption.Zimo) && !yakus.Contains(new 平和()) &&
                !yakus.Contains(new 岭上开花())) fu += 2;
            // 将牌加符
            var tile = hand.Jiang.First;
            if (tile.Suit == Suit.Z)
            {
                // 自风加符2
                if (tile.Equals(status.Zifeng)) fu += 2;
                // 场风加符2
                if (tile.Equals(status.Changfeng)) fu += 2;
                // 三元牌加符2
                if (tile.Index > 4) fu += 2;
            }

            // 听牌加符
            int flag = 0;
            foreach (var mianzi in hand)
            {
                if (!mianzi.Contains(rong)) continue;
                switch (mianzi.Type)
                {
                    case MianziType.Jiang: // 和牌在将中出现，单骑听牌
                        flag++;
                        break;
                    case MianziType.Shunzi: // 和牌在顺子中出现，可能是边张或砍张
                        if (rong.Equals(mianzi.First.Next)) // 和牌是顺子的第二张：砍张
                        {
                            flag++;
                        }
                        else if (rong.Equals(mianzi.First) && rong.Index == 7) // 789的边张
                        {
                            flag++;
                        }
                        else if (rong.Equals(mianzi.Last) && rong.Index == 3) // 123的边张
                        {
                            flag++;
                        }

                        break;
                }
            }

            if (flag != 0) fu += 2; // 听牌加符的几种形式之间是不能共存的
            // 刻子加符
            foreach (var mianzi in hand)
            {
                if (mianzi.Type != MianziType.Kezi) continue;
                int kezi = mianzi.Open ? 2 : 4;
                if (mianzi.IsGangzi) kezi *= 4;
                if (mianzi.IsYaojiu) kezi *= 2;
                fu += kezi;
            }

            // 符为整十，直接返回符数
            if (fu % 10 == 0) return fu;
            // 符不为整十，返回下一个整十
            return (fu / 10 + 1) * 10;
        }
    }
}