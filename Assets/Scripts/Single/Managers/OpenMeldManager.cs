using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single.Managers
{
    public class OpenMeldManager : MonoBehaviour
    {
        private const float Gap = 0.005f;
        public GameObject[] ChowPongPrefabs;
        public GameObject[] OpenKongPrefabs;
        public GameObject SelfKongPrefabs;
        private float offset = 0;

        public void SetMelds(IList<OpenMeld> melds)
        {
            transform.DestroyAllChild();
            offset = 0;
            foreach (var meld in melds)
            {
                AddMeld(meld);
            }
        }

        public void AddMeld(OpenMeld meld)
        {
            var prefab = SelectPrefab(meld);
            var meldObj = Instantiate(prefab, transform);
            var meldInstance = meldObj.GetComponent<MeldInstance>();
            meldObj.transform.localPosition = new Vector3(0, 0, -offset);
            meldInstance.SetMeld(meld);
            offset += meldInstance.MeldWidth + Gap;
        }

        public void ClearMelds()
        {
            transform.DestroyAllChild();
        }

        private GameObject SelectPrefab(OpenMeld meld)
        {
            if (meld.Side == MeldSide.Self)
                return SelfKongPrefabs;
            if (meld.IsKong && !meld.IsAdded)
                return OpenKongPrefabs[(int)meld.Side];
            return ChowPongPrefabs[(int)meld.Side];
        }
    }
}
