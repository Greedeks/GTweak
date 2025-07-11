using GTweak.Utilities.Configuration;
using GTweak.Utilities.Controls;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Managers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class InterfaceTweaks
    {
        internal void AnalyzeAndUpdate(InterfaceView interfaceV)
        {
            interfaceV.TglButton1.StateNA =
                 RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Hilight", "80 80 80") ||
                 RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "HotTrackingColor", "80 80 80");

            interfaceV.TglButton2.StateNA =
                RegistryHelp.CheckValue(@"HKEY_USERS\.DEFAULT\Control Panel\Colors", "InfoWindow", "240 255 255") ||
                RegistryHelp.CheckValue(@"HKEY_USERS\S-1-5-19\Control Panel\Colors", "InfoWindow", "240 255 255") ||
                RegistryHelp.CheckValue(@"HKEY_USERS\S-1-5-20\Control Panel\Colors", "InfoWindow", "240 255 255");

            interfaceV.TglButton3.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionHeight", "-270") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionWidth", "-270");

            interfaceV.TglButton4.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "CursorBlinkRate", "530", false);

            interfaceV.TglButton5.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollHeight", "-210") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollWidth", "-210");

            interfaceV.TglButton6.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", "0");

            interfaceV.TglButton7.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1");

            interfaceV.TglButton8.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1");

            interfaceV.TglButton9.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "20");

            interfaceV.TglButton10.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseHoverTime", "20");

            interfaceV.TglButton11.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0");

            interfaceV.TglButton12.StateNA =
                RegistryHelp.CheckValueBytes(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "link", "0000");

            interfaceV.TglButton13.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", "0", false);

            interfaceV.TglButton14.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", "1");

            interfaceV.TglButton15.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", "0");

            interfaceV.TglButton16.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", "1") ||
                SystemDiagnostics.HardwareData.OSVersion.IndexOf("home", StringComparison.OrdinalIgnoreCase) < 0 &&
                (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", "1")));

            interfaceV.TglButton17.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] && RegistryHelp.KeyExists(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", false);

            interfaceV.TglButton18.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] &&
               (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
               RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", "0"));

            interfaceV.TglButton19.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", "1", false);

            interfaceV.TglButton20.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] &&
                (RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1") ||
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1"));

            interfaceV.TglButton21.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", "1", false);

            interfaceV.TglButton22.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0")) ||

                SystemDiagnostics.IsWindowsVersion[10] &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0"));

            interfaceV.TglButton23.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] &&
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

                SystemDiagnostics.IsWindowsVersion[10] &&
                (RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0"));

            interfaceV.TglButton24.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", "0", false);

            interfaceV.TglButton25.StateNA =
            RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", "0") ||
            RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", "0") ||

            (SystemDiagnostics.IsWindowsVersion[11] && (
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0") ||
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", "0"))) ||

            (SystemDiagnostics.IsWindowsVersion[10] &&
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", "1"));

            interfaceV.TglButton26.StateNA =
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}") ||
                 RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");

            interfaceV.TglButton27.StateNA =
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}") ||
                RegistryHelp.KeyExists(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");

            interfaceV.TglButton28.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "JPEGImportQuality", "100");

            interfaceV.TglButton29.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", "0");

            interfaceV.TglButton30.StateNA =
                RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", "1");

            interfaceV.TglButton31.StateNA =
                SystemDiagnostics.IsWindowsVersion[11] && RegistryHelp.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", "1") ||
                SystemDiagnostics.IsWindowsVersion[10] && RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", "1");

            interfaceV.TglButton32.StateNA =
                RegistryHelp.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", "1");
        }

        internal static void ApplyTweaks(string tweak, bool isDisabled)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, tweak, isDisabled);

            switch (tweak)
            {
                case "TglButton1":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "Hilight", isDisabled ? "80 80 80" : "0 120 215", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "HotTrackingColor", isDisabled ? "80 80 80" : "0 102 204", RegistryValueKind.String);
                    break;
                case "TglButton2":
                    RegistryHelp.Write(Registry.Users, @".DEFAULT\Control Panel\Colors", "InfoWindow", isDisabled ? "240 255 255" : "255 255 255", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.Users, @"S-1-5-19\Control Panel\Colors", "InfoWindow", isDisabled ? "240 255 255" : "255 255 255", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.Users, @"S-1-5-20\Control Panel\Colors", "InfoWindow", isDisabled ? "240 255 255" : "255 255 255", RegistryValueKind.String);
                    break;
                case "TglButton3":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionHeight", isDisabled ? "-270" : "-330", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionWidth", isDisabled ? "-270" : "-330", RegistryValueKind.String);
                    break;
                case "TglButton4":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "CursorBlinkRate", isDisabled ? "200" : "530", RegistryValueKind.String);
                    break;
                case "TglButton5":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollHeight", isDisabled ? "-210" : "-255", RegistryValueKind.String);
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollWidth", isDisabled ? "-210" : "-255", RegistryValueKind.String);
                    break;
                case "TglButton6":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton7":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton8":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton9":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", isDisabled ? "20" : "400", RegistryValueKind.String);
                    break;
                case "TglButton10":
                    RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseHoverTime", isDisabled ? "20" : "400", RegistryValueKind.String);
                    break;
                case "TglButton11":
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
                                    File.Delete(PathLocator.Files.BlankIcon);
                                RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
                            }
                        });
                    }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                    break;
                case "TglButton12":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link", Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link");
                    break;
                case "TglButton13":
                    if (isDisabled)
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}");
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 0, RegistryValueKind.DWord);
                    break;
                case "TglButton14":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton16":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", isDisabled ? 1 : 0, RegistryValueKind.DWord);

                    if (SystemDiagnostics.HardwareData.OperatingSystem.IndexOf("Home", StringComparison.OrdinalIgnoreCase) < 0)
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
                case "TglButton17":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
                    else
                        RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
                    break;
                case "TglButton18":
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton19":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton20":
                    RegistryHelp.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSAIFabricSvc", "Start", isDisabled ? 4 : 2, RegistryValueKind.DWord);
                    if (isDisabled)
                    {
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot\BingChat", "IsUserEligible", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "IsCopilotAvailable", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "CopilotDisabledReason", "FeatureIsDisabled", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableImageCreator", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableCocreator", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeFill", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo");
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 0, RegistryValueKind.DWord);
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint");
                    }
                    break;
                case "TglButton21":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton22":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (SystemDiagnostics.IsWindowsVersion[11])
                    {
                        CommandExecutor.RunCommandAsTrustedInstaller(@"/c del /q /f ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*Copilot*.lnk""");
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                        if (isDisabled && SystemDiagnostics.HardwareData.OSBuild.CompareTo("22635.3785") >= 0)
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
                    else if (SystemDiagnostics.IsWindowsVersion[10])
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
                case "TglButton23":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);

                    if (SystemDiagnostics.IsWindowsVersion[10])
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }

                    if (SystemDiagnostics.IsWindowsVersion[11])
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
                case "TglButton24":
                    if (isDisabled)
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers");
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton25":

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

                    if (SystemDiagnostics.IsWindowsVersion[10])
                    {
                        if (isDisabled)
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, RegistryValueKind.DWord);
                        else
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");
                    }

                    if (SystemDiagnostics.IsWindowsVersion[11])
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton26":
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
                case "TglButton27":
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

                    CommandExecutor.RunCommand(isDisabled ? @"/c rd /s /q ""%userprofile%\3D Objects""" : @"/c mkdir ""%userprofile%\3D Objects""");
                    break;
                case "TglButton28":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality");
                    break;
                case "TglButton29":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", isDisabled ? 0 : 1, RegistryValueKind.DWord);
                    break;
                case "TglButton30":
                    if (isDisabled)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer");
                    break;
                case "TglButton31":
                    if (isDisabled)
                    {
                        if (SystemDiagnostics.IsWindowsVersion[11])
                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", 1, RegistryValueKind.DWord);
                        else if (SystemDiagnostics.IsWindowsVersion[10])
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        if (SystemDiagnostics.IsWindowsVersion[11])
                            RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode");
                        else if (SystemDiagnostics.IsWindowsVersion[10])
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo");
                    }
                    break;
                case "TglButton32":
                    RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", isDisabled ? 1 : 0, RegistryValueKind.DWord);
                    break;

            }
        }


    }
}
