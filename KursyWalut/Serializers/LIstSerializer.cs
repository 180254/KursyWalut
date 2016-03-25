using System.Collections.Generic;
using System.IO;

namespace KursyWalut.Serializers
{
    public class ListSerializer<T> : ISerializer<IList<T>>
    {
        private readonly ISerializer<T> _tSerializer;

        public ListSerializer(ISerializer<T> tSerializer)
        {
            _tSerializer = tSerializer;
        }

        public void Serialize(IList<T> obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj.Count);

            foreach (var o in obj)
                _tSerializer.Serialize(o, stream);
        }

        public IList<T> Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var count = reader.ReadInt32();

            var ret = new List<T>(count);
            for (var i = 0; i < count; i++)
                ret.Add(_tSerializer.Deserialize(stream));

            return ret;
        }
    }
}