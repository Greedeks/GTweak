using GTweak.Utilities.Helpers;
using GTweak.View;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class InterfaceTweaks
    {
        internal void ViewInterface(InterfaceView interfaceV)
        {
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Hilight", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Hilight", string.Empty).ToString() != "80 80 80" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "HotTrackingColor", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "HotTrackingColor", string.Empty).ToString() != "80 80 80")
                interfaceV.TglButton1.StateNA = true;
            else
                interfaceV.TglButton1.StateNA = false;


            if (Registry.GetValue(@"HKEY_USERS\.DEFAULT\Control Panel\Colors", "InfoWindow", null) == null ||
                Registry.GetValue(@"HKEY_USERS\.DEFAULT\Control Panel\Colors", "InfoWindow", string.Empty).ToString() != "240 255 255" ||
                Registry.GetValue(@"HKEY_USERS\S-1-5-19\Control Panel\Colors", "InfoWindow", null) == null ||
                Registry.GetValue(@"HKEY_USERS\S-1-5-19\Control Panel\Colors", "InfoWindow", string.Empty).ToString() != "240 255 255" ||
                Registry.GetValue(@"HKEY_USERS\S-1-5-20\Control Panel\Colors", "InfoWindow", null) == null ||
                Registry.GetValue(@"HKEY_USERS\S-1-5-20\Control Panel\Colors", "InfoWindow", string.Empty).ToString() != "240 255 255")
                interfaceV.TglButton2.StateNA = true;
            else
                interfaceV.TglButton2.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionHeight", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionHeight", string.Empty).ToString() != "-270" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionWidth", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionWidth", string.Empty).ToString() != "-270")
                interfaceV.TglButton3.StateNA = true;
            else
                interfaceV.TglButton3.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "CursorBlinkRate", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "CursorBlinkRate", string.Empty).ToString() != "250")
                interfaceV.TglButton4.StateNA = true;   
            else
                interfaceV.TglButton4.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollHeight", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollHeight", string.Empty).ToString() != "-210" ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollWidth", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollWidth", string.Empty).ToString() != "-210")
                interfaceV.TglButton5.StateNA = true;
            else
                interfaceV.TglButton5.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", string.Empty).ToString() != "0")
                interfaceV.TglButton6.StateNA = true;
            else
                interfaceV.TglButton6.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", string.Empty).ToString() != "1")
                interfaceV.TglButton7.StateNA = true;
            else
                interfaceV.TglButton7.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", string.Empty).ToString() != "1")
                interfaceV.TglButton8.StateNA = true;
            else
                interfaceV.TglButton8.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", string.Empty).ToString() != "20")
                interfaceV.TglButton9.StateNA = true;
            else
                interfaceV.TglButton9.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseHoverTime", null) == null ||
               Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseHoverTime", string.Empty).ToString() != "20")
                interfaceV.TglButton10.StateNA = true;
            else
                interfaceV.TglButton10.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", string.Empty).ToString() != @"%systemroot%\\Blank.ico,0")
                interfaceV.TglButton11.StateNA = true;
            else
                interfaceV.TglButton11.StateNA = false;


            if (Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer")?.GetValue("link", null) == null ||
                string.Concat((byte[])Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer")?.GetValue("link", string.Empty) ?? Array.Empty<byte>()) != "0000")
                interfaceV.TglButton12.StateNA = true;
            else
                interfaceV.TglButton12.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", null) != null &&
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", string.Empty).ToString() == "0")
                interfaceV.TglButton13.StateNA = true;
            else
                interfaceV.TglButton13.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", string.Empty).ToString() != "1")
                interfaceV.TglButton14.StateNA = true;
            else
                interfaceV.TglButton14.StateNA = false;


            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", string.Empty).ToString() != "0")
                    interfaceV.TglButton15.StateNA = true;
                else
                    interfaceV.TglButton15.StateNA = false;
            }


            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", string.Empty).ToString() != "0")
                    interfaceV.TglButton16.StateNA = true;
                else
                    interfaceV.TglButton16.StateNA = false;
            }


            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32") == null)
                    interfaceV.TglButton17.StateNA = true;
                else
                    interfaceV.TglButton17.StateNA = false;
            }

            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", string.Empty).ToString() != "0")
                    interfaceV.TglButton18.StateNA = true;
                else
                    interfaceV.TglButton18.StateNA = false;
            }


            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", string.Empty).ToString() != "1")
                    interfaceV.TglButton19.StateNA = true;
                else
                    interfaceV.TglButton19.StateNA = false;
            }

            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", string.Empty).ToString() != "0")
                    interfaceV.TglButton20.StateNA = true;
                else
                    interfaceV.TglButton20.StateNA = false;
            }
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", string.Empty).ToString() != "0" ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", null) == null ||
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", string.Empty).ToString() != "0")
                    interfaceV.TglButton20.StateNA = true;
                else
                    interfaceV.TglButton20.StateNA = false;
            }

            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", null) != null &&
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", string.Empty).ToString() == "1")
                interfaceV.TglButton21.StateNA = true;
            else
                interfaceV.TglButton21.StateNA = false;

            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", null) == null || 
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", string.Empty).ToString() != "0" || 
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", null) == null || 
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", string.Empty).ToString() != "0" || 
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", null) == null || 
                    Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", string.Empty).ToString() != "0")
                    interfaceV.TglButton22.StateNA = true;
                else
                    interfaceV.TglButton22.StateNA = false;
            }
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
            {
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\\Explorer", "DisableNotificationCenter", null) == null ||
                     Registry.GetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\\Explorer", "DisableNotificationCenter", string.Empty).ToString() != "1" ||
                     Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\\PushNotifications", "ToastEnabled", null) == null ||
                     Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\\PushNotifications", "ToastEnabled", string.Empty).ToString() != "0")
                    interfaceV.TglButton22.StateNA = true;
                else
                    interfaceV.TglButton22.StateNA = false;
            }

            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}") != null)
                interfaceV.TglButton23.StateNA = true;
            else
                interfaceV.TglButton23.StateNA = false;


            if (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}") != null ||
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}") != null)
                interfaceV.TglButton24.StateNA = true;
            else
                interfaceV.TglButton24.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "JPEGImportQuality", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "JPEGImportQuality", string.Empty).ToString() != "100")
                interfaceV.TglButton25.StateNA = true;
            else
                interfaceV.TglButton25.StateNA = false;


            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", null) == null ||
                Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", string.Empty).ToString() != "0")
                interfaceV.TglButton26.StateNA = true;
            else
                interfaceV.TglButton26.StateNA = false;


            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", string.Empty).ToString() != "1")
                interfaceV.TglButton27.StateNA = true;
            else
                interfaceV.TglButton27.StateNA = false;

            if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", null) == null ||
                Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", string.Empty).ToString() != "1")
                interfaceV.TglButton28.StateNA = true;
            else
                interfaceV.TglButton28.StateNA = false;
        }

        private static void RestartExplorer(Process launchExplorer)
        {
            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (string.Compare(process.MainModule?.FileName, $"{Environment.GetEnvironmentVariable("WINDIR")}\\{"explorer.exe"}", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        process.Kill();
                        process.Start();
                    }
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                finally
                {
                    Process[] explorer = Process.GetProcessesByName("explorer");
                    if (explorer.Length == 0)
                    {
                        launchExplorer.StartInfo.FileName = $"{Environment.GetEnvironmentVariable("WINDIR")}\\{"explorer.exe"}";
                        launchExplorer.StartInfo.UseShellExecute = true;
                        launchExplorer.Start();
                    }
                       
                }
            }
        }

        internal static void UseInterface(string tweak, bool isChoose)
        {
            switch (tweak)
            {
                case "TglButton1":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "Hilight", "80 80 80", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "HotTrackingColor", "80 80 80", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "Hilight", "0 120 215", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Colors", "HotTrackingColor", "0 102 204", RegistryValueKind.String);
                    }
                    break;
                case "TglButton2":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.Users, @".DEFAULT\Control Panel\Colors", "InfoWindow", "240 255 255", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.Users, @"S-1-5-19\Control Panel\Colors", "InfoWindow", "240 255 255", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.Users, @"S-1-5-20\Control Panel\Colors", "InfoWindow", "240 255 255", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.Users, @".DEFAULT\Control Panel\Colors", "InfoWindow", "255 255 255", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.Users, @"S-1-5-19\Control Panel\Colors", "InfoWindow", "255 255 255", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.Users, @"S-1-5-20\Control Panel\Colors", "InfoWindow", "255 255 255", RegistryValueKind.String);
                    }
                    break;
                case "TglButton3":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionHeight", "-270", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionWidth", "-270", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionHeight", "-330", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionWidth", "-330", RegistryValueKind.String);
                    }
                    break;
                case "TglButton4":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "CursorBlinkRate", "250", RegistryValueKind.String);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "CursorBlinkRate", "530", RegistryValueKind.String);
                    break;
                case "TglButton5":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollHeight", "-210", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollWidth", "-210", RegistryValueKind.String);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollHeight", "-255", RegistryValueKind.String);
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollWidth", "-255", RegistryValueKind.String);
                    }
                    break;
                case "TglButton6":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton7":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, RegistryValueKind.DWord);
                    new Thread(() => RestartExplorer(new Process())).Start();
                    new Thread(() => RestartExplorer(new Process())).IsBackground = true;
                    break;
                case "TglButton8":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, RegistryValueKind.DWord);
                    new Thread(() => RestartExplorer(new Process())).Start();
                    new Thread(() => RestartExplorer(new Process())).IsBackground = true;
                    break;
                case "TglButton9":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", 20, RegistryValueKind.String);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", 400, RegistryValueKind.String);
                    break;
                case "TglButton10":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseHoverTime", "20", RegistryValueKind.String);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseHoverTime", "400", RegistryValueKind.String);
                    break;
                case "TglButton11":
                    Environment.SpecialFolder _folderWindows = Environment.SpecialFolder.Windows;
                    string _pathToWinF = Environment.GetFolderPath(_folderWindows);
                    try
                    {
                        if (isChoose)
                        {
                            byte[] _iconByte = default;
                            using (MemoryStream fileOut = new MemoryStream(Properties.Resources.Blank))
                            using (GZipStream gz = new GZipStream(fileOut, CompressionMode.Decompress))
                            using (MemoryStream ms = new MemoryStream())
                            {
                                gz.CopyTo(ms);
                                _iconByte = ms.ToArray();
                            }
                            File.WriteAllBytes(_pathToWinF + @"\Blank.ico", _iconByte);

                            RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0", RegistryValueKind.String);
                        }
                        else
                        {
                            File.Delete(_pathToWinF + @"\Blank.ico");
                            RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); };
                    break;
                case "TglButton12":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link", Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link");
                    break;
                case "TglButton13":
                    if (isChoose)
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}"); 
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 0, RegistryValueKind.DWord);
                    break;
                case "TglButton14":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", 0, RegistryValueKind.DWord);
                    break;
                case "TglButton15":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton16":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton17":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
                    else
                        RegistryHelp.DeleteFolderTree(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");

                    new Thread(() => RestartExplorer(new Process())).Start();
                    new Thread(() => RestartExplorer(new Process())).IsBackground = true;
                    break;
                case "TglButton18":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton19":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", 0, RegistryValueKind.DWord);
                    break;
                case "TglButton20":
                    if (isChoose)
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);

                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0, RegistryValueKind.DWord);
                        }

                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0, RegistryValueKind.DWord); 
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0, RegistryValueKind.DWord);
                        }
                    
                    }
                    else
                    {
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 1, RegistryValueKind.DWord);
                        RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 1, RegistryValueKind.DWord);

                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", 1, RegistryValueKind.DWord);
                        }

                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications");
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1, RegistryValueKind.DWord);
                        }
                    }
                    break;
                case "TglButton21":
                    if (isChoose)
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers");
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton22":
                    if (isChoose)
                    {
                        if(SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, RegistryValueKind.DWord);
                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, RegistryValueKind.DWord);
                        }
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("10"))
                            RegistryHelp.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");
                        if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("11"))
                        {
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 1, RegistryValueKind.DWord);
                            RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 1, RegistryValueKind.DWord);
                        }
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 1, RegistryValueKind.DWord);
                    }
                    break;
                case "TglButton23":
                    if (isChoose)
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
                    string _path = Environment.ExpandEnvironmentVariables(@"%userprofile%\3D Objects");
                    if (isChoose)
                    {
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        Directory.Delete(_path, true);
                    }
                    else
                    {
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        RegistryHelp.CreateFolder(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        Directory.CreateDirectory(_path);
                    }
                    break;
                case "TglButton25":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality");
                    break;
                case "TglButton26":
                    if (isChoose)
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", 0, RegistryValueKind.DWord);
                    else
                        RegistryHelp.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", 1, RegistryValueKind.DWord);
                    break;
                case "TglButton27":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer");
                    break;
                case "TglButton28":
                    if (isChoose)
                        RegistryHelp.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", 1, RegistryValueKind.DWord);
                    else
                        RegistryHelp.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode");
                    break;

            }
        }


    }
}
