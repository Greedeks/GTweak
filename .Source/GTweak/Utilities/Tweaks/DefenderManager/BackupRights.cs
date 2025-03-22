using GTweak.Utilities.Control;
using GTweak.Utilities.Helpers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;

namespace GTweak.Utilities.Tweaks.DefenderManager
{
    internal class BackupRights : TaskSchedulerManager
    {
        private static readonly Dictionary<string, RegistryKey> registryPaths = new Dictionary<string, RegistryKey>
        {
            { @"SYSTEM\CurrentControlSet\Services\WinDefend", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Features", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdNisSvc", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\Sense", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdFilter", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdBoot", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdNisDrv", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", Registry.LocalMachine },
            { @"Software\Policies\Microsoft\Windows Defender\Real-Time Protection", Registry.LocalMachine },
            { @"Software\Policies\Microsoft\Windows Defender\SpyNet", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows\System", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\MRT", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Security Center", Registry.LocalMachine },
        };

        internal static readonly string jsonFilePath = Path.Combine(StoragePaths.SystemDisk, @"Windows\System32\Config", "backup_GTweak.json");
        internal static readonly string aclFilePath = Path.Combine(StoragePaths.SystemDisk, @"Windows\System32\Config", "backup_GTweak.acl");

        internal static void ExportRights()
        {
            try
            {
                if (File.Exists(jsonFilePath) && File.Exists(aclFilePath))
                {
                    File.Delete(jsonFilePath);
                    File.Delete(aclFilePath);
                }

                var allValues = new Dictionary<string, Dictionary<string, object>>();
                var aclDataDict = new Dictionary<string, string>();

                foreach (var entry in registryPaths)
                {
                    string path = entry.Key;
                    RegistryKey baseKey = entry.Value;

                    using RegistryKey key = baseKey.OpenSubKey(path, false);
                    if (key == null)
                        continue;

                    var values = new Dictionary<string, object>();
                    foreach (string valueName in key.GetValueNames())
                        values[valueName] = key.GetValue(valueName);

                    allValues[path] = values;

                    RegistrySecurity security = key.GetAccessControl();
                    string aclData = security.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                    aclDataDict[path] = aclData;
                }

                File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(allValues, Formatting.Indented));
                File.WriteAllText(aclFilePath, JsonConvert.SerializeObject(aclDataDict, Formatting.Indented));
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }

        internal static void ImportRights()
        {
            try
            {
                var allValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(jsonFilePath));
                var aclDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(aclFilePath));

                foreach (var entry in registryPaths)
                {
                    string path = entry.Key;
                    RegistryKey baseKey = entry.Value;

                    using RegistryKey key = baseKey.OpenSubKey(path, true) ?? baseKey.CreateSubKey(path);
                    if (key == null)
                        continue;

                    if (allValues.ContainsKey(path))
                    {
                        foreach (var pair in allValues[path])
                            key.SetValue(pair.Key, pair.Value);
                    }

                    if (aclDataDict.ContainsKey(path))
                    {
                        string aclData = aclDataDict[path];
                        RegistrySecurity security = new RegistrySecurity();
                        security.SetSecurityDescriptorSddlForm(aclData);
                        key.SetAccessControl(security);
                    }
                }

                File.Delete(jsonFilePath);
                File.Delete(aclFilePath);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
        }
    }
}