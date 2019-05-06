using System.Collections;
using System.Collections.Generic;
using Single.UI.Controller;
using Single.UI.SubManagers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Single.UI
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
                PlaceManagers[i].gameObject.SetActive(true);
                int playerIndex = playerPlaces[i];
                PlaceManagers[i].SetPoints(playerNames[playerIndex], playerPoints[playerIndex]);
            }
            for (int i = playerPlaces.Length; i < PlaceManagers.Length; i++)
            {
                PlaceManagers[i].gameObject.SetActive(false);
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
