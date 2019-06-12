using System;
using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class DoraPanelManager : MonoBehaviour
    {
        private const string back = "back";
        public Image[] DoraIndicators;
        public Image[] UraDoraIndicators;
        private ResourceManager manager;

        public void SetDoraIndicators(IList<Tile> doraIndicators, IList<Tile> uraDoraIndicators)
        {
            if (manager == null) manager = ResourceManager.Instance;
            SetIndicators(DoraIndicators, doraIndicators);
            SetIndicators(UraDoraIndicators, uraDoraIndicators);
        }

        private void SetIndicators(Image[] indicators, IList<Tile> tiles)
        {
            int minLength = Math.Min(indicators.Length, tiles != null ? tiles.Count : 0);
            for (int i = 0; i < minLength; i++)
            {
                indicators[i].sprite = manager.GetTileSprite(tiles[i]);
            }
            for (int i = minLength; i < indicators.Length; i++)
            {
                indicators[i].sprite = manager.GetTileSpriteByName(back);
            }
        }
    }
}
