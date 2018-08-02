using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.YakuUtils
{
    public class YakuAnalysor
    {

        private static List<Yaku> GetAvailableYakuList()
        {
            var yakuList = new List<Yaku>();
            var yakuTypes = typeof(Yaku).Assembly.GetTypes()
                .Where(clazz => !clazz.IsAbstract && !clazz.IsInterface && typeof(Yaku).IsAssignableFrom(clazz));
            foreach (var type in yakuTypes)
            {
                var yaku = (Yaku) Activator.CreateInstance(type);
                yakuList.Add(yaku);
            }

            return yakuList;
        }

        public static IDictionary<Tile, YakuDetails> Analyze(MahjongHand hand, GameStatus status, params YakuOption[] options)
        {
            if (!hand.HasTing) throw new ArgumentException("手牌未听牌");
            var yakuList = GetAvailableYakuList();
            var dict = new Dictionary<Tile, YakuDetails>();
            foreach (var rong in hand.TingList)
            {
                var newHand = hand.Add(rong);
                foreach (var mianziSet in newHand.Decomposition)
                {
                    var yakus = new List<Yaku>();
                    Yaku.PreTest(mianziSet, rong, status, options);
                    yakuList.ForEach(yaku =>
                    {
                        if (yaku.Test(mianziSet, rong, status, options)) yakus.Add(yaku);
                    });
                    // todo
                }
            }

            return dict;
        }
    }
}