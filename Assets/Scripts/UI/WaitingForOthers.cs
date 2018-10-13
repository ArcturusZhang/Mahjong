using Multi;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WaitingForOthers : MonoBehaviour
    {
        public Text text;

        private void Update()
        {
            if (MahjongManager.Instance != null)
                text.text = MahjongManager.Instance.SceneLoaded.ToString();
        }
    }
}