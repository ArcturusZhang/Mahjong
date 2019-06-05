namespace Single.MahjongDataType.Interfaces
{
    public interface IObserver<in T>
    {
        void UpdateStatus(T subject);
    }
}