using System;
using System.Collections;
using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.PointSummaryPanel
{
	public class PointInfoController : MonoBehaviour
	{
		public NormalPointInfo NormalPointInfo;
		public Image YakuMan;
		public GameObject YakuItemPrefab;
		public Transform YakuItems;

		private WaitForSeconds waiting;

		private void OnDisable()
		{
			YakuItems.DestroyAllChild();
			NormalPointInfo.gameObject.SetActive(false);
			YakuMan.gameObject.SetActive(false);
			gameObject.SetActive(false);
		}

		public IEnumerator SetPointInfo(PointInfo pointInfo, float delay)
		{
			waiting = new WaitForSeconds(delay);
			gameObject.SetActive(true);
			yield return StartCoroutine(AddYakuEntries(pointInfo));
			ShowPointInfo(pointInfo);
			yield return waiting;
		} 

		private void ShowPointInfo(PointInfo pointInfo)
		{
			if (pointInfo.Is青天井 || !pointInfo.IsYakuman)
			{
				NormalPointInfo.gameObject.SetActive(true);
				YakuMan.gameObject.SetActive(false);
				NormalPointInfo.SetPointInfo(pointInfo.TotalFan, pointInfo.Fu);
			}
			else
			{
				NormalPointInfo.gameObject.SetActive(false);
				YakuMan.gameObject.SetActive(true);
			}
		}

		private IEnumerator AddYakuEntries(PointInfo pointInfo)
		{
			var entries = GetYakuEntries(pointInfo, YakuItems);
			Debug.Log($"YakuItem count: {entries.Count}");
			int rows = Mathf.CeilToInt((float) entries.Count / MahjongConstants.YakuItemColumns);
			rows = Math.Max(MahjongConstants.FullItemCountPerColumn, rows);
			for (int i = 0; i < entries.Count; i++)
			{
				int row = i % rows;
				int col = i / rows;
				AddEntry(entries[i], row, col, rows);
				yield return waiting;
			}
		}

		private void AddEntry(GameObject obj, int row, int col, int rows)
		{
			var rectTransform = obj.GetComponent<RectTransform>();
			float alpha = Alpha + Range / rows * row;
			float theta = (-2 * col + 1) * alpha;
			var position = new Vector2(-Mathf.Sin(theta * Mathf.Deg2Rad), Mathf.Cos(theta * Mathf.Deg2Rad)) * Distance;
			rectTransform.anchoredPosition = position;
			obj.SetActive(true);
			var yakuItem = obj.GetComponent<YakuItem>();
			Debug.Log($"Yaku: {yakuItem.YakuName.text}, row: {row}, col: {col}, theta: {theta}");
		}

		private const float Distance = 300;
		private const float Alpha = 60;
		private const float Beta = 42;
		private const float Range = 180 - Alpha - Beta;

		private List<GameObject> GetYakuEntries(PointInfo pointInfo, Transform holder)
		{
			var entries = new List<GameObject>();
			foreach (var yakuValue in pointInfo.Yakus)
			{
				var entry = Instantiate(YakuItemPrefab, holder);
				entry.SetActive(false);
				entries.Add(entry);
				var yakuItem = entry.GetComponent<YakuItem>();
				yakuItem.SetYakuItem(yakuValue, pointInfo.Is青天井);
			}

			if (pointInfo.IsYakuman && !pointInfo.Is青天井) return entries;

			if (pointInfo.Dora > 0)
			{
				var entry = Instantiate(YakuItemPrefab, holder);
				entry.SetActive(false);
				entries.Add(entry);
				var yakuItem = entry.GetComponent<YakuItem>();
				yakuItem.SetYakuItem(new YakuValue {Name = "宝牌", Value = pointInfo.Dora}, pointInfo.Is青天井);
			}

			if (pointInfo.RedDora > 0)
			{
				var entry = Instantiate(YakuItemPrefab, holder);
				entry.SetActive(false);
				entries.Add(entry);
				var yakuItem = entry.GetComponent<YakuItem>();
				yakuItem.SetYakuItem(new YakuValue {Name = "红宝牌", Value = pointInfo.RedDora}, pointInfo.Is青天井);
			}

			if (pointInfo.UraDora > 0)
			{
				var entry = Instantiate(YakuItemPrefab, holder);
				entry.SetActive(false);
				entries.Add(entry);
				var yakuItem = entry.GetComponent<YakuItem>();
				yakuItem.SetYakuItem(new YakuValue {Name = "里宝牌", Value = pointInfo.UraDora}, pointInfo.Is青天井);
			}

			return entries;
		}
	}
}
