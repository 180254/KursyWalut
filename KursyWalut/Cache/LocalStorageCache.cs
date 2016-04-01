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
        private readonly TimeSpan _waitTime = TimeSpan.FromSeconds(1);
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
                Debug.WriteLine("Cache-{0}-{1}", key, await file.GetSizeAsync());
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
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task<bool> Remove(string key)
        {
            throw new NotImplementedException();
        }

        public async Task Clear()
        {
            throw new NotImplementedException();
        }


        private void Serialize<T>(T value, Stream fileStream)
        {
            _serializers.GetSerializer<T>().Serialize(value, fileStream);
        }

        private T Deserialize<T>(Stream fileStream)
        {
            try
            {
                var deserialized = _serializers.GetSerializer<T>().Deserialize(fileStream);

                // ReSharper disable once InvertIf
                if (fileStream.Position != fileStream.Length)
                {
                    var exceptionMsg = string.Format(
                        "some data omitted: stream.position[{0}] != stream.length[{1}]",
                        fileStream.Position, fileStream.Length);
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