using System;
using System.Collections.Generic;

namespace KursyWalut.Serializers
{
    public class SerializersStore
    {
        private readonly IDictionary<Type, object> _serializers = new Dictionary<Type, object>();

        public void RegisterSerializer<T>(ISerializer<T> serializer)
        {
            _serializers.Add(typeof (T), serializer);
        }

        public ISerializer<T> GetSerializer<T>()
        {
            return (ISerializer<T>) _serializers[typeof (T)];
        }
    }
}