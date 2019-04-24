using UnityEngine;

namespace UI.DataBinding.DataVerify
{
    public abstract class Verifier : MonoBehaviour
    {
         public abstract string Verify(string data, string fallback);
    }
}