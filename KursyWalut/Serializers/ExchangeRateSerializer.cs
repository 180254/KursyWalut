﻿using System;
using System.IO;
using KursyWalut.Model;

namespace KursyWalut.Serializers
{
    public class ExchangeRateSerializer : ISerializer<ExchangeRate>
    {
        private readonly ISerializer<DateTime> _dateTimeSerializer;
        private readonly ISerializer<Currency> _currencySerializer;

        public ExchangeRateSerializer(
            ISerializer<DateTime> dateTimeSerializer,
            ISerializer<Currency> currencySerializer)
        {
            _dateTimeSerializer = dateTimeSerializer;
            _currencySerializer = currencySerializer;
        }

        public ExchangeRateSerializer(SerializersStore store) : this(
            store.GetSerializer<DateTime>(),
            store.GetSerializer<Currency>())
        {
        }

        public void Serialize(ExchangeRate obj, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            _dateTimeSerializer.Serialize(obj.Day, stream);
            _currencySerializer.Serialize(obj.Currency, stream);
            writer.Write(obj.AverageRate);
        }

        public ExchangeRate Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var day = _dateTimeSerializer.Deserialize(stream);
            var currency = _currencySerializer.Deserialize(stream);
            var avarageRate = reader.ReadDouble();
            return new ExchangeRate(day, currency, avarageRate);
        }
    }
}