using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Client.View.SubManagers
{
    public class WinTypeManager : MonoBehaviour
    {
        public GameObject Tsumo;
        public GameObject Rong;

        public void SetType(bool tsumo) {
            Tsumo.SetActive(tsumo);
            Rong.SetActive(!tsumo);
        }
    }
}
