using UnityEngine;

namespace Single.MahjongDataType
{
    [CreateAssetMenu(menuName = "Mahjong/GameSettings")]
    public class GameSettings : ScriptableObject
    {
        [Header("General settings")] public int InitialPoints = 25000;
        public int DiceMin = 2;
        public int DiceMax = 12;
        public int MountainReservedTiles = 14;
        
        [Header("Tile drawing settings")] public int InitialDrawRound = 3;
        public int TilesEveryRound = 4;
        public int TilesLastRound = 1;
        public int TilesPerRowInRiver = 6;
        public int MaxRowInRiver = 3;

        [Header("Time settings")] public int BaseTurnTime = 5;
        public int BonusTurnTime = 20;
        public int ServerBufferTime = 2; // extra waiting time for bad networking
        public float PlayerHandTilesSortDelay = 1f;
        public float AutoDiscardDelayAfterRichi = 0.5f;
    }
}