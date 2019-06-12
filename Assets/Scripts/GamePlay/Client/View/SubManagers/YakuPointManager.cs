using GamePlay.Client.Controller;
using UnityEngine;

namespace GamePlay.Client.View.SubManagers
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
            YakuPointController.Close();
            PointCharacter.SetActive(false);
        }
    }
}
