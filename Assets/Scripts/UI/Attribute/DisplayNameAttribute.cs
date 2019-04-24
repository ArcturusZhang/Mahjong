using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class DisplayNameAttribute : System.Attribute
    {
        public readonly string displayName;
        
        public DisplayNameAttribute(string name)
        {
            this.displayName = name;
        }
        
        public string DisplayName
        {
            get { return displayName; }
        }
    }
}
