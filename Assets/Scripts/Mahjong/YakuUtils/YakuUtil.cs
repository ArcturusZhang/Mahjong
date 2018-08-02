namespace Mahjong.YakuUtils
{
    internal static class YakuUtil
    {
        internal static readonly int YakuManBasePoint = 13; 
        internal static int Count1(int flag)
        {
            if (flag < 0) return -1;
            int count = 0;
            while (flag != 0)
            {
                count += flag & 1;
                flag >>= 1;
            }

            return count;
        }
    }
}