using System;

namespace Single.MahjongDataType
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
        public Meld[] OpenMelds;

        public override string ToString()
        {
            return $"HandTiles: {string.Join("", HandTiles)}, "
                + $"OpenMelds: {string.Join(",", OpenMelds)}";
        }
    }
}