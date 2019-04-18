using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Single
{
    public class Debug : MonoBehaviour
    {
        public static Debug Instance { get; private set; }

        public static void Log(object message, bool showOnUi = true)
        {
            UnityEngine.Debug.Log(message);
            if (showOnUi)
                Instance?.LogInternal(message);
        }

        public static void LogWarning(object message, bool showOnUi = true)
        {
            UnityEngine.Debug.LogWarning(message);
            if (showOnUi)
                Instance?.LogWarningInternal(message);
        }

        public static void LogError(object message, bool showOnUi = true)
        {
            UnityEngine.Debug.LogError(message);
            if (showOnUi)
                Instance?.LogErrorInternal(message);
        }

        public GameObject DebugParent;
        // public Text LogText;

        public TMP_Text LogText;

        private void OnEnable()
        {
            Instance = this;
            LogText = GetComponent<TMP_Text>();
            LogText.text = "";
        }

        internal void LogInternal(object message)
        {
            LogText.text += $"<color=#000000ff>{message}</color>\n";
        }

        internal void LogWarningInternal(object message)
        {
            LogText.text += $"<color=#ffff00ff>{message}</color>\n";
        }

        internal void LogErrorInternal(object message)
        {
            LogText.text += $"<color=#ff0000ff>{message}</color>\n";
        }
    }
}