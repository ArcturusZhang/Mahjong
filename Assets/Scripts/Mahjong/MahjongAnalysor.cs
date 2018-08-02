using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mahjong.Yakus;
using Mahjong.YakuUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Mahjong
{
    public class MahjongAnalysor : MonoBehaviour
    {
        public Text input;

        public void TaskOnClick()
        {
            var yakuTypes = typeof(Yaku).Assembly.GetTypes()
                .Where(clazz => !clazz.IsAbstract && !clazz.IsInterface && typeof(Yaku).IsAssignableFrom(clazz));
            var yakuList = new List<Yaku>();
            foreach (var type in yakuTypes)
            {
                var yaku = (Yaku) Activator.CreateInstance(type);
                yakuList.Add(yaku);
            }
            Debug.Log(input.text);
            var hand = new MahjongHand(input.text);
            var options = new[] {YakuOption.Lizhi, YakuOption.Menqing, YakuOption.Zimo};
            var status = new GameStatus();
            Debug.Log($"手牌：{hand}");
            if (hand.HasTing)
            {
                var builder = new StringBuilder();
                builder.Append("听牌：\n");
                foreach (var rong in hand.TingList)
                {
                    builder.Append(rong).Append("\n");
                    var rongHand = hand.Add(rong);
                    foreach (var mianziSet in rongHand.Decomposition)
                    {
                        builder.Append("分解：").Append(mianziSet).Append("\n");
                        Yaku.PreTest(mianziSet, rong, status, options);
                        yakuList.ForEach(yaku =>
                        {
                            if (yaku.Test(mianziSet, rong, status, options))
                                builder.Append(yaku.Name).Append(" ");
                        });
                        builder.Append("\n");
                    }

                    builder.Append("\n");
                }
                Debug.Log(builder.ToString());
            }
        }
    }
}