using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Single
{
    public class Debug : MonoBehaviour
    {
        public static Debug Instance { get; private set; }
        private const int CUT_THRESHOLD = 1200;

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
            CutLog();
        }

        internal void LogWarningInternal(object message)
        {
            LogText.text += $"<color=#ffff00ff>{message}</color>\n";
            CutLog();
        }

        internal void LogErrorInternal(object message)
        {
            LogText.text += $"<color=#ff0000ff>{message}</color>\n";
            CutLog();
        }

        private void CutLog() {
            var log = LogText.text;
            if (log.Length > CUT_THRESHOLD * 2)
            LogText.text = log.Substring(log.Length - CUT_THRESHOLD);
        }
    }
}