using System;

namespace Multi.ServerData
{
    [Serializable]
    public struct PointTransfer
    {
        public int From;
        public int To;
        public int Amount;

        public override string ToString() {
            return $"PointTransfer from player {From} to player {To} with amount of {Amount}";
        }
    }
}