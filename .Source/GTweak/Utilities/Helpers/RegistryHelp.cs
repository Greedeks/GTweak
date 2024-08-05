using Microsoft.Win32;
using System.Diagnostics;
using System;

namespace GTweak.Utilities.Helpers
{
    internal readonly struct RegistryHelp
    {
        internal static  void DeleteFolderTree(in RegistryKey _registrykey, in string _subkey)
        {
            if (_registrykey.OpenSubKey(_subkey) != null)
                try { _registrykey.DeleteSubKeyTree(_subkey); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void DeleteValue(in RegistryKey _registrykey, in string _subkey, in string _value)
        {
            if (_registrykey.OpenSubKey(_subkey) != null && _registrykey.OpenSubKey(_subkey)?.GetValue(_value, null) != null)
                try { _registrykey.OpenSubKey(_subkey, true)?.DeleteValue(_value); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }
        
        internal static void Write<T>(in RegistryKey _registrykey, in string _key, in string _name, in T _data, in RegistryValueKind _kind)
        {
            try { _registrykey.CreateSubKey(_key, true)?.SetValue(_name, _data, _kind); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }


        internal static void CreateFolder(in RegistryKey _registrykey, in string _subkey)
        {
            try { _registrykey.CreateSubKey(_subkey); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }

        internal static void DeleteRegristyTree(in RegistryKey _registrykey, in string _key)
        {
            try
            {
                RegistryKey _registryFolder = _registrykey.OpenSubKey(_key, true);

                if (_registryFolder != null)
                {
                    foreach (string value in _registryFolder.GetValueNames())
                    {
                        try
                        {
                            _registryFolder.DeleteValue(value);
                        }
                        catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                    }
                }

                Registry.LocalMachine.DeleteSubKeyTree(_key, false);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
        }
    }
}
