using System;
using GamePlay.Client.View.SubManagers;
using GamePlay.Server.Model;
using UnityEngine;

namespace GamePlay.Client.View
{
    public class PlayerEffectManager : MonoBehaviour
    {
        public EffectManager[] EffectManagers;

        public float ShowEffect(int placeIndex, Type type)
        {
            return EffectManagers[placeIndex].StartAnimation(type);
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
            Richi, Chow, Pong, Kong, Bei, Tsumo, Rong
        }
    }
}
