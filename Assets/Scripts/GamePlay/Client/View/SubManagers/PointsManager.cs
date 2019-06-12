using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class PointsManager : MonoBehaviour
    {
        public Text[] TextFields;
        [HideInInspector] public int TotalPlayers;
        [HideInInspector] public int[] Places;
        [HideInInspector] public int[] Points;

        private void Update()
        {
            if (Places == null) return;
            for (int i = 0; i < Places.Length; i++)
            {
                if (IsValidPlayer(Places[i]))
                {
                    TextFields[i].gameObject.SetActive(true);
                    TextFields[i].text = Points[i].ToString();
                }
                else
                    TextFields[i].gameObject.SetActive(false);
            }
        }

        private bool IsValidPlayer(int index)
        {
            return index >= 0 && index < TotalPlayers;
        }
    }
}