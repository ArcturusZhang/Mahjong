using System.Collections;
using System.Collections.Generic;
using UI.DataBinding;
using UnityEngine;

namespace Lobby
{
    public class LobbySettingsPanel : MonoBehaviour
    {
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private void Start()
        {
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
        }

        public void CloseSettingsPanel()
        {
            gameObject.SetActive(false);
        }

        public void OpenSettingsPanel()
        {
            gameObject.SetActive(true);
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to default settings");
            var gameSettings = Resources.Load<TextAsset>("Data/GameSettings").text;
            var yakuSettings = Resources.Load<TextAsset>("Data/YakuSettings").text;
            JsonUtility.FromJsonOverwrite(gameSettings, LobbyManager.Instance.GameSettings);
            JsonUtility.FromJsonOverwrite(yakuSettings, LobbyManager.Instance.YakuSettings);
            binders.ForEach(binder => binder?.ApplyBinds());
        }
    }
}
