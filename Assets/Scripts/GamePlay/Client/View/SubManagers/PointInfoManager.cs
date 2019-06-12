using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using DG.Tweening;
using Mahjong.Model;
using Mahjong.Logic;

namespace GamePlay.Client.View.SubManagers
{
    public class PointInfoManager : MonoBehaviour
    {
        public YakuSummaryManager FanAndFuManager;
        public Image YakuMan;
        public GameObject YakuItemPrefab;
        public Transform YakuItems;
        public YakuPointManager PointManager;
        private WaitForSeconds waiting = new WaitForSeconds(MahjongConstants.SummaryPanelDelayTime);

        private void OnDisable()
        {
            YakuItems.DestroyAllChildren();
            FanAndFuManager.gameObject.SetActive(false);
            YakuMan.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public IEnumerator SetPointInfo(PointInfo pointInfo, int totalPoints, bool richi)
        {
            gameObject.SetActive(true);
            yield return waiting;
            yield return AddYakuEntries(pointInfo, richi);
            ShowFuAndFan(pointInfo);
            yield return waiting;
            ShowTotalPoints(totalPoints);
        }

        private void ShowFuAndFan(PointInfo pointInfo)
        {
            RectTransform rect;
            if (pointInfo.IsQTJ || !pointInfo.IsYakuman)
            {
                FanAndFuManager.gameObject.SetActive(true);
                YakuMan.gameObject.SetActive(false);
                FanAndFuManager.SetPointInfo(pointInfo.TotalFan, pointInfo.Fu);
                rect = FanAndFuManager.GetComponent<RectTransform>();
            }
            else
            {
                FanAndFuManager.gameObject.SetActive(false);
                YakuMan.gameObject.SetActive(true);
                rect = YakuMan.GetComponent<RectTransform>();
            }
            rect.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            rect.DOScale(new Vector3(1, 1, 1), AnimationDuration).SetEase(Ease.OutQuad);
        }

        private void ShowTotalPoints(int totalPoints)
        {
            PointManager.SetNumber(totalPoints);
            var rect = PointManager.GetComponent<RectTransform>();
            rect.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
            rect.DOScale(new Vector3(1, 1, 1), AnimationDuration).SetEase(Ease.OutQuad);
        }

        private IEnumerator AddYakuEntries(PointInfo pointInfo, bool richi)
        {
            var entries = GetYakuEntries(pointInfo, richi, YakuItems);
            Debug.Log($"YakuItem count: {entries.Count}");
            int rows = Mathf.CeilToInt((float)entries.Count / MahjongConstants.YakuItemColumns);
            rows = Math.Max(MahjongConstants.FullItemCountPerColumn, rows);
            for (int i = 0; i < entries.Count; i++)
            {
                int row = i % rows;
                int col = i / rows;
                AddEntry(entries[i], row, col, rows);
                yield return waiting;
            }
        }

        private const float scaleFactor = 1.2f;
        private const float Displacement = 50;
        private const float AnimationDuration = 0.5f;

        private void AddEntry(GameObject obj, int row, int col, int rows)
        {
            obj.SetActive(true);
            var rectTransform = obj.GetComponent<RectTransform>();
            float alpha = Alpha + Range / rows * row;
            float theta = (-2 * col + 1) * alpha;
            var position = new Vector2(-Mathf.Sin(theta * Mathf.Deg2Rad), Mathf.Cos(theta * Mathf.Deg2Rad)) * Distance;
            rectTransform.anchoredPosition = position - new Vector2(Displacement, 0);
            rectTransform.DOAnchorPos(position, AnimationDuration).SetEase(Ease.OutQuad);
        }

        private const float Distance = 300;
        private const float Alpha = 70;
        private const float Beta = 48;
        private const float Range = 180 - Alpha - Beta;

        private List<GameObject> GetYakuEntries(PointInfo pointInfo, bool richi, Transform holder)
        {
            var entries = new List<GameObject>();
            foreach (var yakuValue in pointInfo.YakuList)
            {
                var entry = Instantiate(YakuItemPrefab, holder);
                entry.SetActive(false);
                entries.Add(entry);
                var yakuItem = entry.GetComponent<YakuItem>();
                yakuItem.SetYakuItem(yakuValue, pointInfo.IsQTJ);
            }
            if (pointInfo.IsYakuman && !pointInfo.IsQTJ) return entries;
            if (pointInfo.Dora > 0)
            {
                var entry = Instantiate(YakuItemPrefab, holder);
                entry.SetActive(false);
                entries.Add(entry);
                var yakuItem = entry.GetComponent<YakuItem>();
                yakuItem.SetYakuItem(new YakuValue { Name = "宝牌", Value = pointInfo.Dora }, pointInfo.IsQTJ);
            }
            if (pointInfo.RedDora > 0)
            {
                var entry = Instantiate(YakuItemPrefab, holder);
                entry.SetActive(false);
                entries.Add(entry);
                var yakuItem = entry.GetComponent<YakuItem>();
                yakuItem.SetYakuItem(new YakuValue { Name = "红宝牌", Value = pointInfo.RedDora }, pointInfo.IsQTJ);
            }
            if (pointInfo.BeiDora > 0)
            {
                var entry = Instantiate(YakuItemPrefab, holder);
                entry.SetActive(false);
                entries.Add(entry);
                var yakuItem = entry.GetComponent<YakuItem>();
                yakuItem.SetYakuItem(new YakuValue { Name = "北宝牌", Value = pointInfo.BeiDora }, pointInfo.IsQTJ);
            }
            if (richi)
            {
                var entry = Instantiate(YakuItemPrefab, holder);
                entry.SetActive(false);
                entries.Add(entry);
                var yakuItem = entry.GetComponent<YakuItem>();
                yakuItem.SetYakuItem(new YakuValue { Name = "里宝牌", Value = pointInfo.UraDora }, pointInfo.IsQTJ);
            }
            return entries;
        }
    }
}
