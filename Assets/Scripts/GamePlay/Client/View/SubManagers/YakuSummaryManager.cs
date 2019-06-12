using GamePlay.Client.Controller;
using UnityEngine;

namespace GamePlay.Client.View.SubManagers
{
    public class YakuSummaryManager : MonoBehaviour
    {
        public NumberPanelController FanController;
        public NumberPanelController FuController;

        public void SetPointInfo(int fan, int fu)
        {
            FanController.SetNumber(fan);
            FuController.SetNumber(fu);
        }
    }
}
