namespace Utils
{
    public struct Pair<T1, T2>
    {
        public T1 First { get; }
        public T2 Second { get; }

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }
}