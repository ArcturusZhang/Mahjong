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
		public Tile Tile;
	}

	public class InTurnOperationMessage : MessageBase
	{
		public int PlayerIndex;
		public InTurnOperation Operation;
		public Meld Meld;
		public int BonusTurnTime;
		public PointInfo PointInfo;
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
		public Tile DiscardedTile;
		public Meld Meld;
		public int BonusTurnTime;
		public PointInfo PointInfo;
	}

	public class DiscardAfterOpenMessage : MessageBase
	{
		public int PlayerIndex;
		public Tile DefaultTile;
	}

	public class LingshangTileDrawnMessage : MessageBase
	{
		public int PlayerIndex;
		public Tile Lingshang;
	}
}
