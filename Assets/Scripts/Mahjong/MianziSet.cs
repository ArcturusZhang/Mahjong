using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mahjong
{
    [Serializable]
    public class MianziSet : IEnumerable<Mianzi>
    {
        private const int tileKinds = 34;
        private readonly List<Mianzi> list;

        public MianziSet()
        {
            list = new List<Mianzi>();
        }

        public MianziSet(MianziSet copy) : this()
        {
            list.AddRange(copy);
        }

        public void Add(Mianzi item)
        {
            list.Add(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public void Sort()
        {
            list.Sort();
        }

        public Mianzi this[int index] => list[index];

        public int MianziCount => list.Count;

        public int[] TileDistribution
        {
            get
            {
                var distribution = new int[tileKinds];
                foreach (var mianzi in list)
                {
                    int index = MahjongHand.GetIndex(mianzi.First);
                    switch (mianzi.Type)
                    {
                        case MianziType.Single:
                            distribution[index]++;
                            break;
                        case MianziType.Jiang:
                            distribution[index] += 2;
                            break;
                        case MianziType.Kezi:
                            distribution[index] += 3;
                            if (mianzi.IsGangzi) distribution[index]++;
                            break;
                        case MianziType.Shunzi:
                            distribution[index]++;
                            distribution[index + 1]++;
                            distribution[index + 2]++;
                            break;
                        default: throw new ArgumentException("Invalid MianziType");
                    }
                }

                return distribution;
            }
        }

        public int TileCount
        {
            get
            {
                int count = 0;
                foreach (var mianzi in list)
                {
                    switch (mianzi.Type)
                    {
                        case MianziType.Single:
                            count++;
                            break;
                        case MianziType.Jiang:
                            count += 2;
                            break;
                        case MianziType.Shunzi:
                            count += 3;
                            break;
                        case MianziType.Kezi:
                            count += 3;
                            if (mianzi.IsGangzi) count++;
                            break;
                        default: throw new ArgumentException("Invalid MianziType");
                    }
                }

                return count;
            }
        }

        public Mianzi Jiang
        {
            get
            {
                foreach (var mianzi in list)
                {
                    if (mianzi.Type == MianziType.Jiang) return mianzi;
                }
                throw new ArgumentException("No jiang!");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var mianzi in list)
            {
                builder.Append(mianzi).Append(" ");
            }

            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            var mianziSet = obj as MianziSet;
            if (mianziSet == null) return false;
            if (MianziCount != mianziSet.MianziCount) return false;
            for (int i = 0; i < MianziCount; i++)
            {
                if (!this[i].Equals(mianziSet[i])) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public IEnumerator<Mianzi> GetEnumerator()
        {
            return new MianziEnumerator(list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private struct MianziEnumerator : IEnumerator<Mianzi>
        {
            private readonly List<Mianzi> list;
            private int currentIndex;

            internal MianziEnumerator(List<Mianzi> l)
            {
                list = l;
                currentIndex = -1;
            }

            public bool MoveNext()
            {
                return ++currentIndex < list.Count;
            }

            public void Reset()
            {
                currentIndex = -1;
            }

            public Mianzi Current => list[currentIndex];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}