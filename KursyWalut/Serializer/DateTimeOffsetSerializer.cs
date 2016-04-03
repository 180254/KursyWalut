using System;
using System.IO;

namespace KursyWalut.Serializer
{
    public class DateTimeOffsetSerializer : ISerializer<DateTimeOffset>
    {
        public void Serialize(DateTimeOffset obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj.ToUnixTimeSeconds());
        }

        public DateTimeOffset Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var unixTimeSeconds = reader.ReadInt64();
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds);
        }
    }
}