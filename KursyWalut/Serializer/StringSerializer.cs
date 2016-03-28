using System.IO;

namespace KursyWalut.Serializer
{
    public class StringSerializer : ISerializer<string>
    {
        public void Serialize(string obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj);
        }

        public string Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            return reader.ReadString();
        }
    }
}