using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;
using Microsoft.Win32;

namespace GTweak.Utilities.Helpers
{
    internal sealed class RegistryHelp : TakingOwnership
    {
        private static string GeneralRegistry(RegistryKey registrykey)
        {
            return registrykey.Name switch
            {
                "HKEY_LOCAL_MACHINE" => $@"MACHINE\",
                "HKEY_CLASSES_ROOT" => $@"CLASSES_ROOT\",
                "HKEY_CURRENT_USER" => $@"CURRENT_USER\",
                "HKEY_USERS" => $@"USERS\",
                _ => $@"CURRENT_CONFIG\",
            };
        }

        internal static void DeleteValue(RegistryKey registrykey, string subkey, string value, bool isTakingOwner = false)
        {
            Task.Run(delegate
            {
                if (registrykey.OpenSubKey(subkey) == null || registrykey.OpenSubKey(subkey)?.GetValue(value, null) == null)
                {
                    return;
                }

                try
                {
                    if (isTakingOwner)
                    {
                        GrantAdministratorsAccess($"{GeneralRegistry(registrykey)}{subkey}", SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                    }

                    registrykey.OpenSubKey(subkey, true)?.DeleteValue(value);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).GetAwaiter().GetResult();
        }

        internal static void Write<T>(RegistryKey registrykey, string subkey, string name, T data, RegistryValueKind kind, bool isTakingOwner = false)
        {
            Task.Run(delegate
            {
                try
                {
                    if (isTakingOwner)
                    {
                        GrantAdministratorsAccess($"{GeneralRegistry(registrykey)}{subkey}", SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                    }

                    registrykey.CreateSubKey(subkey, true)?.SetValue(name, data, kind);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).GetAwaiter().GetResult();
        }

        internal static void CreateFolder(RegistryKey registrykey, string subkey)
        {
            Task.Run(delegate
            {
                try { registrykey.CreateSubKey(subkey); }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).GetAwaiter().GetResult();
        }

        internal static void DeleteFolderTree(RegistryKey registrykey, string subkey, bool isTakingOwner = false)
        {
            Task.Run(delegate
            {
                try
                {
                    if (isTakingOwner)
                    {
                        GrantAdministratorsAccess($"{GeneralRegistry(registrykey)}{subkey}", SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                    }

                    using RegistryKey registryFolder = registrykey.OpenSubKey(subkey, true);

                    if (registryFolder != null)
                    {
                        foreach (string value in registryFolder.GetValueNames())
                        {
                            try { registryFolder.DeleteValue(value); }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                        }
                    }
                    registrykey.DeleteSubKeyTree(subkey, false);
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).GetAwaiter().GetResult();
        }

        internal static bool KeyExists(RegistryKey registryKey, string subKey, bool invert = false)
        {
            using RegistryKey opened = registryKey.OpenSubKey(subKey);
            bool result = opened != null;
            return invert ? !result : result;
        }

        internal static bool ValueExists(string subKey, string valueName, bool invert = false)
        {
            bool result = Registry.GetValue(subKey, valueName, null) != null;
            return invert ? !result : result;
        }

        internal static bool CheckValue(string subKey, string valueName, string expectedValue, bool invert = false)
        {
            string value = Registry.GetValue(subKey, valueName, null)?.ToString();
            bool result = !string.Equals(value, expectedValue, StringComparison.OrdinalIgnoreCase);
            return invert ? !result : result;
        }

        internal static bool CheckValueBytes(string subkey, string valueName, string expectedValue)
        {
            if (!(Registry.GetValue(subkey, valueName, null) is byte[]))
            {
                return true;
            }

            return string.Concat(Registry.GetValue(subkey, valueName, null) as byte[] ?? Array.Empty<byte>()) != expectedValue;
        }

        internal static T GetValue<T>(string subKey, string valueName, T defaultValue)
        {
            try { return (T)Convert.ChangeType(Registry.GetValue(subKey, valueName, defaultValue), typeof(T)); }
            catch { return defaultValue; }
        }

        internal static T GetSubKeyNames<T>(RegistryKey baseKey, string subKeyPath) where T : ICollection<string>, new()
        {
            try
            {
                using RegistryKey key = baseKey.OpenSubKey(subKeyPath);
                if (key == null)
                {
                    return new T();
                }

                T result = new T();
                foreach (var name in key.GetSubKeyNames())
                {
                    result.Add(name);
                }

                return result;
            }
            catch { return new T(); }
        }
    }
}
