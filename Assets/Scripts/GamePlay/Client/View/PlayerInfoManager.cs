using Common.Interfaces;
using GamePlay.Client.Model;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View
{
    public class PlayerInfoManager : MonoBehaviour, IObserver<ClientRoundStatus>
    {
        public Text[] TextFields;

        private void UpdateNames(ClientRoundStatus status)
        {
            for (int placeIndex = 0; placeIndex < TextFields.Length; placeIndex++)
            {
                int playerIndex = status.GetPlayerIndex(placeIndex);
                if (IsValidPlayer(playerIndex, status.TotalPlayers))
                {
                    TextFields[placeIndex].gameObject.SetActive(true);
                    TextFields[placeIndex].text = status.GetPlayerName(placeIndex);
                }
                else
                    TextFields[placeIndex].gameObject.SetActive(false);
            }
        }

        private bool IsValidPlayer(int index, int totalPlayers)
        {
            return index >= 0 && index < totalPlayers;
        }

        public void UpdateStatus(ClientRoundStatus subject)
        {
            if (subject == null) return;
            UpdateNames(subject);
        }
    }
}
