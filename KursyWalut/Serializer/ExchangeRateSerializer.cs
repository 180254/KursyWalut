using System;
using System.IO;
using KursyWalut.Model;

namespace KursyWalut.Serializer
{
    public class ExchangeRateSerializer : ISerializer<ExchangeRate>
    {
        private readonly ISerializer<DateTimeOffset> _dateTimeOffsetSerializer;
        private readonly ISerializer<Currency> _currencySerializer;

        public ExchangeRateSerializer(
            ISerializer<DateTimeOffset> dateTimeOffsetSerializer,
            ISerializer<Currency> currencySerializer)
        {
            _dateTimeOffsetSerializer = dateTimeOffsetSerializer;
            _currencySerializer = currencySerializer;
        }

        public ExchangeRateSerializer(SerializersStore store) : this(
            store.GetSerializer<DateTimeOffset>(),
            store.GetSerializer<Currency>())
        {
        }

        public void Serialize(ExchangeRate obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            _dateTimeOffsetSerializer.Serialize(obj.Day, stream);
            _currencySerializer.Serialize(obj.Currency, stream);
            writer.Write(obj.AverageRate);
        }

        public ExchangeRate Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var day = _dateTimeOffsetSerializer.Deserialize(stream);
            var currency = _currencySerializer.Deserialize(stream);
            var avarageRate = reader.ReadDouble();
            return new ExchangeRate(day, currency, avarageRate);
        }
    }
}