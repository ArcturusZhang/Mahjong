using System.Text;
using Mahjong.YakuUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Mahjong
{
    public class MahjongAnalysor : MonoBehaviour
    {
        public Text input;

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(TaskOnClick);
        }

        public void TaskOnClick()
        {
            Debug.Log(input.text);
            var hand = new MahjongHand(input.text);
            var options = YakuOptions.Lizhi | YakuOptions.Menqing | YakuOptions.Zimo;
            var status = new GameStatus();
            Debug.Log($"手牌：{hand}");
            var info = YakuAnalysor.Analyze(hand, status, options);
            var builder = new StringBuilder();
            foreach (var entry in info)
            {
                builder.Append(entry.Key).Append(":\n");
                builder.Append(entry.Value.YakuDetail.ToString()).Append("\n");
            }
            Debug.Log(builder.ToString());
        }
    }
}