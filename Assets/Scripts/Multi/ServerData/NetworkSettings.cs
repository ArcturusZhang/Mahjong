using System;
using Single.MahjongDataType;

namespace Multi.ServerData
{
    [Serializable]
    public struct NetworkSettings
    {
        public GamePlayers GamePlayers;
        public int BaseTurnTime;
        public int LingShangTilesCount;
    }
}