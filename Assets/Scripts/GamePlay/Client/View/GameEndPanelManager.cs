using System.Collections;
using GamePlay.Client.Controller;
using GamePlay.Client.View.SubManagers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GamePlay.Client.View
{
    public class GameEndPanelManager : MonoBehaviour
    {
        public PlayerPlaceManager[] PlaceManagers;
        public Button ConfirmButton;
        public CountDownController ConfirmCountDownController;

        public void SetPoints(string[] playerNames, int[] playerPoints, int[] playerPlaces, UnityAction callback)
        {
            gameObject.SetActive(true);
            ConfirmButton.onClick.RemoveAllListeners();
            ConfirmButton.onClick.AddListener(callback);
            for (int i = 0; i < playerPlaces.Length; i++)
            {
                int playerIndex = playerPlaces[i];
                PlaceManagers[i].SetPoints(playerNames[playerIndex], playerPoints[playerIndex], i);
            }
            StartCoroutine(ShowAnimation(playerPlaces.Length));
        }

        private IEnumerator ShowAnimation(int totalPlayers)
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                var duration = PlaceManagers[i].Show();
                yield return new WaitForSeconds(duration);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            for (int i = 0; i < PlaceManagers.Length; i++)
            {
                PlaceManagers[i].gameObject.SetActive(false);
            }
        }
    }
}
