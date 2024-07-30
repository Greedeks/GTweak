using Microsoft.Win32;

namespace GTweak.Utilities.Helpers
{
    internal struct RegistryHelp
    {
        internal static void DeleteFolderTree(in RegistryKey _registrykey, in string _subkey)
        {
            if (_registrykey.OpenSubKey(_subkey) != null)
                try { _registrykey.DeleteSubKeyTree(_subkey); } catch { }
        }

        internal static void DeleteValue(in RegistryKey _registrykey, in string _subkey, in string _value)
        {
            if (_registrykey.OpenSubKey(_subkey) != null && _registrykey.OpenSubKey(_subkey)?.GetValue(_value, null) != null)
                try { _registrykey.OpenSubKey(_subkey, true)?.DeleteValue(_value); } catch { };
        }
        
        internal static void Write<T>(in RegistryKey _registrykey, in string _key, in string _name, in T _data, in RegistryValueKind _kind)
        {
            try { _registrykey.CreateSubKey(_key, true)?.SetValue(_name, _data, _kind); } catch { }
        }


        internal static void CreateFolder(in RegistryKey _registrykey, in string _subkey)
        {
            try { _registrykey.CreateSubKey(_subkey); } catch { }
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
                        catch { }
                    }
                }

                Registry.LocalMachine.DeleteSubKeyTree(_key, false);
            }
            catch { }
        }
    }
}
