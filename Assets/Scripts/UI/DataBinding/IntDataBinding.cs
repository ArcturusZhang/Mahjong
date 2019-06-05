using System;
using System.Reflection;
using UI.DataBinding.DataVerify;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.DataBinding
{
    public class IntDataBinding : MonoBehaviour, IBindData
    {
        public object Target { get; set; }
        [SerializeField] private string fieldName;
        private InputField inputField;
        private FieldInfo fieldInfo;
        private Verifier verifier;

        private void OnEnable()
        {
            inputField = GetComponentInChildren<InputField>();
            verifier = GetComponent<Verifier>();
            inputField.onEndEdit.AddListener(VerifyData);
        }

        public void Apply()
        {
            if (!CheckNull()) return;
            int value = (int)fieldInfo.GetValue(Target);
            inputField.text = value.ToString();
        }

        public void UpdateBind()
        {
            if (!CheckNull()) return;
            if (inputField.isFocused) return;
            var oldValue = (int)fieldInfo.GetValue(Target);
            var value = int.Parse(inputField.text);
            fieldInfo.SetValue(Target, value);
            if (oldValue != value)
                OnValueChanged.Invoke(value);
        }

        private void VerifyData(string data)
        {
            if (!CheckNull() || verifier == null) return;
            inputField.text = verifier.Verify(data, fieldInfo.GetValue(Target).ToString());
        }

        private bool CheckNull()
        {
            if (Target == null)
            {
                Debug.Log($"Target is null");
                return false;
            }
            if (inputField == null)
            {
                inputField = GetComponentInChildren<InputField>();
                if (inputField == null)
                {
                    Debug.LogError($"No input field found ({name})");
                    return false;
                }
            }
            if (fieldInfo == null)
            {
                fieldInfo = Target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (fieldInfo == null)
                {
                    Debug.LogError($"Unknown field name: {fieldName} on ({name})");
                    return false;
                }
            }
            return true;
        }

        public IntEvent OnValueChanged;

        [Serializable]
        public class IntEvent : UnityEvent<int> { }
    }
}