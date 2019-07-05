using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class WarningPanel : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private RectTransform window;
        [SerializeField] private Text text;
        public void Show(int width, int height, string titleString, string content)
        {
            title.text = titleString;
            window.sizeDelta = new Vector2(width, height);
            text.text = content;
            gameObject.SetActive(true);
        }
        public void Show(int width, int height, string content)
        {
            Show(width, height, "", content);
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
