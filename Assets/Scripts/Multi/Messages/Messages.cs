using Single;
using Single.MahjongDataType;
using UnityEngine.Networking;

namespace Multi.Messages
{
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
		public Tile Tile;
	}

	public class InTurnOperationMessage : MessageBase
	{
		public int PlayerIndex;
		public InTurnOperation Operation;
		public Meld Meld;
		public int BonusTurnTime;
	}

	public class DiscardTileMessage : MessageBase
	{
		public int PlayerIndex;
		public InTurnOperation Operation;
		public bool DiscardLastDraw;
		public Tile DiscardTile;
		public int BonusTurnTime;
	}

	/// <summary>
	/// If operation is skip, the data in Meld field is useless
	/// If operation is chow, pong or kong, field Meld stores the claimed meld
	/// If operation is rong, field Meld will be of type Single, storing the winning tile
	/// </summary>
	public class OutTurnOperationMessage : MessageBase
	{
		public int PlayerIndex;
		public OutTurnOperation Operation;
		public Meld Meld;
		public int BonusTurnTime;
	}
}
