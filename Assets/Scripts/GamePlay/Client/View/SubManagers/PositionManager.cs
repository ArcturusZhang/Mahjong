using System.Collections;
using System.Collections.Generic;
using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class PositionManager : MonoBehaviour
    {
        [HideInInspector] public int TotalPlayers;
        [HideInInspector] public int[] Places;
        [HideInInspector] public int OyaPlayerIndex;
        public Image[] PlaceImages;
        public SpriteBundle Images;

        private void Update()
        {
            if (Places == null) return;
            for (int i = 0; i < Places.Length; i++)
            {
                if (IsValidPlayer(Places[i]))
                {
                    PlaceImages[i].gameObject.SetActive(true);
                    var index = Places[i] - OyaPlayerIndex;
                    if (index < 0) index += TotalPlayers;
                    if (index > TotalPlayers) index -= TotalPlayers;
                    PlaceImages[i].sprite = Images.Get(index);
                }
                else
                {
                    PlaceImages[i].gameObject.SetActive(false);
                }
            }
        }

        private bool IsValidPlayer(int index)
        {
            return index >= 0 && index < TotalPlayers;
        }
    }
}
