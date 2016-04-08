using Windows.Foundation.Collections;
using Cimbalino.Toolkit.Extensions;

namespace KursyWalut.Helper
{
    public class PropertySetHelper
    {
        public PropertySetHelper(IPropertySet propertySet)
        {
            BaseSet = propertySet;
        }

        public IPropertySet BaseSet { get; }

        public object this[string key]
        {
            get { return BaseSet[key]; }
            set { BaseSet[key] = value; }
        }

        public T GetValue<T>(string key)
        {
            return (T) BaseSet.GetValue(key, null);
        }

        public T GetValue<T>(string key, T @default)
        {
            return (T) (BaseSet.GetValue(key, null) ?? @default);
        }
    }
}