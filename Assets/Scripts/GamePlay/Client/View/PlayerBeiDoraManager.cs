using Mahjong.Model;
using UnityEngine;

namespace GamePlay.Client.View
{
    public class PlayerBeiDoraManager : MonoBehaviour
    {
        public TileInstance[] Tiles;

        private void Awake()
        {
            for (int i = 0; i < Tiles.Length; i++)
            {
                Tiles[i].SetTile(new Tile(Suit.Z, 4));
                Tiles[i].gameObject.SetActive(false);
            }
        }

        public void SetBeiDoras(int count)
        {
            for (int i = 0; i < Tiles.Length; i++)
                Tiles[i].gameObject.SetActive(i < count);
        }
    }
}
