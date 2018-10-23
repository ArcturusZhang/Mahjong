using UnityEngine;

namespace Multi.GameState
{
    public class RoundEndState : AbstractMahjongState
    {
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            Debug.Log($"Round ends!");
        }
    }
}