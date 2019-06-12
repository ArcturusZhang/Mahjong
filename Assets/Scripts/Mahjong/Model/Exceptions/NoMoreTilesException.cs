namespace Mahjong.Model.Exceptions
{
    [System.Serializable]
    public class NoMoreTilesException : System.Exception
    {
        public NoMoreTilesException() { }
        public NoMoreTilesException(string message) : base(message) { }
        public NoMoreTilesException(string message, System.Exception inner) : base(message, inner) { }
        protected NoMoreTilesException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}