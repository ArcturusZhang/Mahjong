using System.Collections;
using System.Collections.Generic;
using Multi;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.UI;
using Debug = Single.Debug;

public class HandTileInstance : MonoBehaviour
{
    public Tile Tile;
    public Image TileImage;
    public bool IsLastDraw;

    [SerializeField] private ResourceManager manager;

    private void Update()
    {
        if (manager == null) manager = ResourceManager.Instance;
        var sprite = manager.GetTileSprite(Tile);
        TileImage.sprite = sprite;
    }

    public void OnClick()
    {
        Debug.Log($"Requesting discard tile {Tile}");
        ClientBehaviour.Instance.OnDiscardTile(Tile, IsLastDraw);
    }
}
