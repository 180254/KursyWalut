using System.Collections.Generic;
using System.Linq;
using Cimbalino.Toolkit.Extensions;

namespace KursyWalut.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> Averaged<T>(this IEnumerable<T> data, int expectedSize)
        {
            var dataArray = data as T[] ?? data.ToArray();
            if (dataArray.Length <= expectedSize) return dataArray;

            var incr = (int) (dataArray.Length/(double) expectedSize);

            var indexes = Enumerable.Range(0, expectedSize)
                .Select(i => i*incr)
                .Concat(dataArray.Length - 1);

            return Slice(dataArray, indexes);
        }

        public static IEnumerable<T> Slice<T>(this T[] data, IEnumerable<int> indexes)
        {
            return indexes.Select(i => data[i]);
        }
    }
}