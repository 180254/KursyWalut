using Windows.Foundation.Collections;
using Cimbalino.Toolkit.Extensions;

namespace KursyWalut.Helper
{
    public class PropertySetHelper
    {
        private readonly IPropertySet _propertySet;

        public PropertySetHelper(IPropertySet propertySet)
        {
            _propertySet = propertySet;
        }

        public object this[string key]
        {
            get { return _propertySet[key]; }
            set { _propertySet[key] = value; }
        }

        public bool ContainsKey(string key)
        {
            return _propertySet.ContainsKey(key);
        }

        public T GetValue<T>(string key)
        {
            return (T) _propertySet.GetValue(key, null);
        }

        public T GetValue<T>(string key, T @default)
        {
            return (T) (_propertySet.GetValue(key, null) ?? @default);
        }
    }
}