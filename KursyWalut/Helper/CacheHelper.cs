using System;
using System.Collections.Generic;
using KursyWalut.Cache;
using KursyWalut.Model;
using KursyWalut.Serializer;

namespace KursyWalut.Helper
{
    public class CacheHelper
    {
        public static ICache GetStandardLsc()
        {
            var ses = new SerializersStore();
            var lsc = new LocalStorageCache(ses);

            // primitives
            ses.RegisterSerializer(new IntSerializer());
            ses.RegisterSerializer(new StringSerializer());

            // non-collection types
            ses.RegisterSerializer(new CurrencySerializer());
            ses.RegisterSerializer(new DateTimeSerializer());
            ses.RegisterSerializer(new ExchangeRateSerializer(ses));

            // IList<int>
            ses.RegisterSerializer(new ListSerializer<int>(ses));

            // IDictionary<int, IList<DateTime>> 
            ses.RegisterSerializer(new ListSerializer<DateTime>(ses));
            ses.RegisterSerializer(new DictionarySerializer<int, IList<DateTime>>(ses));

            // IDictionary<DateTime, IList<ExchangeRate>>
            ses.RegisterSerializer(new ListSerializer<ExchangeRate>(ses));
            ses.RegisterSerializer(new DictionarySerializer<DateTime, IList<ExchangeRate>>(ses));

            // IDictionary<DateTime, string>
            ses.RegisterSerializer(new DictionarySerializer<DateTime, string>(ses));

            return lsc;
        }
    }
}