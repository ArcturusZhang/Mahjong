using System.Collections.Generic;
using Mahjong.Model;

namespace GamePlay.Client.Model
{
    public struct PlayerHandInfo
    {
        public IList<Tile> HandTiles;
        public IList<OpenMeld> OpenMelds;
        public Tile WinningTile;
        public IList<Tile> DoraIndicators;
        public IList<Tile> UraDoraIndicators;
        public bool IsTsumo;
        public bool IsRichi;
    }
}