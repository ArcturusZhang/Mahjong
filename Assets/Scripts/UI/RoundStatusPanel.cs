using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoundStatusPanel : MonoBehaviour
    {
        public Text FieldInfo;
        public Text RoundInfo;
        public Text[] PointPanels;
        public Image[] PlacePanels;

        private void OnDisable()
        {
            FieldInfo.gameObject.SetActive(false);
            RoundInfo.gameObject.SetActive(false);
            foreach (var pointPanel in PointPanels)
            {
                pointPanel.gameObject.SetActive(false);
            }

            foreach (var placePanel in PlacePanels)
            {
                placePanel.gameObject.SetActive(false);
            }
        }
    }
}