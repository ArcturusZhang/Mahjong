using System.Collections;
using System.Collections.Generic;
using Single.UI.Controller;
using UnityEngine;

namespace Single.UI.SubManagers
{
    public class YakuPointManager : MonoBehaviour
    {
        public NumberPanelController YakuPointController;
        public GameObject PointCharacter;

        public void SetNumber(int point)
        {
            YakuPointController.SetNumber(point);
            PointCharacter.SetActive(true);
        }

        private void OnDisable()
        {
            PointCharacter.SetActive(false);
        }
    }
}
