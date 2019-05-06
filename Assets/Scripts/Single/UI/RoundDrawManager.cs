using System.Collections;
using Multi.ServerData;
using Single.UI.Controller;
using UnityEngine;

namespace Single.UI
{
    public class RoundDrawManager : MonoBehaviour
    {
        public RoundDrawItemController[] Controllers;
        private WaitForSeconds wait = new WaitForSeconds(2f);

        public void SetDrawType(RoundDrawType type)
        {
            var controller = Controllers[(int)type];
            controller.gameObject.SetActive(true);
            StartCoroutine(Disable(controller));
        }

        private IEnumerator Disable(RoundDrawItemController controller)
        {
            yield return wait;
            controller.gameObject.SetActive(false);
        }
    }
}
