using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GTweak.Utilities.Tweaks.DefenderManager
{
    internal class BackupRights : TaskSchedulerManager
    {
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
            { @"SYSTEM\CurrentControlSet\Control\CI", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\CI\Policy", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\CI\State", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\CI\Config", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\DeviceGuard", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Features", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Exclusions", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Cloud", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\CoreService", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Windows Defender\ControlledFolderAccess", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\SpyNet", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Policy Manager", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Scan", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Reporting", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\UX Configuration", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Security Center", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows defender\CoreService", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Security Center", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows\System", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost", Registry.LocalMachine },
            { @"Software\Microsoft\Windows\CurrentVersion\AppHost", Registry.CurrentUser },
            { @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\MRT", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Microsoft Antimalware", Registry.LocalMachine },
            { @"SOFTWARE\Policies\Microsoft\Windows\WTDS", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger", Registry.LocalMachine },
            { @"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Windows Defender/Operational", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Security-Diagnostic/Operational", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceConfidence/Analytic", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Operational", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Verbose", Registry.LocalMachine },
            { @"SOFTWARE\WOW6432Node\Policies\Microsoft\Windows Defender", Registry.LocalMachine },
            { @"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}", Registry.ClassesRoot },
            { @"Software\Microsoft\Edge", Registry.CurrentUser },
            { @"SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\ASR", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Controlled Folder Access", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows defender\Windows defender Exploit Guard\Network Protection", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Advanced Threat Protection", Registry.LocalMachine },
            { @"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications", Registry.LocalMachine },
        };

        internal static void ExportRights()
        {
            try
            {
                Directory.CreateDirectory(PathLocator.Folders.DefenderBackup);

                if (!File.Exists(PathLocator.Files.BackupDataJson) && !File.Exists(PathLocator.Files.BackupRightsAcl))
                {
                    Dictionary<string, Dictionary<string, object>> allValues = new Dictionary<string, Dictionary<string, object>>();
                    Dictionary<string, string> aclDataDict = new Dictionary<string, string>();

                    if (_storageRegPaths == null || _storageRegPaths.Count == 0)
                    {
                        return;
                    }

                    foreach (var entry in _storageRegPaths)
                    {
                        string path = entry.Key;
                        RegistryKey baseKey = entry.Value;

                        using RegistryKey key = baseKey.OpenSubKey(path, false);
                        if (key == null)
                        {
                            continue;
                        }

                        Dictionary<string, object> values = new Dictionary<string, object>();
                        foreach (string valueName in key.GetValueNames())
                        {
                            values[valueName] = key.GetValue(valueName);
                        }
                        allValues[path] = values;

                        try
                        {
                            RegistrySecurity security = key.GetAccessControl();
                            string aclData = security.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                            aclDataDict[path] = aclData;
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }

                    File.WriteAllText(PathLocator.Files.BackupDataJson, JsonConvert.SerializeObject(allValues, Formatting.Indented));
                    File.WriteAllText(PathLocator.Files.BackupRightsAcl, JsonConvert.SerializeObject(aclDataDict, Formatting.Indented));
                }
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal static void ImportRights()
        {
            if (File.Exists(PathLocator.Files.BackupDataJson) && File.Exists(PathLocator.Files.BackupRightsAcl))
            {
                try
                {
                    var allValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(PathLocator.Files.BackupDataJson));
                    var aclDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PathLocator.Files.BackupRightsAcl));

                    foreach (var entry in _storageRegPaths)
                    {
                        string path = entry.Key;
                        RegistryKey baseKey = entry.Value;

                        using RegistryKey key = baseKey.OpenSubKey(path, true) ?? baseKey.CreateSubKey(path);
                        if (key == null)
                        {
                            continue;
                        }

                        if (allValues.ContainsKey(path))
                        {
                            foreach (var pair in allValues[path])
                            {
                                key.SetValue(pair.Key, pair.Value);
                            }
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