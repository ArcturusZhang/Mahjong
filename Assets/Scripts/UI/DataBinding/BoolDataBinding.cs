using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.DataBinding
{
    [RequireComponent(typeof(ToggleGroup))]
    public class BoolDataBinding : MonoBehaviour, IBindData
    {
        public object Target { get; set; }
        [SerializeField] private string fieldName;
        private ToggleGroup toggleGroup;
        private FieldInfo fieldInfo;
        private Toggle[] toggles;

        private void OnEnable()
        {
            toggleGroup = GetComponent<ToggleGroup>();
            toggleGroup.allowSwitchOff = false;
        }

        public void Apply()
        {
            if (!CheckNull()) return;
            bool value = (bool)fieldInfo.GetValue(Target);
            if (value)
                toggles[0].isOn = true;
            else
                toggles[1].isOn = true;
        }

        public void UpdateBind()
        {
            if (!CheckNull()) return;
            var oldValue = (bool)fieldInfo.GetValue(Target);
            var activeIndex = Array.FindIndex(toggles, t => t.isOn);
            fieldInfo.SetValue(Target, activeIndex == 0);
            if (oldValue != (activeIndex == 0))
                OnValueChanged.Invoke(activeIndex == 0);
        }

        private bool CheckNull()
        {
            if (Target == null)
            {
                Debug.Log("Target is null");
                return false;
            }
            if (fieldInfo == null)
            {
                fieldInfo = Target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogError($"Unknown field name: {fieldName}");
                    return false;
                }
            }
            if (transform.childCount < 2)
            {
                Debug.LogError($"Not enough toggle children, found {transform.childCount} require 2");
                return false;
            }
            if (transform.childCount > 2)
            {
                Debug.LogWarning("Too many children found, only using first two");
            }
            if (toggles == null)
            {
                toggles = new Toggle[2];
                for (int i = 0, count = 0; i < transform.childCount; i++)
                {
                    if (count >= toggles.Length) break;
                    var t = transform.GetChild(i).GetComponent<Toggle>();
                    if (t != null) toggles[count++] = t;
                }
            }
            return true;
        }

        public BoolEvent OnValueChanged;

        [Serializable]
        public class BoolEvent : UnityEvent<bool> { }
    }
}
