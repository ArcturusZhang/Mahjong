using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Single.UI
{
    public class PanelManager : MonoBehaviour
    {
        public void OnCloseButtonClicked() {
            gameObject.SetActive(false);
        }

        public void OnOpenButtonClicked() {
            gameObject.SetActive(true);
        }
    }
}
