using System.Collections;
using System.Collections.Generic;
using Single.MahjongDataType;
using UnityEngine;
using Utils;

namespace Single
{
    public class OpenMeldManager : MonoBehaviour
    {
        private const float Gap = 0.005f;
        public GameObject[] ChowPongPrefabs;
        public GameObject[] AddedKongPrefabs;
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

        // private void FixedUpdate()
        // {
        //     if (Input.GetKey(KeyCode.A))
        //     {
        //         var list = new List<OpenMeld> {
        //             new OpenMeld {
        //                 Meld = new Meld(true, new Tile(Suit.M, 1), new Tile(Suit.M, 2), new Tile(Suit.M, 3)),
        //                 DiscardTile = new Tile(Suit.M, 2),
        //                 Side = MeldSide.Left
        //             },
        //             new OpenMeld {
        //                 Meld = new Meld(false, new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5), new Tile(Suit.Z, 5)),
        //                 Side = MeldSide.Self
        //             },
        //             new OpenMeld {
        //                 Meld = new Meld(false, new Tile(Suit.M, 5, true), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5), new Tile(Suit.M, 5)),
        //                 Side = MeldSide.Self
        //             },
        //             new OpenMeld {
        //                 Meld = new Meld(false, new Tile(Suit.M, 5), new Tile(Suit.M, 5, true), new Tile(Suit.M, 5), new Tile(Suit.M, 5)),
        //                 Side = MeldSide.Self
        //             }
        //         };
        //         SetMelds(list);
        //     }
        // }

        private GameObject SelectPrefab(OpenMeld meld)
        {
            if (meld.Side == MeldSide.Self)
                return SelfKongPrefabs;
            if (meld.IsKong)
                return OpenKongPrefabs[(int)meld.Side];
            return ChowPongPrefabs[(int)meld.Side];
            // todo -- added kong prefab
        }
    }
}
