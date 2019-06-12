using Mahjong.Logic;
using Mahjong.Model;

namespace GamePlay.Server.Model
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
            HandStatus baseHandStatus, Tile[] doraTiles, Tile[] uraDoraTiles, int beiDora, GameSetting yakuSettings)
        {
            var zhenting = CurrentRoundStatus.IsZhenting(playerIndex);
            if (zhenting && !baseHandStatus.HasFlag(HandStatus.Tsumo)) return new PointInfo();
            var handData = CurrentRoundStatus.HandData(playerIndex);
            var handStatus = baseHandStatus;
            if (MahjongLogic.TestMenqing(handData.Melds))
                handStatus |= HandStatus.Menqing;
            // test richi
            if (CurrentRoundStatus.RichiStatus(playerIndex))
            {
                handStatus |= HandStatus.Richi;
                // test one-shot
                if (yakuSettings.HasOneShot && CurrentRoundStatus.OneShotStatus(playerIndex))
                    handStatus |= HandStatus.OneShot;
                // test WRichi
                if (CurrentRoundStatus.FirstTurn)
                    handStatus |= HandStatus.WRichi;
            }
            // test first turn
            if (CurrentRoundStatus.FirstTurn)
                handStatus |= HandStatus.FirstTurn;
            var roundStatus = new RoundStatus
            {
                PlayerIndex = playerIndex,
                OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex,
                CurrentExtraRound = CurrentRoundStatus.Extra,
                RichiSticks = CurrentRoundStatus.RichiSticks,
                FieldCount = CurrentRoundStatus.Field,
                TotalPlayer = CurrentRoundStatus.TotalPlayers
            };
            var isQTJ = CurrentRoundStatus.GameSettings.GameMode == GameMode.QTJ;
            return MahjongLogic.GetPointInfo(handData.HandTiles, handData.Melds, winningTile,
                handStatus, roundStatus, yakuSettings, isQTJ, doraTiles, uraDoraTiles, beiDora);
        }
    }
}