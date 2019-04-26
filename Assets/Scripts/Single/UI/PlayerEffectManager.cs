using System;
using System.Collections;
using Multi;
using Single.UI.SubManagers;
using UnityEngine;

namespace Single.UI
{
    public class PlayerEffectManager : MonoBehaviour
    {
        public EffectManager[] EffectManagers;
        private WaitForSeconds wait = new WaitForSeconds(MahjongConstants.AnimationDelay);

        public IEnumerator ShowEffect(int placeIndex, Type type)
        {
            yield return EffectManagers[placeIndex].StartAnimation(type);
        }

        public static Type GetAnimationType(OutTurnOperationType operation)
        {
            switch (operation)
            {
                case OutTurnOperationType.Chow:
                    return Type.Chow;
                case OutTurnOperationType.Pong:
                    return Type.Pong;
                case OutTurnOperationType.Kong:
                    return Type.Kong;
                case OutTurnOperationType.Rong:
                    return Type.Rong;
                default:
                    throw new NotSupportedException($"This kind of operation {operation} does not have an animation");
            }
        }

        public enum Type
        {
            Richi, Chow, Pong, Kong, Tsumo, Rong
        }
    }
}
