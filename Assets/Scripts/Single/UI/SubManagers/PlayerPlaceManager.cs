using System.Collections;
using System.Collections.Generic;
using Single.UI.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI.SubManagers
{
    public class PlayerPlaceManager : MonoBehaviour
    {
        public Text PlayerNameText;
        public NumberPanelController PointController;

        public void SetPoints(string playerName, int points)
        {
            PlayerNameText.text = playerName;
            PointController.SetNumber(points);
        }
    }
}
