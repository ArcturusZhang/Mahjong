using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.MahjongDataType.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class LocalSettingManager : MonoBehaviour, IObserver<ClientLocalSettings>
    {
        private ClientLocalSettings LocalSetting;
        public Toggle Li;
        public Toggle He;
        public Toggle Ming;
        public Toggle Qie;
        public void OnLiChanged(bool value)
        {
            if (LocalSetting == null) return;
            LocalSetting.Li = value;
        }

        public void OnHeChanged(bool value)
        {
            if (LocalSetting == null) return;
            LocalSetting.He = value;
        }

        public void OnMingChanged(bool value)
        {
            if (LocalSetting == null) return;
            LocalSetting.Ming = value;
        }

        public void OnQieChanged(bool value)
        {
            if (LocalSetting == null) return;
            LocalSetting.Qie = value;
        }

        public void UpdateStatus(ClientLocalSettings subject)
        {
            LocalSetting = subject;
            Li.isOn = subject.Li;
            He.isOn = subject.He;
            Ming.isOn = subject.Ming;
            Qie.isOn = subject.Qie;
        }
    }
}
