using System.Collections.Generic;
using System.IO;

namespace KursyWalut.Serializers
{
    public class DictionarySerializer<TK, TV> : ISerializer<IDictionary<TK, TV>>
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

        public DictionarySerializer(SerializersStore store) : this(
            store.GetSerializer<TK>(),
            store.GetSerializer<TV>())
        {
        }

        public void Serialize(IDictionary<TK, TV> obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);

            var enumerator = obj.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var kvp = enumerator.Current;
                writer.Write(true);
                _tkSerializer.Serialize(kvp.Key, stream);
                _tvSerializer.Serialize(kvp.Value, stream);
            }

            writer.Write(false);
        }

        public IDictionary<TK, TV> Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);

            var dictionary = new Dictionary<TK, TV>();
            while (reader.ReadBoolean())
            {
                var key = _tkSerializer.Deserialize(stream);
                var value = _tvSerializer.Deserialize(stream);
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}