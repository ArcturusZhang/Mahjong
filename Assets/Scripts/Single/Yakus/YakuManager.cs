using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.Yakus
{
    public class YakuManager : MonoBehaviour
    {
        public static YakuManager Instance;

        public YakuData YakuData;

        private IEnumerable<MethodInfo> yakuMethods;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            yakuMethods = YakuData.GetType().GetMethods().Where(p =>
                p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new[]
                    {typeof(List<Meld>), typeof(Tile), typeof(HandStatus), typeof(PlayerStatus)}));
        }

        public IList<YakuValue> CountYaku(List<Meld> decompose, Tile winningTile, HandStatus handStatus,
            PlayerStatus playerStatus)
        {
            var result = new List<YakuValue>();
            foreach (var yakuMethod in yakuMethods)
            {
                var value = (YakuValue) yakuMethod.Invoke(YakuData,
                    new object[] {decompose, winningTile, handStatus, playerStatus});
                if (value.Value != 0)
                    result.Add(value);
            }

            return result;
        }

        private void FixedUpdate()
        {
            // for test
            if (Input.GetKeyDown(KeyCode.A))
            {
                var handTiles = new List<Tile>
                {
                    new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 1), new Tile(Suit.M, 2),
                    new Tile(Suit.M, 2), new Tile(Suit.M, 2), new Tile(Suit.M, 3), new Tile(Suit.M, 3),
                    new Tile(Suit.M, 3), new Tile(Suit.M, 4), new Tile(Suit.M, 4), new Tile(Suit.M, 4),
                    new Tile(Suit.M, 5),
                };
                var decompose = MahjongLogic.Decompose(handTiles, new List<Meld>(), new Tile(Suit.M, 5));
                foreach (var list in decompose)
                {
                    CountYaku(list, new Tile(Suit.M, 5), HandStatus.Menqing,
                        new PlayerStatus {StartIndex = 0, RoundCount = 1, FieldCount = 1});
                }
            }
        }
    }
}