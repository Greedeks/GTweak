using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Managers;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            [@"SYSTEM\CurrentControlSet\Services\wscsvc"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\WdFilter"] = RegistryHive.LocalMachine,
            [@"SYSTEM\CurrentControlSet\Services\WdFilter\Instances\WdFilter Instance"] = RegistryHive.LocalMachine,
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
            [@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\MsMpEng.exe"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\runtimebroker.exe"] = RegistryHive.LocalMachine,
            [@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\MRT.exe"] = RegistryHive.LocalMachine,
        };

        private sealed class RegistryValueBackup
        {
            internal string Kind { get; set; }
            internal object Value { get; set; }
        }

        private class FileRightsInfo
        {
            internal string OriginalPath { get; set; }
            internal string Owner { get; set; }
            internal string Permissions { get; set; }
            internal string Sddl { get; set; }
        }

        internal static void ExportRights()
        {
            try
            {
                Directory.CreateDirectory(PathLocator.Folders.DefenderBackup);

                Dictionary<string, Dictionary<string, RegistryValueBackup>> allValues = new Dictionary<string, Dictionary<string, RegistryValueBackup>>(StringComparer.OrdinalIgnoreCase);
                Dictionary<string, string> aclDataDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (_storageRegPaths == null || _storageRegPaths.Count == 0)
                {
                    return;
                }

                foreach (var entry in _storageRegPaths)
                {
                    string path = entry.Key;
                    RegistryHive hive = entry.Value;

                    foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                    {
                        string keyId = $"{view}:{path}";

                        try
                        {
                            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                            using RegistryKey key = baseKey.OpenSubKey(path, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ReadKey | RegistryRights.ReadPermissions);

                            if (key == null)
                            {
                                continue;
                            }

                            var values = new Dictionary<string, RegistryValueBackup>(StringComparer.Ordinal);

                            foreach (string valueName in key.GetValueNames())
                            {
                                try
                                {
                                    RegistryValueKind kind = key.GetValueKind(valueName);
                                    object val = key.GetValue(valueName);

                                    if (val is byte[] bytes)
                                    {
                                        values[valueName] = new RegistryValueBackup
                                        {
                                            Kind = kind.ToString(),
                                            Value = Convert.ToBase64String(bytes)
                                        };
                                    }
                                    else
                                    {
                                        values[valueName] = new RegistryValueBackup
                                        {
                                            Kind = kind.ToString(),
                                            Value = val
                                        };
                                    }
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }

                            allValues[keyId] = values;

                            try
                            {
                                RegistrySecurity security = key.GetAccessControl(AccessControlSections.All);
                                string aclData = security.GetSecurityDescriptorSddlForm(AccessControlSections.All);
                                aclDataDict[keyId] = aclData;
                            }
                            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }
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
                    Dictionary<string, Dictionary<string, RegistryValueBackup>> allValues = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, RegistryValueBackup>>>(File.ReadAllText(PathLocator.Files.BackupDataJson));
                    Dictionary<string, string> aclDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(PathLocator.Files.BackupRightsAcl));

                    if (allValues == null)
                    {
                        return;
                    }

                    foreach (var kv in allValues)
                    {
                        string keyId = kv.Key;
                        string[] parts = keyId.Split(new[] { ':' }, 2);
                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        if (!Enum.TryParse(parts[0], out RegistryView view))
                        {
                            continue;
                        }

                        string path = parts[1];

                        if (!_storageRegPaths.TryGetValue(path, out RegistryHive hive))
                        {
                            hive = RegistryHive.LocalMachine;
                        }

                        try
                        {
                            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view);
                            using RegistryKey key = baseKey.CreateSubKey(path, true);

                            if (key == null)
                            {
                                continue;
                            }

                            foreach (var valuePair in kv.Value)
                            {
                                string valueName = valuePair.Key;
                                RegistryValueBackup backup = valuePair.Value;

                                if (backup == null)
                                {
                                    continue;
                                }

                                if (!Enum.TryParse(backup.Kind, out RegistryValueKind kind))
                                {
                                    try
                                    {
                                        key.SetValue(valueName, ConvertJTokenToObject(backup.Value));
                                    }
                                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }

                                    continue;
                                }

                                try
                                {
                                    object valueToSet = ConvertBackupValueToClr(backup.Value, kind);
                                    key.SetValue(valueName, valueToSet, kind);
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }

                            if (aclDataDict != null && aclDataDict.TryGetValue(keyId, out string sddl) && !string.IsNullOrWhiteSpace(sddl))
                            {
                                try
                                {
                                    RegistrySecurity security = new RegistrySecurity();
                                    security.SetSecurityDescriptorSddlForm(sddl);

                                    key.SetAccessControl(security);
                                }
                                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                            }
                        }
                        catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    }
                }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }
        }

        private static object ConvertBackupValueToClr(object rawValue, RegistryValueKind kind)
        {
            if (rawValue is JToken token)
            {
                switch (kind)
                {
                    case RegistryValueKind.Binary:
                        string b64 = token.Type == JTokenType.String ? token.ToObject<string>() : token.ToString(Formatting.None);
                        return Convert.FromBase64String(b64);
                    case RegistryValueKind.DWord:
                        return token.ToObject<int>();
                    case RegistryValueKind.QWord:
                        return token.ToObject<long>();
                    case RegistryValueKind.MultiString:
                        return token.ToObject<string[]>();
                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        return token.ToObject<string>();
                    case RegistryValueKind.None:
                    default:
                        return token.ToObject<object>();
                }
            }

            switch (kind)
            {
                case RegistryValueKind.Binary:
                    if (rawValue is string s)
                    {
                        return Convert.FromBase64String(s);
                    }

                    if (rawValue is byte[] bytes)
                    {
                        return bytes;
                    }

                    break;
                case RegistryValueKind.DWord:
                    return Convert.ToInt32(rawValue);
                case RegistryValueKind.QWord:
                    return Convert.ToInt64(rawValue);
                case RegistryValueKind.MultiString:
                    if (rawValue is JArray jarr)
                    {
                        return jarr.ToObject<string[]>();
                    }

                    if (rawValue is string[] sa)
                    {
                        return sa;
                    }

                    break;
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    return rawValue?.ToString();
                case RegistryValueKind.None:
                default:
                    return rawValue;
            }

            return rawValue;
        }

        private static object ConvertJTokenToObject(object tokenOrObj)
        {
            if (tokenOrObj is JToken t)
            {
                return t.ToObject<object>();
            }

            return tokenOrObj;
        }
    }
}
