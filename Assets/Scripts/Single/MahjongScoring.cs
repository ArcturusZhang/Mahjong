using System;
using System.Collections.Generic;
using System.Linq;
using Multi;
using Multi.ServerData;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;

namespace Single
{
    public static class MahjongScoring
    {
        public static PointsTransfer[] GetPointsTransfers(RoundEndType type, NetworkRoundStatus roundStatus,
            GameStatus gameStatus, params PlayerServerData[] data)
        {
            switch (type)
            {
                case RoundEndType.Tsumo:
                    Assert.AreEqual(data.Length, 1, "When tsumo, there should only be the winning player's data");
                    return GetPointsTransfersForTsumo(roundStatus, gameStatus, data[0]);
                case RoundEndType.Rong:
                    return GetPointsTransfersForRong(roundStatus, gameStatus, data);
                case RoundEndType.Draw:
                    Assert.AreEqual(data.Length, gameStatus.TotalPlayer, "Not enough data to analyse hand readiness.");
                    return GetPointsTransfersForDraw(data);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "No such RoundEndType value");
            }
        }

        private static PointsTransfer[] GetPointsTransfersForTsumo(NetworkRoundStatus roundStatus,
            GameStatus gameStatus, PlayerServerData data)
        {
            var transfers = new List<PointsTransfer>();
            var current = gameStatus.CurrentPlayerIndex;
            var currentMultiplier = GetMultiplier(roundStatus, current);
            var point = GetPointInfo(data, MahjongManager.Instance.YakuSettings);
            for (int i = gameStatus.NextPlayerIndex(); i != current; i = gameStatus.NextPlayerIndex(i))
            {
                var victimMultiplier = GetMultiplier(roundStatus, i);
                var transfer = new PointsTransfer
                {
                    From = i, To = current, Amount = point.BasePoint * currentMultiplier * victimMultiplier
                };
                transfers.Add(transfer);
            }

            return transfers.ToArray();
        }

        private static PointsTransfer[] GetPointsTransfersForRong(NetworkRoundStatus roundStatus, GameStatus gameStatus,
            PlayerServerData[] data)
        {
            var transfers = new List<PointsTransfer>();
            var current = gameStatus.CurrentPlayerIndex;
            for (int i = 0; i < data.Length; i++)
            {
                var index = data[i].PlayerIndex;
                var point = GetPointInfo(data[i], MahjongManager.Instance.YakuSettings);
                var multiplier = roundStatus.IsDealer(index) ? 2 * gameStatus.TotalPlayer : gameStatus.TotalPlayer;
                var transfer = new PointsTransfer
                {
                    From = current, To = index, Amount = point.BasePoint * multiplier
                };
                transfers.Add(transfer);
            }
            return transfers.ToArray();
        }

        private static PointsTransfer[] GetPointsTransfersForDraw(PlayerServerData[] data)
        {
            throw new NotImplementedException();
        }

        private static int GetMultiplier(NetworkRoundStatus roundStatus, int index)
        {
            return roundStatus.IsDealer(index) ? 2 : 1;
        }

        private static PointInfo GetPointInfo(PlayerServerData data, YakuSettings yakuSettings)
        {
            return MahjongLogic.GetPointInfo(data.HandTiles, data.OpenMelds, data.WinningTile, data.HandStatus,
                data.RoundStatus, yakuSettings, data.DoraIndicators, data.UraDoraIndicators);
        }
    }
}