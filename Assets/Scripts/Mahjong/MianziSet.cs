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
        private readonly int[] counts;

        public MianziSet()
        {
            list = new List<Mianzi>();
            counts = new int[tileKinds];
        }

        public MianziSet(MianziSet copy) : this()
        {
            list.AddRange(copy);
            Array.Copy(copy.counts, counts, counts.Length);
        }

        public void Add(Mianzi item)
        {
            list.Add(item);
            int index = MahjongHand.GetIndex(item.First);
            switch (item.Type)
            {
                case MianziType.Single:
                    counts[index]++;
                    break;
                case MianziType.Jiang:
                    counts[index] += 2;
                    break;
                case MianziType.Kezi:
                    counts[index] += 3;
                    if (item.IsGangzi) counts[index]++;
                    break;
                case MianziType.Shunzi:
                    counts[index]++;
                    counts[index + 1]++;
                    counts[index + 2]++;
                    break;
                default: throw new ArgumentException("Invalid MianziType");
            }
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

        public int Count => list.Count;

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
            if (obj == null) return false;
            if (obj is MianziSet)
            {
                var set = (MianziSet) obj;
                if (Count != set.Count) return false;
                for (int i = 0; i < Count; i++)
                {
                    if (!this[i].Equals(set[i])) return false;
                }

                return true;
            }

            return false;
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
                if (++currentIndex >= list.Count) return false;
                return true;
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