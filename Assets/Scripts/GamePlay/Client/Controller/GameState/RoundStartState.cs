using System.Collections.Generic;
using Mahjong.Model;

namespace GamePlay.Client.Controller.GameState
{
    public class RoundStartState : ClientState
    {
        public IList<Tile> LocalPlayerHandTiles;
        public int OyaPlayerIndex;
        public int Dice;
        public int Field;
        public int Extra;
        public int RichiSticks;
        public MahjongSetData MahjongSetData;
        public int[] Points;
        public override void OnClientStateEnter()
        {
            // update local tiles
            CurrentRoundStatus.NewRound(OyaPlayerIndex, Dice, Field, Extra, RichiSticks);
            CurrentRoundStatus.SetMahjongSetData(MahjongSetData);
            CurrentRoundStatus.CheckLocalHandTiles(LocalPlayerHandTiles);
            CurrentRoundStatus.SetZhenting(false);
            // update other player's hand tile count
            for (int placeIndex = 0; placeIndex < 4; placeIndex++)
            {
                int playerIndex = CurrentRoundStatus.GetPlayerIndex(placeIndex);
                if (playerIndex < CurrentRoundStatus.TotalPlayers)
                    CurrentRoundStatus.SetHandTiles(placeIndex, LocalPlayerHandTiles.Count);
                else
                    CurrentRoundStatus.SetHandTiles(placeIndex, 0);
            }
            // sync points
            CurrentRoundStatus.UpdatePoints(Points);
            // update ui statement
            var controller = ViewController.Instance;
            // reset yama
            controller.YamaManager.ResetAllTiles();
            // stand hand tiles
            controller.TableTilesManager.StandUp();
            // clear claimed open melds
            controller.TableTilesManager.ClearMelds();
            // send ready message
            ClientBehaviour.Instance.ClientReady();
        }

        public override void OnClientStateExit()
        {
            controller.HandPanelManager.Show();
            controller.HandPanelManager.UnlockTiles();
        }

        public override void OnStateUpdate()
        {
        }
    }
}