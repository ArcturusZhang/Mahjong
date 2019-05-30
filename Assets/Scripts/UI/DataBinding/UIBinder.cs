using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.DataBinding
{
    public class UIBinder : MonoBehaviour
    {
        [SerializeField] private ScriptableObject target;
        private readonly List<IBindData> cache = new List<IBindData>();

        private void OnEnable()
        {
            cache.Clear();
            var currentAssembly = this.GetType().Assembly;
            var iBindDataTypes = currentAssembly.DefinedTypes
                .Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IBindData)));
            foreach (var type in iBindDataTypes)
            {
                foreach (var obj in GetComponentsInChildren(type.AsType(), true))
                {
                    var bindingInstance = obj as IBindData;
                    if (bindingInstance != null)
                    {
                        cache.Add(bindingInstance);
                        bindingInstance.Target = target;
                    }
                }
            }
        }

        private void Start()
        {
            ApplyBinds();
        }

        private void Update()
        {
            UpdateBinds();
        }

        public virtual void ApplyBinds()
        {
            cache.ForEach(binder => binder.Apply());
        }

        public virtual void UpdateBinds()
        {
            cache.ForEach(binder => binder.UpdateBind());
        }
    }
}