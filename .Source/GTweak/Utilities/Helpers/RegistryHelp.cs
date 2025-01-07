using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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
            Task.Run(async delegate
            {
                if (registrykey.OpenSubKey(subkey) == null || registrykey.OpenSubKey(subkey)?.GetValue(value, null) == null) return;
                try
                {
                    if (isTakingOwner)
                    {
                        GrantAdministratorsAccess($"{GeneralRegistry(registrykey)}{subkey}", SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                        await Task.Delay(200);
                    }

                    registrykey.OpenSubKey(subkey, true)?.DeleteValue(value);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
        }

        internal static void Write<T>(RegistryKey registrykey, string subkey, string name, T data, RegistryValueKind kind, bool isTakingOwner = false)
        {
            Task.Run(async delegate
            {
                try
                {
                    if (isTakingOwner)
                    {
                        GrantAdministratorsAccess($"{GeneralRegistry(registrykey)}{subkey}", SE_OBJECT_TYPE.SE_REGISTRY_KEY);
                        await Task.Delay(200);
                    }

                    registrykey.CreateSubKey(subkey, true)?.SetValue(name, data, kind);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
        }

        internal static void CreateFolder(RegistryKey registrykey, string subkey)
        {
            Task.Run(() =>
            {
                try
                {
                    registrykey.CreateSubKey(subkey);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
        }

        internal static void DeleteFolderTree(RegistryKey registrykey, string subkey)
        {
            Task.Run(() =>
            {
                try
                {
                    RegistryKey registryFolder = registrykey.OpenSubKey(subkey, true);

                    if (registryFolder != null)
                    {
                        foreach (string value in registryFolder.GetValueNames())
                        {
                            try { registryFolder.DeleteValue(value); }
                            catch (Exception ex) { Debug.WriteLine(ex.Message); }
                        }
                    }
                    registrykey.DeleteSubKeyTree(subkey, false);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            });
        }

        internal static bool CheckExists(in RegistryKey registrykey, in string subkey, in bool isNegation = true)
        {
            bool result = registrykey.OpenSubKey(subkey) != null;
            return isNegation ? result : !result;
        }

        internal static bool CheckValue(in string subkey, in string valueName, in string expectedValue, in bool isNegation = true)
        {
            string value = Registry.GetValue(subkey, valueName, null)?.ToString();
            bool result = value != null && value == expectedValue;
            return isNegation ? !result : result;
        }

        internal static bool CheckValueBytes(in string subkey, in string valueName, in string expectedValue)
        {
            if (!(Registry.GetValue(subkey, valueName, null) is byte[]))
                return true;

            return string.Concat((byte[])Registry.GetValue(subkey, valueName, null) ?? Array.Empty<byte>()) != expectedValue;
        }

        internal static T GetValue<T>(in string subKey, in string valueName, in T defaultValue)
        {
            try
            {
                return (T)Convert.ChangeType(Registry.GetValue(subKey, valueName, defaultValue), typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
