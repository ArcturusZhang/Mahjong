namespace Multi
{
    public static class GameSettings
    {
        #region MahjongBasicConstants

        public const int InitialDrawRound = 3;
        public const int TilesEveryRound = 4;
        public const int TilesLastRound = 1;
        public const int DiceMin = 2;
        public const int DiceMax = 12;
        public const int TilesPerRowInRiver = 6;
        public const int MaxRowInRiver = 3;

        #endregion

        #region TimeSettings

        public const float PlayerHandTilesSortDelay = 1f;
//        public const int BaseTurnTime = 2;
        public const int ServerBufferTime = 2; // extra waiting time for bad networking
        public const float AutoDiscardDelayAfterRichi = 0.5f;

        #endregion
    }
}