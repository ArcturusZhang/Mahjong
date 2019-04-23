using Single;
using Single.MahjongDataType;

namespace Multi.ServerData
{
    public static class ServerMahjongLogic
    {
        /// <summary>
        /// Note: lingshang and haidi should be obtained in baseHandStatus
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="CurrentRoundStatus"></param>
        /// <param name="winningTile"></param>
        /// <param name="baseHandStatus"></param>
        /// <returns></returns>
        public static PointInfo GetPointInfo(int playerIndex, ServerRoundStatus CurrentRoundStatus, Tile winningTile,
            HandStatus baseHandStatus, Tile[] doraTiles, Tile[] uraDoraTiles, YakuSettings yakuSettings)
        {
            var handData = CurrentRoundStatus.HandData(playerIndex);
            var handStatus = baseHandStatus;
            if (MahjongLogic.TestMenqing(handData.OpenMelds))
                handStatus |= HandStatus.Menqing;
            // test richi
            if (CurrentRoundStatus.RichiStatus[playerIndex])
            {
                handStatus |= HandStatus.Richi;
                // test one-shot
                if (CurrentRoundStatus.OneShotStatus[playerIndex])
                    handStatus |= HandStatus.OneShot;
                // test WRichi -- todo
            }
            // test first turn -- todo
            var roundStatus = new RoundStatus
            {
                PlayerIndex = playerIndex,
                OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex,
                CurrentExtraRound = CurrentRoundStatus.Extra,
                RichiSticks = CurrentRoundStatus.RichiSticks,
                FieldCount = CurrentRoundStatus.Field,
                TotalPlayer = CurrentRoundStatus.TotalPlayers
            };
            return MahjongLogic.GetPointInfo(handData.HandTiles, handData.OpenMelds, winningTile,
                handStatus, roundStatus, yakuSettings, doraTiles, uraDoraTiles);
        }
    }
}