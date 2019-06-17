using System.Collections;
using System.Collections.Generic;
using Mahjong.Model;
using UI.DataBinding;
using UnityEngine;

namespace PUNLobby
{
    public class RulePanel : MonoBehaviour
    {
        public RectTransform baseSettingPanel;
        public RectTransform yakuSettingPanel;
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private GameSetting gameSettings;
        private void OnEnable()
        {
            baseSettingPanel.gameObject.SetActive(true);
            yakuSettingPanel.gameObject.SetActive(false);
        }
        public void Show(GameSetting gameSettings)
        {
            this.gameSettings = gameSettings;
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
            gameObject.SetActive(true);
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
        public void OpenYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(false);
            yakuSettingPanel.gameObject.SetActive(true);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }
        public void CloseYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(true);
            yakuSettingPanel.gameObject.SetActive(false);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }
    }
}
