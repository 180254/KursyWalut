using System.IO;

namespace KursyWalut.Serializers
{
    internal class StringSerializer : ISerializer<string>
    {
        public void Serialize(string obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj);
            writer.Flush();
        }

        public string Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            return reader.ReadString();
        }
    }
}