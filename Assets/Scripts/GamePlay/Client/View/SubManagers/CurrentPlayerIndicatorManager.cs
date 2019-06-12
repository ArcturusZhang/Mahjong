using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class CurrentPlayerIndicatorManager : MonoBehaviour
    {
        public Image[] Indicators;
        public int CurrentPlaceIndex
        {
            get => currentPlayerIndex;
            set
            {
                if (value == currentPlayerIndex) return;
                currentPlayerIndex = value;
                SwitchToPlayer(currentPlayerIndex);
            }
        }
        private int currentPlayerIndex = -1;

        private void Start()
        {
            CurrentPlaceIndex = -1;
        }

        private void SwitchToPlayer(int place)
        {
            TurnOff();
            TurnOn(place);
        }

        public void TurnOff()
        {
            foreach (var indicator in Indicators)
            {
                indicator.gameObject.SetActive(false);
            }
        }

        public void TurnOn(int place)
        {
            if (place < 0 || place >= Indicators.Length)
            {
                TurnOff();
                return;
            }
            Indicators[place].gameObject.SetActive(true);
        }
    }
}
