using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleTextColorChanger : MonoBehaviour
    {
        public Color onColor = Color.black;
        public Color offColor = Color.white;
        private Toggle toggle;
        private Text text;

        private void Start()
        {
            toggle = GetComponent<Toggle>();
            text = GetComponentInChildren<Text>(true);
            OnValueChanged(toggle.isOn);
        }

        public void OnValueChanged(bool isOn)
        {
            if (text == null) return;
            if (isOn) text.color = onColor;
            else text.color = offColor;
        }
    }
}
