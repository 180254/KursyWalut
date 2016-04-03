using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using KursyWalut.Serializer;
using WinRTXamlToolkit.IO.Extensions;

namespace KursyWalut.Cache
{
    public class LocalStorageCache : ICache
    {
        private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly SerializersStore _serializers;

        /// Warning: register serializer for all types, which will be cached!
        public LocalStorageCache(SerializersStore serializers)
        {
            _serializers = serializers;
        }

        public async Task<T> Get<T>(string key, Func<T> default_)
        {
            _lock.EnterReadLock();

            try
            {
                var file = await _localFolder.TryGetItemAsync(key) as IStorageFile;
                if (file == null) return default_.Invoke();

#if DEBUG
                Debug.WriteLine("Cache-{0}-{1}-{2}", nameof(Get), key, await file.GetSizeAsync());
#endif

                using (var fileStream = await file.OpenStreamForReadAsync())
                {
                    return Deserialize<T>(fileStream);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task Store<T>(string key, T value)
        {
            _lock.EnterWriteLock();

            try
            {
                var file = await _localFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                using (var fileStream = await file.OpenStreamForWriteAsync())
                {
                    Serialize(value, fileStream);
                }

#if DEBUG
                Debug.WriteLine("Cache-{0}-{1}-{2}", nameof(Store), key, await file.GetSizeAsync());
#endif
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public Task<bool> Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task Clear()
        {
            throw new NotImplementedException();
        }


        private void Serialize<T>(T value, Stream stream)
        {
            _serializers.GetSerializer<T>().Serialize(value, stream);
        }

        private T Deserialize<T>(Stream stream)
        {
            try
            {
                var deserialized = _serializers.GetSerializer<T>().Deserialize(stream);

                // ReSharper disable once InvertIf
                if (stream.Position != stream.Length)
                {
                    var exceptionMsg = string.Format(
                        "some data omitted: stream.position[{0}] != stream.length[{1}]",
                        stream.Position, stream.Length);
                    throw new InvalidCastException(exceptionMsg);
                }

                return deserialized;
            }
            catch (Exception ex) when (ex is EndOfStreamException)
            {
                throw new InvalidCastException("end of stream reached", ex);
            }
        }
    }
}