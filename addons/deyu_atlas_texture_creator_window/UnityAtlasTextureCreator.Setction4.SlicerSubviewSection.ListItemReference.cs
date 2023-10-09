#if TOOLS


using System.Collections.Generic;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly struct ListItemReference<T>
    {
        public T Value { get => m_BackingList[m_Index]; set => m_BackingList[m_Index] = value; }

        private readonly IList<T> m_BackingList;
        private readonly int m_Index;

        public bool IsValid => m_Index >= 0 && m_Index < m_BackingList.Count;

        public ListItemReference<T> GetNext()
        {
            var nextIndex = m_Index + 1;
            if (nextIndex < m_BackingList.Count) return new(m_BackingList, nextIndex);

            return new(m_BackingList, -1);
        }

        public ListItemReference<T> GetPrev()
        {
            var previousIndex = m_Index - 1;
            if (previousIndex > 0) return new(m_BackingList, previousIndex);

            return new(m_BackingList, -1);
        }

        private ListItemReference(IList<T> backingList, int index)
        {
            m_BackingList = backingList;
            m_Index = index;
        }

        public static IEnumerable<ListItemReference<T>> CreateForEach(IList<T> list)
        {
            for (var index = 0; index < list.Count; index++)
            {
                yield return new(list, index);
            }
        }

        public static ListItemReference<T> CreateFor(IList<T> list) => new(list, 0);
    }
}
#endif
