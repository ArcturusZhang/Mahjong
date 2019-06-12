using System;
using UnityEngine.Assertions;

namespace Mahjong.Model
{
    [Flags]
    public enum HandStatus
    {
        Nothing = 1 << 0,
        Menqing = 1 << 1,
        Tsumo = 1 << 2,
        Richi = 1 << 3,
        WRichi = 1 << 4,
        FirstTurn = 1 << 5,
        Lingshang = 1 << 6,
        Haidi = 1 << 7,
        RobKong = 1 << 8,
        OneShot = 1 << 9
    }

    [Serializable]
    public struct RoundStatus
    {
        public int PlayerIndex;
        public int OyaPlayerIndex;
        public int CurrentExtraRound;
        public int RichiSticks;
        public int FieldCount; // Starts with 0
        public int TotalPlayer;

        public Tile SelfWind
        {
            get
            {
                int offSet = PlayerIndex - OyaPlayerIndex;
                if (offSet < 0) offSet += TotalPlayer;
                Assert.IsTrue(offSet >= 0 && offSet < 4, "Self wind should be one of E, S, W, N");
                return new Tile(Suit.Z, offSet + 1);
            }
        }

        public Tile PrevailingWind => new Tile(Suit.Z, FieldCount + 1);

        public bool IsDealer => PlayerIndex == OyaPlayerIndex;

        public override string ToString()
        {
            return $"PlayerIndex: {PlayerIndex}, OyaPlayerIndex: {OyaPlayerIndex}, CurrentExtraRound: {CurrentExtraRound}, "
                   + $"RichiSticks: {RichiSticks}, FieldCount: {FieldCount}, TotalPlayer: {TotalPlayer}";
        }
    }
}