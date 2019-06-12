using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UI.ResourcesBundle;
using GamePlay.Client.Controller;

namespace GamePlay.Client.View.SubManagers
{
    public class PlayerPlaceManager : MonoBehaviour
    {
        public Text PlayerNameText;
        public NumberPanelController PointController;
        public Image PlaceNumber;
        public Image PlaceCharacter;
        public SpriteBundle NumberBundle;
        public SpriteBundle CharacterBundle;
        private RectTransform rect;

        public void SetPoints(string playerName, int points, int place)
        {
            rect = GetComponent<RectTransform>();
            PlayerNameText.text = playerName;
            PointController.SetNumber(points);
            PlaceNumber.sprite = NumberBundle.Get(place);
            PlaceCharacter.sprite = CharacterBundle.Get(place);
        }

        public float Show()
        {
            gameObject.SetActive(true);
            var position = rect.anchoredPosition;
            rect.anchoredPosition = position - new Vector2(DisplacementX, 0);
            var t = rect.DOAnchorPos(position, Duration).SetEase(Ease.OutQuad);
            return t.Duration();
        }

        private const float DisplacementX = 50f;
        private const float Duration = 1f;
    }
}
