using System.IO;

namespace KursyWalut.Serializers
{
    internal interface ISerializer<T>
    {
        void Serialize(T obj, Stream stream);
        T Deserialize(Stream stream);
    }
}