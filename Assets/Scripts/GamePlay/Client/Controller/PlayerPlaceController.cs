using System.Collections;
using UI.ResourcesBundle;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Client.Controller
{
    public class PlayerPlaceController : MonoBehaviour
    {
        public Image Background;
        public Image PlaceNumber;
        public Image PlaceCharacter;
        public SpriteBundle NumberBundle;
        public SpriteBundle CharacterBundle;

        public void SetPlace(int place)
        {
            PlaceNumber.sprite = NumberBundle.Get(place);
            PlaceCharacter.sprite = CharacterBundle.Get(place);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            // todo -- animation
        }

        public void Close() {
            gameObject.SetActive(false);
        }
    }
}