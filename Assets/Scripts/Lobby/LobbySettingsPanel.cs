using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lobby
{
    public class LobbySettingsPanel : MonoBehaviour
    {
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
        }
    }
}
