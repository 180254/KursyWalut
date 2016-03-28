using System.IO;

namespace KursyWalut.Serializer
{
    public class IntSerializer : ISerializer<int>
    {
        public void Serialize(int obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj);
        }

        public int Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            return reader.ReadInt32();
        }
    }
}