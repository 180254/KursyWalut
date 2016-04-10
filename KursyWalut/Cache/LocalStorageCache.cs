using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using KursyWalut.Serializer;
using WinRTXamlToolkit.IO.Extensions;

namespace KursyWalut.Cache
{
    public class LocalStorageCache : ICache
    {
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
        private readonly SerializersStore _serializers;

        /// Warning: register serializers for all types, which will be cached!
        public LocalStorageCache(SerializersStore serializers)
        {
            _serializers = serializers;
        }

        public async Task<T> Get<T>(string key)
        {
            var file = await _localFolder.TryGetItemAsync(key) as IStorageFile;
            if (file == null) return default(T);

            Debug.WriteLine("Cache-{0}-{1}-{2}", nameof(Get), key, (await file.GetSizeAsync()).GetSizeString());

            try
            {
                using (var fileReadStream = await file.OpenStreamForReadAsync())
                {
                    return Deserialize<T>(fileReadStream);
                }
            }
            catch (InvalidCastException)
            {
                return default(T);
            }
        }

        public async Task Store<T>(string key, T value)
        {
            var file = await _localFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

            using (var fileWriteStream = await file.OpenStreamForWriteAsync())
            {
                Serialize(value, fileWriteStream);
            }

            Debug.WriteLine("Cache-{0}-{1}-{2}", nameof(Store), key, (await file.GetSizeAsync()).GetSizeString());
        }

        public Task<bool> Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task Clear()
        {
            throw new NotImplementedException();
        }


        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        private void Serialize<T>(T value, Stream stream)
        {
            _serializers.GetSerializer<T>().Serialize(value, stream);
        }

        /// <exception cref="T:System.InvalidCastException">Stream content != properly serialized T.</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        private T Deserialize<T>(Stream stream)
        {
            try
            {
                var deserialized = _serializers.GetSerializer<T>().Deserialize(stream);
                EnsureWholeStreamRead(stream); // possible InvalidCastException

                return deserialized;
            }
            catch (Exception ex) when (ex is EndOfStreamException)
            {
                throw new InvalidCastException("stream length exceeded", ex);
            }
        }

        /// <exception cref="T:System.InvalidCastException">Stream was not entire read.</exception>
        private static void EnsureWholeStreamRead(Stream stream)
        {
            // ReSharper disable once InvertIf
            if (stream.Position != stream.Length)
            {
                var exceptionMsg = string.Format(
                    "some data omitted: stream.position[{0}] != stream.length[{1}]",
                    stream.Position, stream.Length);

                throw new InvalidCastException(exceptionMsg);
            }
        }
    }
}