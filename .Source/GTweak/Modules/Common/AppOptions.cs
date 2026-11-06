using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using GTweak.Modules.Helpers;
using Microsoft.Win32;

namespace GTweak.Modules.Common
{
    internal static class AppOptions
    {
        private static readonly ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();

        internal static T Get<T>(T defaultValue, [CallerMemberName] string key = default)
        {
            if (_cache.TryGetValue(key, out var val))
            {
                return (T)val;
            }

            if (!RegistryHelper.ValueExists(PathTargets.Registry.BaseKey, key))
            {
                Set(defaultValue, key);
                return defaultValue;
            }

            try
            {
                object registryValue = RegistryHelper.GetValue(PathTargets.Registry.BaseKey, key, defaultValue);

                T result = defaultValue switch
                {
                    bool _ => (T)(object)(Convert.ToInt32(registryValue) != 0),
                    int _ => (T)(object)Convert.ToInt32(registryValue),
                    _ => (T)(object)(registryValue?.ToString() ?? string.Empty)
                };

                _cache[key] = result;
                return result;
            }
            catch
            {
                _cache[key] = defaultValue;
                return defaultValue;
            }
        }

        internal static void Set<T>(T value, [CallerMemberName] string key = default)
        {
            _cache[key] = value;

            var (regValue, kind) = value switch
            {
                bool valueBool => ((object)(valueBool ? 1 : 0), RegistryValueKind.DWord),
                int valueInt => (valueInt, RegistryValueKind.DWord),
                _ => (value?.ToString() ?? string.Empty, RegistryValueKind.String)
            };

            RegistryHelper.Write(Registry.CurrentUser, PathTargets.Registry.SubKey, key, regValue, kind);
        }
    }
}
