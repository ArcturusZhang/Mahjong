using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace Single.Managers
{
    public class PlayerInfoManager : ManagerBase
    {
        public Text[] TextFields;
        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            UpdateNames();
        }

        private void UpdateNames()
        {
            for (int placeIndex = 0; placeIndex < TextFields.Length; placeIndex++)
            {
                int playerIndex = CurrentRoundStatus.GetPlayerIndex(placeIndex);
                if (IsValidPlayer(playerIndex))
                {
                    TextFields[placeIndex].gameObject.SetActive(true);
                    TextFields[placeIndex].text = CurrentRoundStatus.GetPlayerName(placeIndex);
                }
                else
                    TextFields[placeIndex].gameObject.SetActive(false);
            }
        }

        private bool IsValidPlayer(int index)
        {
            return index >= 0 && index < CurrentRoundStatus.TotalPlayers;
        }
    }
}
