namespace Multi.MahjongMessages
{
    public static class MessageIds
    {
        // Server to client messages
        public const short ServerPrepareMessage = 100;
        public const short ServerRoundStartMessage = 101;
        public const short ServerDrawTileMessage = 102;
        public const short ServerOtherDrawTileMessage = 103;
        public const short ServerDiscardOperationMessage = 104;
        public const short ServerTurnEndMessage = 105;
        public const short ServerTsumoMessage = 106;
        public const short ServerRongMessage = 107;
        public const short ServerRoundDrawMessage = 108;

        // Client to server messages
        public const short ClientReadinessMessage = 200;
        public const short ClientDiscardTileMessage = 201;
        public const short ClientInTurnOperationMessage = 202;
        public const short ClientOutTurnOperationMessage = 203;
        public const short ClientNextRoundMessage = 300;
    }
}