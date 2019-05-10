using Single.MahjongDataType;
using UnityEngine;

namespace Single.Managers
{
    public abstract class ManagerBase : MonoBehaviour
    {
        public virtual ClientRoundStatus CurrentRoundStatus { get; set; }
    }
}