using System.Collections;
using Multi.ServerData;
using Single.UI.Controller;
using UnityEngine;

namespace Single.UI
{
    public class RoundDrawManager : MonoBehaviour
    {
        public RoundDrawItemController[] Controllers;
        private WaitForSeconds wait = new WaitForSeconds(5f);

        public void SetDrawType(RoundDrawType type)
        {
            Controllers[(int)type].gameObject.SetActive(true);
        }

        private IEnumerator Disable(RoundDrawItemController controller)
        {
            yield return wait;
            controller.gameObject.SetActive(false);
        }
    }
}
