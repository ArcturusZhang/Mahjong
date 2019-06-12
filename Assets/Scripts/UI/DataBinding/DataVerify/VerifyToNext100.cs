using Mahjong.Logic;
using UnityEngine;

namespace UI.DataBinding.DataVerify
{
    public class VerifyToNext100 : Verifier
    {
        public override string Verify(string data, string fallback)
        {
            Debug.Log($"Verifying {data}");
            int value;
            if (int.TryParse(data, out value))
            {
                if (value > 0)
                    return MahjongLogic.ToNextUnit(value, 100).ToString();
            }
            return fallback;
        }
    }
}