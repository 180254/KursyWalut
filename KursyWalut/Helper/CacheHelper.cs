using System;
using System.Collections.Generic;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Serializer;

namespace KursyWalut.Helper
{
    public class CacheHelper
    {
        /// <summary>
        ///     ICache implementation - LocalStorageCache with registered all serializers used in this project.
        /// </summary>
        /// <returns>ICache</returns>
        public static ICache GetStandardLsc()
        {
            var ses = new SerializersStore();
            var lsc = new LocalStorageCache(ses);

            // primitives
            ses.RegisterSerializer(new IntSerializer());
            ses.RegisterSerializer(new StringSerializer());

            // non-collection types
            ses.RegisterSerializer(new CurrencySerializer());
            ses.RegisterSerializer(new DateTimeOffsetSerializer());
            ses.RegisterSerializer(new ExchangeRateSerializer(ses));

            // IList<int>
            ses.RegisterSerializer(new ListSerializer<int>(ses));

            // IDictionary<int, IList<DateTimeOffset>> 
            ses.RegisterSerializer(new ListSerializer<DateTimeOffset>(ses));
            ses.RegisterSerializer(new DictionarySerializer<int, IList<DateTimeOffset>>(ses));

            // IDictionary<DateTimeOffset, IList<ExchangeRate>>
            ses.RegisterSerializer(new ListSerializer<ExchangeRate>(ses));
            ses.RegisterSerializer(new DictionarySerializer<DateTimeOffset, IList<ExchangeRate>>(ses));

            // IDictionary<DateTimeOffset, string>
            ses.RegisterSerializer(new DictionarySerializer<DateTimeOffset, string>(ses));

            return lsc;
        }
    }
}