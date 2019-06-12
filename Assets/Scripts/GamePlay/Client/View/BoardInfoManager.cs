using Common.Interfaces;
using GamePlay.Client.Model;
using GamePlay.Client.View.SubManagers;
using UnityEngine;


namespace GamePlay.Client.View
{
    public class BoardInfoManager : MonoBehaviour, IObserver<ClientRoundStatus>
    {
        [SerializeField] private RoundInfoManager RoundInfoManager;
        [SerializeField] private PointsManager PointsManager;
        [SerializeField] private PositionManager PositionManager;
        [SerializeField] private RichiStatusManager RichiStatusManager;
        [SerializeField] private CurrentPlayerIndicatorManager IndicatorManager;

        public void UpdateCurrentPlayer(ClientRoundStatus status)
        {
            IndicatorManager.CurrentPlaceIndex = status.CurrentPlaceIndex;
        }

        public void UpdateRoundInfo(ClientRoundStatus status)
        {
            RoundInfoManager.OyaPlayerIndex = status.OyaPlayerIndex;
            RoundInfoManager.Field = status.Field;
            RoundInfoManager.Extra = status.Extra;
            RoundInfoManager.RichiSticks = status.RichiSticks;
        }

        public void UpdatePoints(ClientRoundStatus status)
        {
            PointsManager.TotalPlayers = status.TotalPlayers;
            PointsManager.Places = status.Places;
            PointsManager.Points = status.Points;
        }

        public void UpdatePosition(ClientRoundStatus status)
        {
            PositionManager.TotalPlayers = status.TotalPlayers;
            PositionManager.OyaPlayerIndex = status.OyaPlayerIndex;
            PositionManager.Places = status.Places;
        }

        public void UpdateRichiStatus(ClientRoundStatus status)
        {
            RichiStatusManager.TotalPlayers = status.TotalPlayers;
            RichiStatusManager.Places = status.Places;
            RichiStatusManager.RichiStatus = status.RichiStatus;
        }

        public void UpdateStatus(ClientRoundStatus subject)
        {
            UpdateCurrentPlayer(subject);
            UpdateRoundInfo(subject);
            UpdatePoints(subject);
            UpdatePosition(subject);
            UpdateRichiStatus(subject);
        }
    }
}
