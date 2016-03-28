using System;
using System.IO;

namespace KursyWalut.Serializer
{
    public class DateTimeSerializer : ISerializer<DateTime>
    {
        public void Serialize(DateTime obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj.ToBinary());
        }

        public DateTime Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var binaryDateTime = reader.ReadInt64();
            return DateTime.FromBinary(binaryDateTime);
        }
    }
}