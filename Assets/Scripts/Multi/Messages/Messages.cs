using System;
using Multi.ServerData;
using Single;
using Single.MahjongDataType;
using UnityEngine.Networking;

namespace Multi.Messages
{
	public class SceneLoadedMessage : MessageBase
	{
		public int PlayerIndex;
	}
	
	public class ReadinessMessage : MessageBase
	{
		public int PlayerIndex;
	}

	public class InitialDrawingMessage : MessageBase
	{
		public int Dice;
		public int TotalPlayers;
		public int MountainOpenIndex;
		public Tile[] Tiles;
		public int[] DoraIndicatorIndices;
		public Tile[] DoraIndicators;
	}

	public class DrawTileMessage : MessageBase
	{
		public int PlayerIndex;
		public Tile Tile;
		public bool Lingshang;
	}

	public class InTurnOperationMessage : MessageBase
	{
		public int PlayerIndex;
		public InTurnOperation Operation;
		public Tile LastDraw;
		public Meld Meld;
		public int BonusTurnTime;
		public PlayerClientData PlayerClientData;
	}

	[Serializable]
	public struct PlayerClientData
	{
		public Tile[] HandTiles;
		public Meld[] OpenMelds;
		public Tile WinningTile;
		public int WinPlayerIndex;
		public HandStatus HandStatus;
		public RoundStatus RoundStatus;

		public override string ToString()
		{
			return $"HandTiles: {string.Join("", HandTiles)}, OpenMelds: [{string.Join(",", OpenMelds)}], "
			       + $"WinningTile: {WinningTile}, WinningPlayerIndex: {WinPlayerIndex}, HandStatus: {HandStatus}, "
			       + $"RoundStatus: {RoundStatus}";
		}
	}

	public class DiscardTileMessage : MessageBase
	{
		public int PlayerIndex;
		public InTurnOperation Operation;
		public bool DiscardLastDraw;
		public Tile DiscardTile;
		public int BonusTurnTime;
	}
	
	public class OutTurnOperationMessage : MessageBase
	{
		public int PlayerIndex;
		public OutTurnOperation Operation;
		public Tile DiscardTile;
		public Meld Meld;
		public int BonusTurnTime;
		public PlayerClientData PlayerClientData;
	}

	public class DiscardAfterOpenMessage : MessageBase
	{
		public int PlayerIndex;
		public Tile DefaultTile;
		public Tile[] ForbiddenTiles;
	}
}
