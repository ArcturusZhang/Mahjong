using UnityEngine;
using UnityEngine.Assertions;
using DG.Tweening;
using Mahjong.Model;
using Mahjong.Logic;

namespace GamePlay.Client.View.SubManagers
{
    public class YakuRankManager : MonoBehaviour
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
            ManGuan = new[] { Man, Guan };
            TiaoMan = new[] { Tiao, Man };
            BeiMan = new[] { Bei, Man };
            SanBeiMan = new[] { Numbers[1], Bei, Man };
            YiMan = new[] { Yi, Man };
            LeiJiYiMan = new[] { Lei, Ji, Yi, Man };
            BeiYiMan = new[] { Bei, Yi, Man };
        }

        public void ShowYakuRank(PointInfo pointInfo) {
            SetYakuRank(pointInfo);
            var rect = GetComponent<RectTransform>();
            rect.localScale = new Vector3(ScaleFactor, ScaleFactor, ScaleFactor);
            rect.DOScale(Vector3.one, AnimationDuration).SetEase(Ease.OutQuad);
        }

        private const float ScaleFactor = 1.2f;
        private const float AnimationDuration = 0.5f;

        private void SetYakuRank(PointInfo pointInfo)
        {
            if (pointInfo.TotalFan == 0) return;
            if (pointInfo.IsQTJ) return;
            if (pointInfo.IsYakuman)
            {
                var value = pointInfo.TotalFan;
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
        }
    }
}
