using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class WarningPanel : MonoBehaviour
    {
        public RectTransform window;
        public Text text;
        public void Show(int width, int height, string content)
        {
            window.sizeDelta = new Vector2(width, height);
            text.text = content;
            gameObject.SetActive(true);
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
