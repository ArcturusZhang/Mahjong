using Single;
using UnityEngine;
using UnityEngine.UI;

namespace UI.RoundEndPanel
{
    public class PlayerPlace : MonoBehaviour
    {
        public Image Number;
        public Image Character;

        public void SetPlace(int place)
        {
            var sprites = ResourceManager.Instance.Place(place);
            Number.sprite = sprites.First;
            Character.sprite = sprites.Second;
        }
    }
}