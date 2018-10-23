using System.Collections.Generic;
using Single;
using Single.MahjongDataType;
using UnityEngine;
using UnityEngine.Assertions;

namespace Multi
{
    public class PlayerOpenHolder : MonoBehaviour
    {
        public MahjongSelector MahjongSelector;

        private float offset = 0f;
        private List<MeldInstance> meldInstances;

        private void Awake()
        {
            meldInstances = new List<MeldInstance>();
        }

        public void Open(Meld meld, Tile discardTile, MeldInstanceType instanceType)
        {
            var prefab = MahjongSelector.PrefabDict[instanceType];
            var meldObject = Instantiate(prefab, transform);
            meldObject.transform.localPosition = new Vector3(0, 0, offset);
            meldObject.transform.localRotation = Quaternion.identity;
            var meldInstance = meldObject.GetComponent<MeldInstance>();
            meldInstance.SetMeld(meld, discardTile, instanceType);
            offset -= meldInstance.MeldWidth + MahjongConstants.Gap;
            meldInstances.Add(meldInstance);
        }

        public void ConcealedKong(Meld meld)
        {
            var prefab = MahjongSelector.PrefabDict[MeldInstanceType.SelfKong];
            var meldObject = Instantiate(prefab, transform);
            meldObject.transform.localPosition = new Vector3(0, 0, offset);
            meldObject.transform.localRotation = Quaternion.identity;
            var meldInstance = meldObject.GetComponent<MeldInstance>();
            meldInstance.SetMeld(meld);
            offset -= meldInstance.MeldWidth + MahjongConstants.Gap;
            meldInstances.Add(meldInstance);
        }

        public void AddToKong(Meld meld, Tile lastDraw)
        {
            Assert.AreEqual(meld.TileCount, 4, "A kong should have 4 tiles");
            var meldInstance = meldInstances.Find(instance =>
                instance.Meld.Type == meld.Type && instance.Meld.First.EqualsIgnoreColor(meld.First));
            Assert.IsNotNull(meldInstance, "meldInstance != null");
            // find the fourth tile
            meldInstance.AddToKong(lastDraw);
            meldInstance.Meld = meld;
        }
    }
}