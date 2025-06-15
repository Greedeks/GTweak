using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace GTweak.Utilities.Tweaks.DefenderManager
{
    internal class BackupRights : TaskSchedulerManager
    {
        private static readonly string _folderBackupPath = Path.Combine(PathLocator.Folders.SystemDrive, @"Windows\System32\Config\WDBackup_GTweak");
        private static readonly string _jsonFilePath = Path.Combine(_folderBackupPath, "BackupData.json");
        private static readonly string _aclFilePath = Path.Combine(_folderBackupPath, "BackupRights.acl");

        private static readonly Dictionary<string, RegistryKey> _storageRegPaths = new Dictionary<string, RegistryKey>
        {
            { @"SYSTEM\CurrentControlSet\Services\WinDefend", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\SecurityHealthService", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\Sense", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\SgrmAgent", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\SgrmBroker", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdBoot", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdNisDrv", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\WdNisSvc", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\MDCoreSvc", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\MsSecCore", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\MsSecFlt", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\MsSecWfp", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\webthreatdefsvc", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\webthreatdefusersvc", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Features", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Security Center", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\SpyNet", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows\System", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\MRT", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Windows Defender/Operational", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Security-Diagnostic/Operational", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Cloud", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\smartscreen.exe", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\SgrmBroker.exe", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\MsMpEng.exe", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\MpDefenderCoreService.exe", Registry.LocalMachine },
            { @"Software\Microsoft\Edge", Registry.CurrentUser },
        };

        internal static void ExportRights()
        {
            if (Directory.Exists(_folderBackupPath) == false)
            {
                try
                {
                    Directory.CreateDirectory(_folderBackupPath);

                    var allValues = new Dictionary<string, Dictionary<string, object>>();
                    var aclDataDict = new Dictionary<string, string>();

                    foreach (var entry in _storageRegPaths)
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

                    File.WriteAllText(_jsonFilePath, JsonConvert.SerializeObject(allValues, Formatting.Indented));
                    File.WriteAllText(_aclFilePath, JsonConvert.SerializeObject(aclDataDict, Formatting.Indented));
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }


        internal static void ImportRights()
        {
            if (File.Exists(_jsonFilePath) && File.Exists(_aclFilePath))
            {
                try
                {
                    var allValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(_jsonFilePath));
                    var aclDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_aclFilePath));

                    foreach (var entry in _storageRegPaths)
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
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }
    }
}