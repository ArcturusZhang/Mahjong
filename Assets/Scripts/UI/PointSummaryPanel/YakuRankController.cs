using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI.PointSummaryPanel
{
    public class YakuRankController : MonoBehaviour
    {
        public Transform[] Numbers;
        public Transform Lei;
        public Transform Ji;
        public Transform Bei;
        public Transform Tiao;
        public Transform Yi;
        public Transform Man;
        public Transform Guan;

        private Transform[] ManGuan;
        private Transform[] TiaoMan;
        private Transform[] BeiMan;
        private Transform[] SanBeiMan;
        private Transform[] YiMan;
        private Transform[] LeiJiYiMan;
        private Transform[] BeiYiMan;

        private void Awake()
        {
            ManGuan = new[] {Man, Guan};
            TiaoMan = new[] {Tiao, Man};
            BeiMan = new[] {Bei, Man};
            SanBeiMan = new[] {Numbers[1], Bei, Man};
            YiMan = new[] {Yi, Man};
            LeiJiYiMan = new[] {Lei, Ji, Yi, Man};
            BeiYiMan = new[] {Bei, Yi, Man};
        }

        public void SetYakuRank(PointInfo pointInfo)
        {
            if (pointInfo.Fan == 0) return;
            if (pointInfo.Is青天井) return;
            gameObject.SetActive(true);
            if (pointInfo.IsYakuman)
            {
                var value = pointInfo.Fan;
                if (value == 1)
                {
                    SetRank(YiMan);
                    return;
                }

                Assert.IsTrue(value >= 2);
                SetRank(Numbers[value - 2], BeiYiMan);
                return;
            }

            switch (pointInfo.BasePoint)
            {
                case MahjongConstants.Yakuman:
                    SetRank(LeiJiYiMan);
                    break;
                case MahjongConstants.Sanbaiman:
                    SetRank(SanBeiMan);
                    break;
                case MahjongConstants.Baiman:
                    SetRank(BeiMan);
                    break;
                case MahjongConstants.Haneman:
                    SetRank(TiaoMan);
                    break;
                case MahjongConstants.Mangan:
                    SetRank(ManGuan);
                    break;
                default:
                    Assert.IsTrue(pointInfo.BasePoint < MahjongConstants.Mangan,
                        $"Point info: {pointInfo} should be less than mangan");
                    break;
            }
        }

        private void SetRank(Transform number, Transform[] rankImages)
        {
            number.gameObject.SetActive(true);
            SetRank(rankImages);
        }

        private void SetRank(Transform[] rankImages)
        {
            int index = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                var t = transform.GetChild(i);
                if (t == rankImages[index])
                {
                    t.gameObject.SetActive(true);
                    if (++index >= rankImages.Length) break;
                }
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }
    }
}