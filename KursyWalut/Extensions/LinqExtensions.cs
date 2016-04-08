using System;
using System.Collections.Generic;
using System.Linq;
using Cimbalino.Toolkit.Extensions;

namespace KursyWalut.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        ///     This functions is to ensure that data to has length approximately "expected length".<br />
        ///     It is usefull if data has far too many elements to be processed.<br />
        ///     This simply implementation just select some indexes omitting others.<br />
        ///     Always first and last element is included in result.<br />
        ///     <br />
        ///     Example:<br />
        ///     data = { 1, 2, 2, 3, 1, 6, 2, 1, 1 },<br />
        ///     expectedLength = 3,<br />
        ///     return = { 1, 3, 2, 1 }<br />
        ///     <br />
        ///     Note: returns just given data if data.Length &lt;= expectedSize.<br />
        ///     I other cases returned enumarable has <b>at laest</b> expectedLength elements (probably more).
        /// </summary>
        /// <typeparam name="T">data' type</typeparam>
        /// <param name="data">data</param>
        /// <param name="expectedLength">new expected size</param>
        /// <returns></returns>
        public static IEnumerable<T> Averaged<T>(this IEnumerable<T> data, int expectedLength)
        {
            var dataArray = data as T[] ?? data.ToArray();
            if (dataArray.Length <= expectedLength) return dataArray;

            var incr = (int) Math.Round(dataArray.Length/(double) expectedLength);
            var nextSize = (int) Math.Floor(dataArray.Length/(double) incr);

            var indexes = Enumerable.Range(0, nextSize)
                .Select(i => i*incr);

            if (nextSize*incr != dataArray.Length - 1)
                indexes = indexes.Concat(dataArray.Length - 1);

            return Slice(dataArray, indexes);
        }

        public static IEnumerable<T> Slice<T>(this T[] data, IEnumerable<int> indexes)
        {
            return indexes.Select(i => data[i]);
        }
    }
}