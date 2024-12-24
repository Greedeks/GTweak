using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace GTweak.Utilities.Helpers
{
    internal sealed class RegistryHelp
    {
        internal static void DeleteFolderTree(in RegistryKey registrykey, in string subkey)
        {
            if (registrykey.OpenSubKey(subkey) == null) return;
            try { registrykey.DeleteSubKeyTree(subkey); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void DeleteValue(in RegistryKey registrykey, in string subkey, in string value)
        {
            if (registrykey.OpenSubKey(subkey) == null || registrykey.OpenSubKey(subkey)?.GetValue(value, null) == null) return;
            try { registrykey.OpenSubKey(subkey, true)?.DeleteValue(value); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void Write<T>(in RegistryKey registrykey, in string key, in string name, in T data, in RegistryValueKind kind)
        {
            try { registrykey.CreateSubKey(key, true)?.SetValue(name, data, kind); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void CreateFolder(in RegistryKey registrykey, in string subkey)
        {
            try { registrykey.CreateSubKey(subkey); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void DeleteRegristyTree(in RegistryKey registrykey, in string key)
        {
            try
            {
                RegistryKey registryFolder = registrykey.OpenSubKey(key, true);

                if (registryFolder != null)
                {
                    foreach (string value in registryFolder.GetValueNames())
                    {
                        try { registryFolder.DeleteValue(value); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    }
                }

                Registry.LocalMachine.DeleteSubKeyTree(key, false);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static bool CheckExists(in RegistryKey registrykey, in string key, in bool isNegation = true)
        {
            bool result = registrykey.OpenSubKey(key) != null;
            return isNegation ? result : !result;
        }

        internal static bool CheckValue(in string key, in string valueName, in string expectedValue, in bool isNegation = true)
        {
            string value = Registry.GetValue(key, valueName, null)?.ToString();
            bool result = value != null && value == expectedValue;
            return isNegation ? !result : result;
        }

        internal static bool CheckValueBytes(in string key, in string valueName, in string expectedValue)
        {
            if (!(Registry.GetValue(key, valueName, null) is byte[]))
                return true;

            return string.Concat((byte[])Registry.GetValue(key, valueName, null) ?? Array.Empty<byte>()) != expectedValue;
        }

    }
}
