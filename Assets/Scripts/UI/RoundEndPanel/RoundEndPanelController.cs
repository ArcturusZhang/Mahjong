using System.Collections.Generic;
using Multi;
using UnityEngine;
using UnityEngine.UI;

namespace UI.RoundEndPanel
{
	public class RoundEndPanelController : MonoBehaviour
	{
		public PlayerInfoPanel[] PlayerInfoPanels;
		public Image[] Arrows0;
		public Image[] Arrows1;
		public Image[] Arrows2;
		public Image[] Arrows3;
		
		public List<Player> Players { private get; set; }

		private Image[][] Arrows;

		private void Awake()
		{
			Arrows = new[] {Arrows0, Arrows1, Arrows2, Arrows3};
		}
	}
}
