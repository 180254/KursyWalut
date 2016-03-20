using System.IO;
using KursyWalut.Model;

namespace KursyWalut.Serializers
{
    internal class CurrencySerializer : ISerializer<Currency>
    {
        public void Serialize(Currency obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(obj.Code);
            writer.Write(obj.Name);
            writer.Write(obj.Multiplier);
        }

        public Currency Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var code = reader.ReadString();
            var name = reader.ReadString();
            var multiplier = reader.ReadInt32();
            return new Currency(code, name, multiplier);
        }
    }
}