using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using KursyWalut.Model;
using KursyWalut.Serializers;

namespace KursyWalut.Cache
{
    /// Impl-dependent!
    /// All used serializers must be defined and registered.
    /// GetStandard() returns LocalStorageCache with serializers used in project.
    public class LocalStorageCache : ICache
    {
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
        private readonly IDictionary<Type, object> _serializers = new ConcurrentDictionary<Type, object>();


        public T Get<T>(string key, Func<T> default_)
        {
            var file1 = _localFolder.TryGetItemAsync(key);
            file1.AsTask().Wait();

            var file = file1.GetResults() as IStorageFile;
            if (file == null) return default_.Invoke();

            var fileStream1 = file.OpenStreamForReadAsync();
            fileStream1.Wait();

            using (var fileStream = fileStream1.Result)
            {
                return Deserialize<T>(fileStream);
            }
        }

        public void Store<T>(string key, T value)
        {
            var file1 = _localFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
            file1.AsTask().Wait();

            var file = file1.GetResults();

            var fileStream1 = file.OpenStreamForWriteAsync();
            fileStream1.Wait();

            using (var fileStream = fileStream1.Result)
            {
                GetSerializer<T>().Serialize(value, fileStream);
            }
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }


        public static LocalStorageCache GetStandard()
        {
            var lsc = new LocalStorageCache();

            lsc.RegisterSerializer(new CurrencySerializer());
            lsc.RegisterSerializer(new DateTimeSerializer());
            lsc.RegisterSerializer(new StringSerializer());
            lsc.RegisterSerializer(new IntSerializer());
            lsc.RegisterSerializer(new ExchangeRateSerializer(
                lsc.GetSerializer<DateTime>(),
                lsc.GetSerializer<Currency>()));

            // IList<int>
            lsc.RegisterSerializer(new ListSerializer<int>(
                lsc.GetSerializer<int>()));

            // IDictionary<int, IList<DateTime>> 
            lsc.RegisterSerializer(
                new ListSerializer<DateTime>(
                    lsc.GetSerializer<DateTime>()));
            lsc.RegisterSerializer(
                new DictionarySerializer<int, IList<DateTime>>(
                    lsc.GetSerializer<int>(),
                    lsc.GetSerializer<IList<DateTime>>()));

            // IDictionary<DateTime, IList<ExchangeRate>>
            lsc.RegisterSerializer(
                new ListSerializer<ExchangeRate>(
                    lsc.GetSerializer<ExchangeRate>()));
            lsc.RegisterSerializer(
                new DictionarySerializer<DateTime, IList<ExchangeRate>>(
                    lsc.GetSerializer<DateTime>(),
                    lsc.GetSerializer<IList<ExchangeRate>>()));

            // IDictionary<DateTime, string>
            lsc.RegisterSerializer<IDictionary<DateTime, string>> (
                new DictionarySerializer<DateTime, string>(
                    lsc.GetSerializer<DateTime>(),
                    lsc.GetSerializer<string>()));

            return lsc;
        }


        public void RegisterSerializer<T>(ISerializer<T> serializer)
        {
            _serializers.Add(typeof (T), serializer);
        }

        private ISerializer<T> GetSerializer<T>()
        {
            return (ISerializer<T>) _serializers[typeof (T)];
        }

        private T Deserialize<T>(Stream fileStream)
        {
            try
            {
                var deserialized = GetSerializer<T>().Deserialize(fileStream);

                if (fileStream.Position != fileStream.Length)
                    throw new InvalidCastException("some data omitted");

                return deserialized;
            }
            catch (Exception ex) when (ex is EndOfStreamException)
            {
                throw new InvalidCastException("end of stream reached", ex);
            }
        }
    }
}