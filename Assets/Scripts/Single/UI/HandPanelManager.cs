using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;

namespace Single.UI
{
    public class HandPanelManager : MonoBehaviour
    {
        public HandTileInstance[] HandTileInstances;
        public HandTileInstance LastDrawInstance;
        [HideInInspector] public List<Tile> Tiles;
        [HideInInspector] public Tile? LastDraw;

        private void Update()
        {
            ShowHandTiles();
            ShowLastDraw();
        }

        private void ShowHandTiles()
        {
            int length = Tiles == null ? 0 : Tiles.Count;
            for (int i = 0; i < length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(true);
                HandTileInstances[i].SetTile(Tiles[i]);
            }
            for (int i = length; i < HandTileInstances.Length; i++)
            {
                HandTileInstances[i].gameObject.SetActive(false);
            }
        }

        private void ShowLastDraw()
        {
            if (LastDraw == null)
                LastDrawInstance.gameObject.SetActive(false);
            else
            {
                LastDrawInstance.SetTile((Tile)LastDraw);
                LastDrawInstance.gameObject.SetActive(true);
            }
        }
    }
}
