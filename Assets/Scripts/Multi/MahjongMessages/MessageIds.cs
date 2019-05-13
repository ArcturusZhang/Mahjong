namespace Multi.MahjongMessages
{
    public static class MessageIds
    {
        // Server to client messages
        public const short ServerGamePrepareMessage = 100;
        public const short ServerRoundStartMessage = 101;
        public const short ServerDrawTileMessage = 102;
        public const short ServerDiscardOperationMessage = 103;
        public const short ServerKongMessage = 104;
        public const short ServerOperationPerformMessage = 105;
        public const short ServerTurnEndMessage = 110;
        public const short ServerTsumoMessage = 111;
        public const short ServerRongMessage = 112;
        public const short ServerRoundDrawMessage = 113;
        public const short ServerPointTransferMessage = 114;
        public const short ServerGameEndMessage = 115;

        // Client to server messages
        public const short ClientReadinessMessage = 200;
        public const short ClientDiscardTileMessage = 201;
        public const short ClientInTurnOperationMessage = 202;
        public const short ClientOutTurnOperationMessage = 203;
        public const short ClientNextRoundMessage = 300;
    }
}