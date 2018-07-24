using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Mahjong
{
    public class MahjongAnalysor : MonoBehaviour
    {

        public Text input;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TaskOnClick()
        {
            var hand = new MahjongHand(input.text);
            // string input = "456m1m2m3m978s11z222z";
            // var hand = new MahjongHand(input);
            // var hand = new MahjongHand("112233s223344p44z");
            // var hand = new MahjongHand("11112345678999s");
            // var hand = new MahjongHand("11122233344455p");
            if (hand.HasWin)
                foreach (var mianziSet in hand.Decomposition)
                {
                    // var builder = new StringBuilder();
                    // foreach (var mianzi in list)
                    // {
                    //     builder.Append(mianzi).Append(" ");
                    // }
                    Debug.Log(mianziSet.ToString());
                }
            else
            {
                if (hand.HasTing)
                {
                    var builder = new StringBuilder();
                    foreach (var tile in hand.TingList)
                    {
                        builder.Append(tile).Append(" ");
                    }
                    Debug.Log(builder.ToString());
                }
            }
        }
    }
}
