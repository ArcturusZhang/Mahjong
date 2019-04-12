using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class RoundStatusPanel : MonoBehaviour
    {
        public Text[] PointPanels;

        private void OnDisable()
        {
            foreach (var pointPanel in PointPanels)
            {
                pointPanel.gameObject.SetActive(false);
            }
        }
    }
}