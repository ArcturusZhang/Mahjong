using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
{
    public class RichiStatusManager : MonoBehaviour
    {
        public Image[] RichiSticks;
        [HideInInspector] public int TotalPlayers;
        [HideInInspector] public int[] Places;
        [HideInInspector] public bool[] RichiStatus;
        private void Update()
        {
            if (Places == null) return;
            for (int i = 0; i < Places.Length; i++)
            {
                if (IsValidPlayer(Places[i]))
                {
                    RichiSticks[i].gameObject.SetActive(RichiStatus[i]);
                }
                else
                {
                    RichiSticks[i].gameObject.SetActive(false);
                }
            }
        }

        private bool IsValidPlayer(int index)
        {
            return index >= 0 && index < TotalPlayers;
        }
    }
}
