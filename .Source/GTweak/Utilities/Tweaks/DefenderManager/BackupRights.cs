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
        private static readonly Dictionary<string, RegistryHive> _storageRegPaths = new Dictionary<string, RegistryHive>
        {
            [@"SYSTEM\CurrentControlSet\Services\WinDefend"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\SecurityHealthService"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\Sense"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\SgrmAgent"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\SgrmBroker"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\WdBoot"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\WdNisDrv"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\WdNisSvc"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\MDCoreSvc"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\MsSecCore"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\MsSecFlt"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\MsSecWfp"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\webthreatdefsvc"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\webthreatdefusersvc"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\CI"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\CI\Policy"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\CI\State"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\CI\Config"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\DeviceGuard"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Features"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Exclusions"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Cloud"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\CoreService"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Windows Defender\ControlledFolderAccess"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\SpyNet"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Policy Manager"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\MpEngine"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Scan"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\SmartScreen"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Reporting"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\UX Configuration"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Security Center"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender\Signature Updates"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Security Center"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender Security Center"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Notifications"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows\System"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppHost"] = RegistryHive.LocalMachine,
            [@"Software\Microsoft\Windows\CurrentVersion\AppHost"] = RegistryHive.CurrentUser,
            [@"SOFTWARE\Policies\Microsoft\MicrosoftEdge\PhishingFilter"] = RegistryHive.LocalMachine,
            [@"Software\Microsoft\Edge"] = RegistryHive.CurrentUser,
            [@"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\MRT"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Microsoft Antimalware"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Policies\Microsoft\Windows\WTDS"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Windows Defender/Operational"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Security-Diagnostic/Operational"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceConfidence/Analytic"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Operational"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-DeviceGuard/Verbose"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\WOW6432Node\Policies\Microsoft\Windows Defender"] = RegistryHive.LocalMachine,
            [@"CLSID\{09A47860-11B0-4DA5-AFA5-26D86198A780}"] = RegistryHive.ClassesRoot,
            [@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\ASR"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Controlled Folder Access"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender\Windows Defender Exploit Guard\Network Protection"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Advanced Threat Protection"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows Defender Security Center\Notifications"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"] = RegistryHive.LocalMachine,
        };

        internal static void ExportRights()
        {
            try
            {
                Directory.CreateDirectory(PathLocator.Folders.DefenderBackup);

                Dictionary<string, Dictionary<string, object>> allValues = new Dictionary<string, Dictionary<string, object>>();
                Dictionary<string, string> aclDataDict = new Dictionary<string, string>();

                if (_storageRegPaths == null || _storageRegPaths.Count == 0)
                {
                    return;
                }

                foreach (var entry in _storageRegPaths)
                {
                    string path = entry.Key;
                    RegistryHive hive = entry.Value;

                    try
                    {
                        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                        using RegistryKey key = baseKey.OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey | RegistryRights.ReadPermissions);

                        if (key == null)
                        {
                            continue;
                        }
                        Dictionary<string, object> values = new Dictionary<string, object>();
                        foreach (string valueName in key.GetValueNames())
                        {
                            object val = key.GetValue(valueName);
                            if (val is byte[] byteVal)
                            {
                                values[valueName] = Convert.ToBase64String(byteVal);
                            }
                            else
                            {
                                values[valueName] = val;
                            }
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
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }

                File.WriteAllText(PathLocator.Files.BackupDataJson, JsonConvert.SerializeObject(allValues, Formatting.Indented));
                File.WriteAllText(PathLocator.Files.BackupRightsAcl, JsonConvert.SerializeObject(aclDataDict, Formatting.Indented));

            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }


        internal static void ImportRights()
        {
            if (File.Exists(PathLocator.Files.BackupDataJson) && File.Exists(PathLocator.Files.BackupRightsAcl))
            {
                try
                {
                    Dictionary<string, Dictionary<string, object>> allValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(PathLocator.Files.BackupDataJson));
                    Dictionary<string, string> aclDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PathLocator.Files.BackupRightsAcl));

                    foreach (var entry in _storageRegPaths)
                    {
                        string path = entry.Key;
                        RegistryHive hive = entry.Value;

                        try
                        {
                            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
                            using RegistryKey key = baseKey.OpenSubKey(path, true) ?? baseKey.CreateSubKey(path);

                            if (key == null)
                            {
                                continue;
                            }

                            if (allValues.ContainsKey(path))
                            {
                                foreach (var pair in allValues[path])
                                {
                                    object val = pair.Value;

                                    if (val is string sVal)
                                    {
                                        try
                                        {
                                            val = Convert.FromBase64String(sVal);
                                        }
                                        catch
                                        {
                                            val = sVal;
                                        }
                                    }

                                    key.SetValue(pair.Key, val);
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
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }
    }
}
