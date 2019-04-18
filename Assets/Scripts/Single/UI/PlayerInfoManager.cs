using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class PlayerInfoManager : MonoBehaviour
    {
        [HideInInspector] public int TotalPlayers;
        [HideInInspector] public int[] Places;
        [HideInInspector] public string[] Names;
        public Text[] TextFields;
        private void Update()
        {
            for (int i = 0; i < Names.Length; i++)
            {
                if (IsValidPlayer(Places[i]))
                {
                    TextFields[i].gameObject.SetActive(true);
                    TextFields[i].text = Names[i];
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
