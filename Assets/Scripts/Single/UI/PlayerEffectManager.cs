using System;
using System.Collections;
using Single.UI.SubManagers;
using UnityEngine;

namespace Single.UI
{
    public class PlayerEffectManager : MonoBehaviour
    {
        public EffectManager[] EffectManagers;
        private WaitForSeconds wait = new WaitForSeconds(MahjongConstants.AnimationDelay);

        public IEnumerator ShowEffect(int placeIndex, AnimationType type)
        {
            yield return EffectManagers[placeIndex].StartAnimation(type);
        }

        public static AnimationType GetAnimationType(OutTurnOperationType operation)
        {
            switch (operation)
            {
                case OutTurnOperationType.Chow:
                    return AnimationType.Chow;
                case OutTurnOperationType.Pong:
                    return AnimationType.Pong;
                case OutTurnOperationType.Kong:
                    return AnimationType.Kong;
                case OutTurnOperationType.Rong:
                    return AnimationType.Rong;
                default:
                    throw new NotSupportedException($"This kind of operation {operation} does not have an animation");
            }
        }
    }

    public enum AnimationType
    {
        Richi, Chow, Pong, Kong, Tsumo, Rong
    }
}
