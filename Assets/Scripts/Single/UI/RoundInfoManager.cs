using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;

namespace Single.UI
{
    public class RoundInfoManager : MonoBehaviour
    {
        public Text FieldInfo;
        public Text RichiSticksInfo;
        public Text ExtraInfo;
        [HideInInspector] public int OyaPlayerIndex;
        [HideInInspector] public int Field;
        [HideInInspector] public int RichiSticks;
        [HideInInspector] public int Extra;

        private void Update()
        {
            if (OyaPlayerIndex < 0) FieldInfo.text = "";
            else
            {
                var fieldWind = MahjongConstants.PositionWinds[Field];
                FieldInfo.text = $"{fieldWind}{OyaPlayerIndex + 1}局";
            }
            RichiSticksInfo.text = RichiSticks.ToString();
            ExtraInfo.text = Extra.ToString();
        }
    }
}