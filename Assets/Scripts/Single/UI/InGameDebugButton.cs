using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    [RequireComponent(typeof(Button))]
    public class InGameDebugButton : MonoBehaviour
    {
        private void OnEnable()
        {
            var obj = GameObject.Find("DebugPanel");
            if (obj == null) return;
            var button = GetComponent<Button>();
            var manager = obj.GetComponent<PanelManager>();
            button.onClick.AddListener(manager.OnOpenButtonClicked);
        }
    }
}
