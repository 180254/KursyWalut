using System;
using System.IO;
using System.Threading;
using Windows.Storage;
using KursyWalut.Serializer;

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

        public T Get<T>(string key, Func<T> default_)
        {
            _lock.EnterReadLock();

            try
            {
                var file1 = _localFolder.TryGetItemAsync(key);
                file1.AsTask().Wait(_waitTime);

                var file = file1.GetResults() as IStorageFile;
                if (file == null) return default_.Invoke();

                var fileStream1 = file.OpenStreamForReadAsync();
                fileStream1.Wait(_waitTime);

                using (var fileStream = fileStream1.Result)
                {
                    return Deserialize<T>(fileStream);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Store<T>(string key, T value)
        {
            _lock.EnterWriteLock();

            try
            {
                var file1 = _localFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
                file1.AsTask().Wait(_waitTime);

                var file = file1.GetResults();

                var fileStream1 = file.OpenStreamForWriteAsync();
                fileStream1.Wait(_waitTime);

                using (var fileStream = fileStream1.Result)
                {
                    Serialize(value, fileStream);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
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