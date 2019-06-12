using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong.Model
{
    [Serializable]
    public struct WaitingData
    {
        public Tile[] HandTiles;
        public Tile[] WaitingTiles;

        public override string ToString()
        {
            return $"HandTiles: {string.Join("", HandTiles)}, "
                + $"WaitingTiles: {string.Join("", WaitingTiles)}";
        }
    }

    [Serializable]
    public struct PlayerHandData
    {
        public Tile[] HandTiles;
        public OpenMeld[] OpenMelds;

        public override string ToString()
        {
            var hands = HandTiles == null ? "Confidential" : string.Join("", HandTiles);
            return $"HandTiles: {hands}, "
                + $"OpenMelds: {string.Join(",", OpenMelds)}";
        }

        public Meld[] Melds => OpenMelds.Select(openMeld => openMeld.Meld).ToArray();
    }
}