using System;
using System.Collections.Generic;
using System.Linq;
using Cimbalino.Toolkit.Extensions;

namespace KursyWalut.Helper
{
    public static class LinqHelper
    {
        public static IEnumerable<T> Averaged<T>(this IEnumerable<T> data, int expectedSize)
        {
            var data2 = data as T[] ?? data.ToArray();
            if (data2.Length <= expectedSize) return data2;

            var incr = (int) Math.Floor(data2.Length/(double) expectedSize);
            var indexes = Enumerable.Range(0, expectedSize)
                .Select(i => i*incr)
                .Concat(data2.Length - 1);

            return Slice(data2, indexes);
        }

        public static IEnumerable<T> Slice<T>(this IList<T> data, IEnumerable<int> indexes)
        {
            return indexes.Select(i => data[i]);
        }
    }
}