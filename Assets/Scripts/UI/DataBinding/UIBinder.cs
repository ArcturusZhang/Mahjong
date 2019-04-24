using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.DataBinding
{
    public class UIBinder : MonoBehaviour
    {
        [SerializeField] private ScriptableObject target;
        private readonly IList<IBindData> cache = new List<IBindData>();

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
                    Debug.Log($"Binding {obj.name}");
                    var bindingInstance = obj as IBindData;
                    if (bindingInstance != null)
                    {
                        cache.Add(bindingInstance);
                        bindingInstance.Target = target;
                        bindingInstance.Apply();
                    }
                }
            }
        }

        private void Update()
        {
            foreach (var data in cache)
            {
                data.UpdateBind();
            }
        }
    }
}