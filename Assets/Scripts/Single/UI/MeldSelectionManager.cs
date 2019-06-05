using System;
using Single.MahjongDataType;
using Single.UI.Controller;
using UnityEngine;
using Utils;

namespace Single.UI
{
    public class MeldSelectionManager : MonoBehaviour
    {
        public GameObject MeldOptionPrefab;

        public void SetMeldOptions(OpenMeld[] options, Action<OpenMeld> callback)
        {
            gameObject.SetActive(true);
            foreach (var meld in options)
            {
                var obj = Instantiate(MeldOptionPrefab, transform);
                var option = obj.GetComponent<MeldOptionController>();
                option.SetMeld(meld, callback);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            transform.DestroyAllChildren();
        }
    }
}
