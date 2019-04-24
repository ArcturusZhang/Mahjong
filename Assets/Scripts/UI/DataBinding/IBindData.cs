using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.DataBinding
{
    public interface IBindData
    {
        ScriptableObject Target { get; set; }
        void Apply();
        void UpdateBind();
    }
}
