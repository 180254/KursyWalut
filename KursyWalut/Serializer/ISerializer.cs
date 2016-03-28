using System.IO;

namespace KursyWalut.Serializer
{
    public interface ISerializer<T>
    {
        void Serialize(T obj, Stream stream);
        T Deserialize(Stream stream);
    }
}