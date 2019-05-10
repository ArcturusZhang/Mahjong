using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single.Managers
{
    public class TableTilesManager : ManagerBase
    {
        public PlayerHandManager[] HandManagers;
        public OpenMeldManager[] OpenManagers;
        public PlayerRiverManager[] RiverManagers;

        private void Update()
        {
            if (CurrentRoundStatus == null) return;
            UpdateHands();
            UpdateRivers();
        }

        private void UpdateHands()
        {
            for (int placeIndex = 0; placeIndex < HandManagers.Length; placeIndex++)
            {
                var hand = HandManagers[placeIndex];
                hand.Count = CurrentRoundStatus.GetTileCount(placeIndex);
                hand.LastDraw = CurrentRoundStatus.GetLastDraw(placeIndex);
            }
            HandManagers[0].HandTiles = CurrentRoundStatus.LocalPlayerHandTiles;
        }

        public void SetMelds(int placeIndex, IList<OpenMeld> melds)
        {
            OpenManagers[placeIndex].SetMelds(melds);
        }

        public void ClearMelds(int placeIndex)
        {
            OpenManagers[placeIndex].ClearMelds();
        }

        public void ClearMelds()
        {
            System.Array.ForEach(OpenManagers, m => m.ClearMelds());
        }

        private void UpdateRivers()
        {
            for (int placeIndex = 0; placeIndex < RiverManagers.Length; placeIndex++)
            {
                var manager = RiverManagers[placeIndex];
                manager.RiverTiles = CurrentRoundStatus.GetRiverTiles(placeIndex);
            }
        }

        public void DiscardTile(int placeIndex, bool discardingLastDraw)
        {
            HandManagers[placeIndex].DiscardTile(discardingLastDraw);
        }

        public void SetHandTiles(int placeIndex, IList<Tile> handTiles)
        {
            HandManagers[placeIndex].HandTiles = handTiles;
        }

        public void StandUp(int placeIndex)
        {
            HandManagers[placeIndex].StandUp();
        }

        public void StandUp()
        {
            System.Array.ForEach(HandManagers, manager => manager.StandUp());
        }

        public void OpenUp(int placeIndex)
        {
            HandManagers[placeIndex].OpenUp();
        }

        public void OpenUp()
        {
            System.Array.ForEach(HandManagers, m => m.OpenUp());
        }

        public void CloseDown(int placeIndex)
        {
            HandManagers[placeIndex].CloseDown();
        }

        public void CloseDown()
        {
            System.Array.ForEach(HandManagers, m => m.CloseDown());
        }
    }
}
