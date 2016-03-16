using System;
using System.Collections.Generic;
using Windows.Storage;

namespace KursyWalut.Cache
{
    internal class LocalSettingsCache : ICache
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public void Set<TR>(string key, TR value)
        {
            _localSettings.Values[key] = value;
        }

        public TR Get<TR>(string key)
        {
            return (TR) _localSettings.Values[key];
        }

        public TR GetOrSet<TR>(string key, TR value)
        {
            return GetOrCompute(key, () => value);
        }

        public TR GetOrCompute<TR>(string key, Func<TR> valueFunc)
        {
            try
            {
                return Get<TR>(key);
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                Set(key, valueFunc.Invoke());
                return Get<TR>(key);
            }
        }
    }
}