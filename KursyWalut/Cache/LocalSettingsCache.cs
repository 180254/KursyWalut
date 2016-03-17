using System;
using System.Collections.Generic;
using Windows.Storage;

namespace KursyWalut.Cache
{
    // TODO: doesn't work ;] doesn't actually know how to cache
    internal class LocalSettingsCache : ICache
    {
        private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;

        public void Set<TR>(string key, TR value)
        {
            _localSettings.Values[key] = value;
        }

        public TR SetIfAbsent<TR>(string key, TR value)
        {
            return ComputeIfAbsent(key, () => value);
        }

        public TR Compute<TR>(string key, Func<TR> valueFunc)
        {
            var value = valueFunc.Invoke();
            Set(key, value);
            return value;
        }

        public TR ComputeIfAbsent<TR>(string key, Func<TR> valueFunc)
        {
            try
            {
                return Get<TR>(key);
            }
            catch (Exception ex) when (ex is KeyNotFoundException)
            {
                return Compute(key, valueFunc);
            }
        }

        public TR Get<TR>(string key)
        {
            var ret = (TR) _localSettings.Values[key];
            if (ret == null) throw new KeyNotFoundException("key: " + key);
            return ret;
        }

        public bool Remove(string key)
        {
            return _localSettings.Values.Remove(key);
        }
    }
}