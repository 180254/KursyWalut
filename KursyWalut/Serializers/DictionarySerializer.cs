using System.Collections.Generic;
using System.IO;

namespace KursyWalut.Serializers
{
    internal class DictionarySerializer<TK, TV> : ISerializer<Dictionary<TK, TV>>
    {
        private readonly ISerializer<TK> _tkSerializer;
        private readonly ISerializer<TV> _tvSerializer;

        public DictionarySerializer(
            ISerializer<TK> tkSerializer,
            ISerializer<TV> tvSerializer)
        {
            _tkSerializer = tkSerializer;
            _tvSerializer = tvSerializer;
        }

        public void Serialize(Dictionary<TK, TV> obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(obj.Count);
            foreach (var kvp in obj)
            {
                _tkSerializer.Serialize(kvp.Key, stream);
                _tvSerializer.Serialize(kvp.Value, stream);
            }

            writer.Flush();
        }

        public Dictionary<TK, TV> Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);

            var count = reader.ReadInt32();
            var dictionary = new Dictionary<TK, TV>(count);
            for (var n = 0; n < count; n++)
            {
                var key = _tkSerializer.Deserialize(stream);
                var value = _tvSerializer.Deserialize(stream);
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}