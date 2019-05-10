using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using Single.Managers;
using Single.UI.SubManagers;
using UnityEngine;


namespace Single.UI
{
    public class BoardInfoManager : ManagerBase
    {
        [SerializeField] private RoundInfoManager RoundInfoManager;
        [SerializeField] private PointsManager PointsManager;
        [SerializeField] private PositionManager PositionManager;
        [SerializeField] private RichiStatusManager RichiStatusManager;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            UpdateRoundInfo();
            UpdatePoints();
            UpdatePosition();
            UpdateRichiStatus();
        }

        public void UpdateRoundInfo()
        {
            RoundInfoManager.OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex;
            RoundInfoManager.Field = CurrentRoundStatus.Field;
            RoundInfoManager.Extra = CurrentRoundStatus.Extra;
            RoundInfoManager.RichiSticks = CurrentRoundStatus.RichiSticks;
        }

        public void UpdatePoints()
        {
            PointsManager.TotalPlayers = CurrentRoundStatus.TotalPlayers;
            PointsManager.Places = CurrentRoundStatus.Places;
            PointsManager.Points = CurrentRoundStatus.Points;
        }

        public void UpdatePosition()
        {
            PositionManager.TotalPlayers = CurrentRoundStatus.TotalPlayers;
            PositionManager.OyaPlayerIndex = CurrentRoundStatus.OyaPlayerIndex;
            PositionManager.Places = CurrentRoundStatus.Places;
        }

        public void UpdateRichiStatus()
        {
            RichiStatusManager.TotalPlayers = CurrentRoundStatus.TotalPlayers;
            RichiStatusManager.Places = CurrentRoundStatus.Places;
            RichiStatusManager.RichiStatus = CurrentRoundStatus.RichiStatus;
        }
    }
}
