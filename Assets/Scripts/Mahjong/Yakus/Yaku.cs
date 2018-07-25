namespace Mahjong.Yakus
{
    public interface Yaku
    {
        string Name { get; }
        int Value { get; }
        bool IsYakuMan { get; }
        bool Test(MianziSet hand, Tile rong, GameStatus status, params YakuOption[] options);
    }
}