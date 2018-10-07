using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Multi
{
	public class PlayerManager : NetworkBehaviour
	{
		public static PlayerManager Instance { get; private set; }
		
		[Header("Player Infos")]
		public List<Player> Players = new List<Player>();

		public Player LocalPlayer;

		private void Awake()
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		public void AddPlayer(Player player)
		{
			Players.Add(player);
		}

		public void RemovePlayer(Player player)
		{
			Players.Remove(player);
		}

		public void ClearPlayers()
		{
			Players.Clear();
		}
	}
}
