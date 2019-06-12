using Mahjong.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.View.SubManagers
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