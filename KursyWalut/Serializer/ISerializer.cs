using System.IO;

namespace KursyWalut.Serializer
{
    public interface ISerializer<T>
    {
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        void Serialize(T obj, Stream stream);

        /// <exception cref="T:System.IO.EndOfStreamException">Stream content != properly serialized T as end of stream reached.</exception>
        /// <exception cref="T:System.IO.IOException">Something go wrong with I/O.</exception>
        T Deserialize(Stream stream);
    }
}