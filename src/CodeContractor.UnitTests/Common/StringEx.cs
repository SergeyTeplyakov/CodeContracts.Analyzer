using System.Collections.Generic;

namespace CodeContractor.UnitTests.Common
{
    internal static class StringEx
    {
        public static IEnumerable<int> GetIndicesOfAndRemove(this string source, string content)
        {
            int currentIndex = 0;
            while (true)
            {
                var index = source.IndexOf(content, currentIndex);
                if (index == -1)
                {
                    yield break;
                }

                yield return index;
                source = source.Remove(index, content.Length);
                currentIndex = index + 1;
            }
        }
    }
}