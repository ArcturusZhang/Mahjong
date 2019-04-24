using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UI.DataBinding
{
    public class UIDataBindBoostrap : MonoBehaviour
    {
        private readonly IList<IBindData> cache = new List<IBindData>();
        private void Start()
        {
            var currentAssembly = this.GetType().Assembly;

            // we filter the defined classes according to the interfaces they implement
            var iBindDataTypes = currentAssembly.DefinedTypes
                .Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IBindData)))
                .Select(typeInfo => typeInfo.AsType());
            foreach (Type type in iBindDataTypes)
            {
                foreach (var obj in GameObject.FindObjectsOfType(type))
                {
                    Debug.Log($"Binding {obj.name}");
                    IBindData bindingInstance = obj as IBindData;
                    if (bindingInstance != null)
                    {
                        cache.Add(bindingInstance);
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
