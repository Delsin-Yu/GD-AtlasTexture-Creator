#if TOOLS


using System.Collections.Generic;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly struct ListItemReference<T>
    {
        public T Value { get => _backingList[_index]; set => _backingList[_index] = value; }

        private readonly IList<T> _backingList;
        private readonly int _index;

        public bool IsValid => _index >= 0 && _index < _backingList.Count;

        public ListItemReference<T> GetNext()
        {
            var nextIndex = _index + 1;
            if (nextIndex < _backingList.Count) return new(_backingList, nextIndex);

            return new(_backingList, -1);
        }

        public ListItemReference<T> GetPrev()
        {
            var previousIndex = _index - 1;
            if (previousIndex > 0) return new(_backingList, previousIndex);

            return new(_backingList, -1);
        }

        private ListItemReference(IList<T> backingList, int index)
        {
            _backingList = backingList;
            _index = index;
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
