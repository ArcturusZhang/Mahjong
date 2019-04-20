using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = Single.Debug;

namespace Single.UI
{
    public class BoardInfoManager : MonoBehaviour
    {
        [SerializeField] private RoundInfoManager RoundInfoManager;
        [SerializeField] private PointsManager PointsManager;
        [SerializeField] private PositionManager PositionManager;
        [SerializeField] private RichiStatusManager RichiStatusManager;
        private void Start()
        {
            RoundInfoManager = GetComponentInChildren<RoundInfoManager>();
            PointsManager = GetComponentInChildren<PointsManager>();
            PositionManager = GetComponentInChildren<PositionManager>();
            RichiStatusManager = GetComponentInChildren<RichiStatusManager>();
        }

        public void UpdateRoundInfo(int oya, int field, int extra, int richiSticks)
        {
            if (RoundInfoManager == null)
            {
                Debug.LogWarning("RoundInfoManager is null");
                return;
            }
            RoundInfoManager.OyaPlayerIndex = oya;
            RoundInfoManager.Field = field;
            RoundInfoManager.Extra = extra;
            RoundInfoManager.RichiSticks = richiSticks;
        }

        public void UpdatePoints(int totalPlayers, int[] places, int[] points)
        {
            if (PointsManager == null) {
                Debug.LogWarning("PointsManager is null");
                return;
            }
            PointsManager.TotalPlayers = totalPlayers;
            PointsManager.Places = places;
            PointsManager.Points = points;
        }

        public void UpdatePosition(int totalPlayers, int oya, int[] places) {
            if (PositionManager == null) {
                Debug.LogWarning("PositionManager is null");
                return;
            }
            PositionManager.TotalPlayers = totalPlayers;
            PositionManager.OyaPlayerIndex = oya;
            PositionManager.Places = places;
        }

        public void UpdateRichiStatus(int totalPlayers, int[] places, bool[] richiStatus) {
            if (RichiStatusManager == null) {
                Debug.LogWarning("RichiStatusManager is null");
                return;
            }
            RichiStatusManager.TotalPlayers = totalPlayers;
            RichiStatusManager.Places = places;
            RichiStatusManager.RichiStatus = richiStatus;
        }
    }
}
