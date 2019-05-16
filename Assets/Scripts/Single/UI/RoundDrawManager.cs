using System.Collections;
using Multi.ServerData;
using Single.UI.Controller;
using UnityEngine;

namespace Single.UI
{
    public class RoundDrawManager : MonoBehaviour
    {
        public RoundDrawItemController[] Controllers;

        public void SetDrawType(RoundDrawType type)
        {
            var controller = Controllers[(int)type];
            controller.gameObject.SetActive(true);
        }

        public void Fade(RoundDrawType type) {
            var controller = Controllers[(int)type];
            controller.Fade();
        }

        public void Close()
        {
            foreach (var controller in Controllers)
            {
                controller.gameObject.SetActive(false);
            }
        }
    }
}
