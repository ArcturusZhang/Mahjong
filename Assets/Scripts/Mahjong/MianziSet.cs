using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mahjong
{
    [Serializable]
    public class MianziSet : IEnumerable<Mianzi>
    {
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

        public Mianzi this[int index]
        {
            get { return list[index]; }
        }

        public int Count
        {
            get { return list.Count; }
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

            public Mianzi Current
            {
                get { return list[currentIndex]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
            }
        }
    }
}