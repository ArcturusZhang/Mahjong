using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Mahjong.Logic;

namespace GamePlay.Client.Controller
{
    [RequireComponent(typeof(Image))]
    public class RoundDrawItemController : MonoBehaviour
    {
        private Image image;

        private void OnEnable()
        {
            image = GetComponent<Image>();
            image.DOFade(1, MahjongConstants.FadeDuration);
        }

        public void Fade()
        {
            image.DOFade(0, MahjongConstants.FadeDuration);
        }

        private void OnDisable()
        {
            image.color = new Color(1, 1, 1, 0);
        }
    }
}
