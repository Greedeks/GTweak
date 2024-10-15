using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace GTweak.Utilities.Helpers
{
    internal readonly struct RegistryHelp
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
                        try
                        {
                            registryFolder.DeleteValue(value);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    }
                }

                Registry.LocalMachine.DeleteSubKeyTree(key, false);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }
    }
}
