using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using Microsoft.Win32;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class InterfaceTweaks
    {
        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);

        internal void AnalyzeAndUpdate()
        {
            _сontrolWriter.ColorPicker[1] = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Hilight", "0 120 215");

            _сontrolWriter.ColorPicker[2] = RegistryHelp.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "InfoWindow", "255 255 225");

            _сontrolWriter.Checkbox[1] = HardwareData.OS.IsWin11 &&
                ((HardwareData.OS.Build > 22621m && (RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}") ||
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}"))) ||
                (HardwareData.OS.Build <= 22621m && (RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace_36354489\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}") ||
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace_36354489\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}"))));

            _сontrolWriter.Checkbox[2] = HardwareData.OS.IsWin11 &&
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}") ||
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");

            _сontrolWriter.Checkbox[3] =
                RegistryHelp.CheckValue(@"HKEY_CLASSES_ROOT\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", "0");

            foreach ((int i, string n, string v) in new (int Index, string Name, string Value)[] { (4, "Hidden", "1"), (5, "ShowSuperHidden", "1"), (6, "HideFileExt", "0"), (7, "HideDrivesWithNoMedia", "0") })
            {
                _сontrolWriter.Checkbox[i] = RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", n, v, true);
            }

            foreach ((int i, string g) in new (int Index, string Guid)[] { (8, "{20D04FE0-3AEA-1069-A2D8-08002B30309D}"), (9, "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}"), (10, "{645FF040-5081-101B-9F08-00AA002F954E}"), (11, "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}"), (12, "{59031A47-3F72-44A7-89C5-5595FE6B30EE}") })
            {
                _сontrolWriter.Checkbox[i] = RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", g, "0", true);
            }

            _сontrolWriter.Button[1] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionHeight", "-270") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionWidth", "-270");

            _сontrolWriter.Button[2] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "CursorBlinkRate", "530", true);

            _сontrolWriter.Button[3] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollHeight", "-210") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollWidth", "-210");

            _сontrolWriter.Button[4] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", "0");

            _сontrolWriter.Button[5] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1");

            _сontrolWriter.Button[6] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");

            _сontrolWriter.Button[7] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "20");

            _сontrolWriter.Button[8] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseHoverTime", "20");

            _сontrolWriter.Button[9] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0");

            _сontrolWriter.Button[10] =
                RegistryHelp.CheckValueBytes(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "link", "0000");

            _сontrolWriter.Button[11] =
                HardwareData.OS.IsWin11 && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", "0");

            _сontrolWriter.Button[12] =
                HardwareData.OS.IsWin11 &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", "1") ||
                HardwareData.OS.Name.IndexOf("home", StringComparison.OrdinalIgnoreCase) < 0 &&
                (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", "1")));

            _сontrolWriter.Button[13] =
                HardwareData.OS.IsWin11 && RegistryHelp.KeyExists(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", true);

            _сontrolWriter.Button[14] =
                HardwareData.OS.IsWin11 &&
               (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
               RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", "0"));

            _сontrolWriter.Button[15] =
                HardwareData.OS.IsWin11 && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", "1", true);

            _сontrolWriter.Button[16] =
                HardwareData.OS.IsWin11 &&
                (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1"));

            _сontrolWriter.Button[17] =
                HardwareData.OS.IsWin11 && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", "1", true);

            _сontrolWriter.Button[18] =
                HardwareData.OS.IsWin11 && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", "0");

            _сontrolWriter.Button[19] =
                HardwareData.OS.IsWin11 &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0")) ||
                HardwareData.OS.IsWin10 &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0"));

            _сontrolWriter.Button[20] =
                HardwareData.OS.IsWin11 &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", "0")) ||
                HardwareData.OS.IsWin10 &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0"));

            _сontrolWriter.Button[21] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", "0", true);

            _сontrolWriter.Button[22] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", "0") ||
                (HardwareData.OS.IsWin11 && (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0") || RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", "0"))) ||
                (HardwareData.OS.IsWin10 && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", "1"));

            _сontrolWriter.Button[23] =
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");

            _сontrolWriter.Button[24] =
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}") ||
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");

            _сontrolWriter.Button[25] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "JPEGImportQuality", "100");

            _сontrolWriter.Button[26] =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", "0");

            _сontrolWriter.Button[27] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", "1");

            _сontrolWriter.Button[28] =
                HardwareData.OS.Build >= 22621m ? RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", "1") :
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", "1");

            _сontrolWriter.Button[29] =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC", "1") ||
                (HardwareData.OS.Build >= 22000m && RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption", "0"));
        }

        internal void ApplyTweaksColor(string tweak, string value)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, tweak, value);

            switch (tweak)
            {
                case "ColorPicker1":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "Hilight", value, RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "HotTrackingColor", value == "0 120 215" ? "0 102 204" : value, RegistryValueKind.String);
                    break;
                case "ColorPicker2":
                    RegistryHelp.Write(Registry.Users, @".DEFAULT\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                    RegistryHelp.Write(Registry.Users, @"S-1-5-19\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                    RegistryHelp.Write(Registry.Users, @"S-1-5-20\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                    break;
                default:
                    break;
            }
        }

        internal void ApplyTweaksCheckBox(string tweak, bool isDisabled)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, tweak, isDisabled);

            switch (tweak)
            {
                case "Checkbox1":
                    string baseKey = HardwareData.OS.Build > 22621m ? @"NameSpace" : @"NameSpace_36354489";
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, $@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}");
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}", string.Empty, @"CLSID_MSGraphHomeFolder", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, $@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}", string.Empty, @"CLSID_MSGraphHomeFolder", RegistryValueKind.String);
                    }
                    break;
                case "Checkbox2":
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", string.Empty, @"{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", string.Empty, @"{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", RegistryValueKind.String);
                    }
                    break;
                case "Checkbox3":
                    RegistryHelp.Write(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", isDisabled ? 0 : 1, RegistryValueKind.String);
                    RegistryHelp.Write(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", isDisabled ? 0 : 1, RegistryValueKind.String);
                    break;
                case string checkbox when new[] { "Checkbox4", "Checkbox5", "Checkbox6", "Checkbox7" }.Contains(checkbox):
                    {
                        (string name, int value) = checkbox switch
                        {
                            "Checkbox4" => ("Hidden", 1),
                            "Checkbox5" => ("ShowSuperHidden", 1),
                            "Checkbox6" => ("HideFileExt", 0),
                            _ => ("HideDrivesWithNoMedia", 0)
                        };

                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", name, isDisabled ? 1 - value : value, RegistryValueKind.DWord);
                    }

                    break;
                case string checkbox when new[] { "Checkbox8", "Checkbox9", "Checkbox10", "Checkbox11", "Checkbox12" }.Contains(checkbox):
                    {
                        (string guid, int value, bool isDelete) = checkbox switch
                        {
                            "Checkbox8" => ("{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 0, true),
                            "Checkbox9" => ("{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", 0, false),
                            "Checkbox10" => ("{645FF040-5081-101B-9F08-00AA002F954E}", 0, false),
                            "Checkbox11" => ("{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", 0, false),
                            _ => ("{59031A47-3F72-44A7-89C5-5595FE6B30EE}", 0, false)
                        };

                        if (isDisabled && isDelete)
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", guid);
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", guid, isDisabled ? 1 - value : value, RegistryValueKind.DWord);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        internal void ApplyTweaks(string tweak, bool isDisabled)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, tweak, isDisabled);

            switch (tweak)
            {
                case "TglButton1":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionHeight", isDisabled ? "-270" : "-330", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionWidth", isDisabled ? "-270" : "-330", RegistryValueKind.String);
                    break;
                case "TglButton2":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "CursorBlinkRate", isDisabled ? "200" : "530", RegistryValueKind.String);
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollHeight", isDisabled ? "-210" : "-255", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollWidth", isDisabled ? "-210" : "-255", RegistryValueKind.String);
                    break;
                case "TglButton4":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton5":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton6":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton7":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", isDisabled ? "20" : "400", RegistryValueKind.String);
                    break;
                case "TglButton8":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseHoverTime", isDisabled ? "20" : "400", RegistryValueKind.String);
                    break;
                case "TglButton9":
                    try
                    {
                        Task.Run(delegate
                        {
                            if (isDisabled)
                            {
                                ArchiveManager.Unarchive(PathLocator.Files.BlankIcon, Properties.Resources.Blank);
                                RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0", RegistryValueKind.String);
                            }
                            else
                            {
                                if (File.Exists(PathLocator.Files.BlankIcon))
                                {
                                    File.Delete(PathLocator.Files.BlankIcon);
                                }

                                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
                            }
                        });
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    break;
                case "TglButton10":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link", Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link");
                    }
                    break;
                case "TglButton11":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton12":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", isDisabled ? 1 : 0, RegistryValueKind.DWord);

                    if (HardwareData.OS.Name.IndexOf("Home", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        if (isDisabled)
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Start");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Education");
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection");
                        }
                    }
                    break;
                case "TglButton13":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
                    }
                    break;
                case "TglButton14":
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton16":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSAIFabricSvc", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "CopilotPWAPin", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "RecallPin", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarCompanion", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\3389499533", "EnabledState", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\4027803789", "EnabledState", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\450471565", "EnabledState", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\2283032206", "EnabledState", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\502943886", "EnabledState", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\generativeAI", "Value", "Deny", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\systemAIModels", "Value", "Deny", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsCopilot", "AllowCopilotRuntime", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot\BingChat", "IsUserEligible", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "IsCopilotAvailable", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "CopilotDisabledReason", "FeatureIsDisabled", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeEntraCopilotPageContext", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "Microsoft365CopilotChatIconEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeHistoryAISearchEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "GenAILocalFoundationalModelSettings", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BuiltInAIAPIsEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "AIGenThemesEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DevToolsGenAiSettings", 2, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ShareBrowsingHistoryWithCopilotSearchAllowed", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableImageCreator", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableCocreator", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeFill", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeErase", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableRemoveBackground", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\WindowsNotepad", "DisableAIFeatures", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "CopilotPWAPin", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "RecallPin", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarCompanion", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\3389499533", "EnabledState", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\4027803789", "EnabledState", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\450471565", "EnabledState", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\2283032206", "EnabledState", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\502943886", "EnabledState", 0, RegistryValueKind.DWord);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\generativeAI");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\systemAIModels");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors");
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 0, RegistryValueKind.DWord);
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsCopilot", "AllowCopilotRuntime", 1, RegistryValueKind.DWord);
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeEntraCopilotPageContext");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "Microsoft365CopilotChatIconEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeHistoryAISearchEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "GenAILocalFoundationalModelSettings");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BuiltInAIAPIsEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "AIGenThemesEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DevToolsGenAiSettings");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ShareBrowsingHistoryWithCopilotSearchAllowed");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\WindowsNotepad");
                    }
                    break;
                case "TglButton17":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton18":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton19":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (HardwareData.OS.IsWin11)
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller(@"/c del /q /f ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*Copilot*.lnk""");
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                        if (isDisabled && HardwareData.OS.Build.CompareTo(22635.3785m) >= 0)
                        {
                            RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{2cc5ca98-6485-489a-920e-b3e88a6ccce3}");
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", "", "Windows Spotlight", RegistryValueKind.String);
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}");
                        }
                    }
                    else if (HardwareData.OS.IsWin10)
                    {
                        if (isDisabled)
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\NewsAndInterests\AllowNewsAndInterests", "value", 0, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\NewsAndInterests\AllowNewsAndInterests");
                        }
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", isDisabled ? 2 : 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton20":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (HardwareData.OS.IsWin10)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }
                    else if (HardwareData.OS.IsWin11)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton21":
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers");
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton22":

                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled");
                    }

                    if (HardwareData.OS.IsWin10)
                    {
                        if (isDisabled)
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");
                        }
                    }
                    else if (HardwareData.OS.IsWin11)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton23":
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");

                    }
                    else
                    {
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");
                    }
                    break;
                case "TglButton24":
                    if (isDisabled)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                    }
                    else
                    {
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                    }

                    CommandExecutor.RunCommand(isDisabled ? @"/c rd /checkbox /q ""%userprofile%\3D Objects""" : @"/c mkdir ""%userprofile%\3D Objects""");
                    break;
                case "TglButton25":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality");
                    }
                    break;
                case "TglButton26":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton27":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer");
                    }
                    break;
                case "TglButton28":
                    if (HardwareData.OS.Build >= 22621m)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", isDisabled ? 1 : 2, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton29":
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC", 1, RegistryValueKind.DWord);
                        if (HardwareData.OS.Build >= 22000m)
                        {
                            RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption", 0, RegistryValueKind.DWord);
                        }
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC");
                        if (HardwareData.OS.Build >= 22000m)
                        {
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption");
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
