using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using GTweak.Modules.Configuration;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using Microsoft.Win32;

namespace GTweak.Modules.Tweaks
{
    internal enum InterfaceColor
    {
        CursorSelection = 1,
        Tooltip
    }

    internal enum InterfaceCheckbox
    {
        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        ExplorerHome = 1,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        ExplorerGallery,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        ExplorerOneDrive,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        HiddenFiles,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        SystemFiles,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FileExtensions,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        EmptyDrives,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconComputer,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconNetwork,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconRecycleBin,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconControlPanel,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconUserFiles,

        [PostAction(shell: ExplorerManager.ShellType.Refresh)]
        IconOneDrive,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderObjects3D,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderDesktop,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderDownloads,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderDocuments,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderPictures,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderMusic,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        FolderVideo
    }

    internal enum InterfaceToggle
    {
        [PostAction(NotificationManager.AlertType.Logout)]
        SystemButtonSize = 1,

        [PostAction(NotificationManager.AlertType.Logout)]
        CursorFlickerFrequency,

        [PostAction(NotificationManager.AlertType.Logout)]
        ScrollbarSize,

        TaskbarTransparency,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        TaskbarDarkTheme,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        AppDarkTheme,

        [PostAction(NotificationManager.AlertType.Logout)]
        ContextMenuDelay,

        [PostAction(NotificationManager.AlertType.Logout)]
        TaskbarPreviewDelay,

        [PostAction(NotificationManager.AlertType.Logout)]
        IconOverlayBadges,

        [PostAction(NotificationManager.AlertType.Logout)]
        ShortcutNameSuffix,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        TaskbarAlignment,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        StartMenuLayout,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        CompactContextMenu,

        TipsSuggestions,

        CompactExplorerMode,

        [PostAction(NotificationManager.AlertType.Restart)]
        CopilotRecall,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        TaskbarEndTask,

        [PostAction(NotificationManager.AlertType.Logout)]
        SnapLayouts,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        TaskbarIconsButtons,

        PersonalizedAds,

        RestoreFolderWindows,

        TipsNotifications,

        JpegWallpaperCompression,

        DesktopTooltips,

        [PostAction(shell: ExplorerManager.ShellType.Restart)]
        BingStartSearch,

        OpenQuickAccess,

        [PostAction(NotificationManager.AlertType.Restart)]
        AdaptiveBrightness
    }

    internal sealed class InterfaceTweaks
    {
        internal readonly static Dictionary<string, object> ControlStates = new Dictionary<string, object>();
        private readonly ControlWriterManager _сontrolWriter = new ControlWriterManager(ControlStates);
        private readonly Dictionary<InterfaceColor, (Func<string> Check, Action<string> Apply)> _colorTweaks;
        private readonly Dictionary<InterfaceCheckbox, (Func<bool> Check, Action<bool> Apply)> _cbTweaks;
        private readonly Dictionary<InterfaceToggle, (Func<bool> Check, Action<bool> Apply)> _tglTweaks;

        public InterfaceTweaks()
        {
            _colorTweaks = new Dictionary<InterfaceColor, (Func<string> Check, Action<string> Apply)>
            {
                [InterfaceColor.CursorSelection] = (
                    Check: () => RegistryHelper.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "Hilight", "0 120 215"),
                    Apply: (value) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Colors", "Hilight", value, RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Colors", "HotTrackingColor", value == "0 120 215" ? "0 102 204" : value, RegistryValueKind.String);
                    }
                ),

                [InterfaceColor.Tooltip] = (
                    Check: () => RegistryHelper.GetValue(@"HKEY_CURRENT_USER\Control Panel\Colors", "InfoWindow", "255 255 225"),
                    Apply: (value) =>
                    {
                        RegistryHelper.Write(Registry.Users, @".DEFAULT\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                        RegistryHelper.Write(Registry.Users, @"S-1-5-19\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                        RegistryHelper.Write(Registry.Users, @"S-1-5-20\Control Panel\Colors", "InfoWindow", value, RegistryValueKind.String);
                    }
                )
            };

            _cbTweaks = new Dictionary<InterfaceCheckbox, (Func<bool> Check, Action<bool> Apply)>
            {
                [InterfaceCheckbox.ExplorerHome] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        ((HardwareData.OS.Build > 22621m && (RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}"))) ||
                        (HardwareData.OS.Build <= 22621m && (RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace_36354489\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace_36354489\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}")))) ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}", "System.IsPinnedToNameSpaceTree", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}\ShellFolder", "Attributes", unchecked((int)0xB0940064).ToString());
                    },
                    Apply: (state) =>
                    {
                        string baseKey = HardwareData.OS.Build > 22621m ? @"NameSpace" : @"NameSpace_36354489";
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, $@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}");
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}", "System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}\ShellFolder", "Attributes", unchecked((int)0xB0940064), RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}", string.Empty, @"CLSID_MSGraphHomeFolder", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, $@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\{baseKey}\{{f874310e-b6b7-47dc-bc84-b9e6b38f5903}}", string.Empty, @"CLSID_MSGraphHomeFolder", RegistryValueKind.String);
                            RegistryHelper.DeleteFolderTree(Registry.CurrentUser, @"Software\Classes\CLSID\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}");
                        }
                    }
                ),

                [InterfaceCheckbox.ExplorerGallery] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", string.Empty, @"{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", string.Empty, @"{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}", RegistryValueKind.String);
                        }
                    }
                ),

                [InterfaceCheckbox.ExplorerOneDrive] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CLASSES_ROOT\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CLASSES_ROOT\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", "0");
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.ClassesRoot, @"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", state ? 0 : 1, RegistryValueKind.String);
                        RegistryHelper.Write(Registry.ClassesRoot, @"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "System.IsPinnedToNameSpaceTree", state ? 0 : 1, RegistryValueKind.String);
                    }
                ),

                [InterfaceCheckbox.HiddenFiles] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", "1", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.SystemFiles] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden", "1", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.FileExtensions] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.EmptyDrives] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideDrivesWithNoMedia", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconComputer] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{20D04FE0-3AEA-1069-A2D8-08002B30309D}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconNetwork] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconRecycleBin] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", "1", false),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{645FF040-5081-101B-9F08-00AA002F954E}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconControlPanel] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{5399E694-6CE5-4D6C-8FCE-1D8870FDCBA0}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconUserFiles] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{59031A47-3F72-44A7-89C5-5595FE6B30EE}", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{59031A47-3F72-44A7-89C5-5595FE6B30EE}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.IconOneDrive] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{018D5C66-4533-4307-9B53-224DE2ED1FE6}", "0", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{018D5C66-4533-4307-9B53-224DE2ED1FE6}", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceCheckbox.FolderObjects3D] = (
                    Check: () =>
                    {
                        return RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}") ||
                        RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                        }

                        CommandExecutor.RunCommand(state ? @"/c rd /s /q ""%userprofile%\3D Objects""" : @"/c mkdir ""%userprofile%\3D Objects""");
                    }
                ),

                [InterfaceCheckbox.FolderDesktop] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}");
                        }
                    }
                ),

                [InterfaceCheckbox.FolderDownloads] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{088e3905-0323-4b02-9826-5d99428e115f}");
                        }
                    }
                ),

                [InterfaceCheckbox.FolderDocuments] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{d3162b92-9365-467a-956b-92703aca08af}");
                        }
                    }
                ),

                [InterfaceCheckbox.FolderPictures] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{24ad3ad4-a569-4530-98e1-ab02f9417aa8}");
                        }
                    }
                ),

                [InterfaceCheckbox.FolderMusic] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}");
                        }
                    }
                ),

                [InterfaceCheckbox.FolderVideo] = (
                    Check: () => RegistryHelper.KeyExists(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");
                        }
                        else
                        {
                            RegistryHelper.CreateFolder(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}");
                        }
                    }
                )
            };

            _tglTweaks = new Dictionary<InterfaceToggle, (Func<bool> Check, Action<bool> Apply)>
            {
                [InterfaceToggle.SystemButtonSize] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionHeight", "-270") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "CaptionWidth", "-270");
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionHeight", state ? "-330" : "-270", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "CaptionWidth", state ? "-330" : "-270", RegistryValueKind.String);
                    }
                ),

                [InterfaceToggle.CursorFlickerFrequency] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "CursorBlinkRate", "530", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop", "CursorBlinkRate", state ? "530" : "200", RegistryValueKind.String)
                ),

                [InterfaceToggle.ScrollbarSize] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollHeight", "-210") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "ScrollWidth", "-210");
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollHeight", state ? "-255" : "-210", RegistryValueKind.String);
                        RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "ScrollWidth", state ? "-255" : "-210", RegistryValueKind.String);
                    }
                ),

                [InterfaceToggle.TaskbarTransparency] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", "0"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.TaskbarDarkTheme] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", "1"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.AppDarkTheme] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", "1"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", state ? 0 : 1, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.ContextMenuDelay] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", "20"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop", "MenuShowDelay", state ? "400" : "20", RegistryValueKind.String)
                ),

                [InterfaceToggle.TaskbarPreviewDelay] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseHoverTime", "20"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Mouse", "MouseHoverTime", state ? "400" : "20", RegistryValueKind.String)
                ),

                [InterfaceToggle.IconOverlayBadges] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTypeOverlay", "0");
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTypeOverlay", state ? 1 : 0, RegistryValueKind.DWord);

                        try
                        {
                            Task.Run(delegate
                            {
                                if (state)
                                {
                                    FileDirectoryHelper.DeleteFile(PathTargets.Files.BlankIcon);

                                    RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
                                }
                                else
                                {
                                    ArchiveManager.Unarchive(PathTargets.Files.BlankIcon, Properties.Resources.Blank);
                                    RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", @"%systemroot%\\Blank.ico,0", RegistryValueKind.String);
                                }
                            });
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                    }
                ),

                [InterfaceToggle.ShortcutNameSuffix] = (
                    Check: () => RegistryHelper.CheckValueBytes(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer", "link", "0000"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer", "link", Encoding.Unicode.GetBytes("\0\0"), RegistryValueKind.Binary);
                        }
                    }
                ),

                [InterfaceToggle.TaskbarAlignment] = (
                    Check: () => HardwareData.OS.IsWin11 && RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", "0"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.StartMenuLayout] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", "1") ||
                        HardwareData.OS.Name.IndexOf("home", StringComparison.OrdinalIgnoreCase) < 0 &&
                        (RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", "1")));
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_Layout", state ? 0 : 1, RegistryValueKind.DWord);

                        if (HardwareData.OS.Name.IndexOf("Home", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            if (state)
                            {
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Start");
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Education");
                                RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection");
                            }
                            else
                            {
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Start", "HideRecommendedSection", 1, RegistryValueKind.DWord);
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\current\device\Education", "IsEducationEnvironment", 1, RegistryValueKind.DWord);
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                ),

                [InterfaceToggle.CompactContextMenu] = (
                    Check: () => HardwareData.OS.IsWin11 && RegistryHelper.KeyExists(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", true),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", RegistryValueKind.String);
                        }
                    }
                ),

                [InterfaceToggle.TipsSuggestions] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", "0"));
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                    }
                ),

                [InterfaceToggle.CompactExplorerMode] = (
                    Check: () => HardwareData.OS.IsWin11 && RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", "1", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "UseCompactMode", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.CopilotRecall] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        (RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", "1"));
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Services\WSAIFabricSvc", "Start", state ? 2 : 4, RegistryValueKind.DWord);
                        if (state)
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "CopilotPWAPin", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "RecallPin", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarCompanion", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\3389499533", "EnabledState", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\4027803789", "EnabledState", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\450471565", "EnabledState", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\2283032206", "EnabledState", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\502943886", "EnabledState", 0, RegistryValueKind.DWord);
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\generativeAI");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\systemAIModels");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors");
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 0, RegistryValueKind.DWord);
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsCopilot", "AllowCopilotRuntime", 1, RegistryValueKind.DWord);
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeEntraCopilotPageContext");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "Microsoft365CopilotChatIconEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeHistoryAISearchEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "GenAILocalFoundationalModelSettings");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BuiltInAIAPIsEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "AIGenThemesEnabled");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DevToolsGenAiSettings");
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ShareBrowsingHistoryWithCopilotSearchAllowed");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint");
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\WindowsNotepad");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "CopilotPWAPin", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "RecallPin", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarCompanion", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\3389499533", "EnabledState", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\4027803789", "EnabledState", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\450471565", "EnabledState", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\2283032206", "EnabledState", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\ControlSet001\Control\FeatureManagement\Overrides\8\502943886", "EnabledState", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\generativeAI", "Value", "Deny", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\systemAIModels", "Value", "Deny", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "TurnOffSavingSnapshots", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableSettingsAgent", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentConnectors", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableAgentWorkspaces", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"Software\Policies\Microsoft\Windows\WindowsAI", "DisableRemoteAgentConnectors", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\WindowsAI\DisableImageCreator", "value", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsCopilot", "AllowCopilotRuntime", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot\BingChat", "IsUserEligible", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "IsCopilotAvailable", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\Shell\Copilot", "CopilotDisabledReason", "FeatureIsDisabled", RegistryValueKind.String);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DefaultBrowserSettingsCampaignEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeEntraCopilotPageContext", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "Microsoft365CopilotChatIconEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeHistoryAISearchEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "GenAILocalFoundationalModelSettings", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BuiltInAIAPIsEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "AIGenThemesEnabled", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "DevToolsGenAiSettings", 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "ShareBrowsingHistoryWithCopilotSearchAllowed", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableImageCreator", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableCocreator", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeFill", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeErase", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableRemoveBackground", 1, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\WindowsNotepad", "DisableAIFeatures", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.TaskbarEndTask] = (
                    Check: () => HardwareData.OS.IsWin11 && RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", "1", true),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", state ? 1 : 0, RegistryValueKind.DWord)
                ),


                [InterfaceToggle.SnapLayouts] = (
                    Check: () => HardwareData.OS.IsWin11 && RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", "0"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.TaskbarIconsButtons] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0")) ||
                        HardwareData.OS.IsWin10 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", "0"));
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", state ? 1 : 0, RegistryValueKind.DWord);

                        if (HardwareData.OS.IsWin11)
                        {
                            CommandExecutor.RunCommandAsTrustedInstaller(@"/c del /q /f ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*Copilot*.lnk""");
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", state ? 1 : 0, RegistryValueKind.DWord);

                            if (state || HardwareData.OS.Build.CompareTo(22635.3785m) < 0)
                            {
                                RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", "", "Windows Spotlight", RegistryValueKind.String);
                                RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}");
                            }
                            else
                            {
                                RegistryHelper.DeleteFolderTree(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{2cc5ca98-6485-489a-920e-b3e88a6ccce3}");
                                RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel", "{2cc5ca98-6485-489a-920e-b3e88a6ccce3}", 1, RegistryValueKind.DWord);
                            }
                        }
                        else if (HardwareData.OS.IsWin10)
                        {
                            if (state)
                            {
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh");
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds");
                                RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\NewsAndInterests\AllowNewsAndInterests");
                            }
                            else
                            {
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0, RegistryValueKind.DWord);
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0, RegistryValueKind.DWord);
                                RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\PolicyManager\default\NewsAndInterests\AllowNewsAndInterests", "value", 0, RegistryValueKind.DWord);
                            }
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", state ? 0 : 2, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCortanaButton", state ? 1 : 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.PersonalizedAds] = (
                    Check: () =>
                    {
                        return HardwareData.OS.IsWin11 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", "0")) ||
                        HardwareData.OS.IsWin10 &&
                        (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0"));
                    },
                    Apply: (state) =>
                    {
                        RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", state ? 1 : 0, RegistryValueKind.DWord);

                        if (HardwareData.OS.IsWin10)
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338393Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        }
                        else if (HardwareData.OS.IsWin11)
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.RestoreFolderWindows] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", "0", true),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers", 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "PersistBrowsers");
                        }
                    }
                ),

                [InterfaceToggle.TipsNotifications] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", "0") ||
                        (HardwareData.OS.IsWin11 && (RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", "0") ||
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", "0"))) ||
                        (HardwareData.OS.IsWin10 && RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", "1"));
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips");
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "AllowOnlineTips", 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\PushNotifications", "ToastEnabled", 0, RegistryValueKind.DWord);
                        }

                        if (HardwareData.OS.IsWin10)
                        {
                            if (state)
                            {
                                RegistryHelper.DeleteValue(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter");
                            }
                            else
                            {
                                RegistryHelper.Write(Registry.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1, RegistryValueKind.DWord);
                            }
                        }
                        else if (HardwareData.OS.IsWin11)
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                            RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", state ? 1 : 0, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.JpegWallpaperCompression] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "JPEGImportQuality", "100"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\Desktop", "JPEGImportQuality", 100, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.DesktopTooltips] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", "0"),
                    Apply: (state) => RegistryHelper.Write(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowInfoTip", state ? 1 : 0, RegistryValueKind.DWord)
                ),

                [InterfaceToggle.BingStartSearch] = (
                    Check: () => RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", "1"),
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteFolderTree(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer");
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.OpenQuickAccess] = (
                    Check: () =>
                    {
                        return HardwareData.OS.Build >= 22621m ? RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", "1") :
                        RegistryHelper.CheckValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", "1");
                    },
                    Apply: (state) =>
                    {
                        if (HardwareData.OS.Build >= 22621m)
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", state ? 0 : 1, RegistryValueKind.DWord);
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", state ? 2 : 1, RegistryValueKind.DWord);
                        }
                    }
                ),

                [InterfaceToggle.AdaptiveBrightness] = (
                    Check: () =>
                    {
                        return RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC", "1") ||
                        (HardwareData.OS.Build >= 22000m && RegistryHelper.CheckValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption", "0"));
                    },
                    Apply: (state) =>
                    {
                        if (state)
                        {
                            RegistryHelper.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC");
                            if (HardwareData.OS.Build >= 22000m)
                            {
                                RegistryHelper.DeleteValue(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption");
                            }
                        }
                        else
                        {
                            RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "DisableCABC", 1, RegistryValueKind.DWord);
                            if (HardwareData.OS.Build >= 22000m)
                            {
                                RegistryHelper.Write(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "CABCOption", 0, RegistryValueKind.DWord);
                            }
                        }
                    }
                ),
            };
        }

        internal void CheckAll()
        {
            foreach (var tweak in _colorTweaks)
            {
                _сontrolWriter.ColorPicker[(int)tweak.Key] = tweak.Value.Check();
            }

            foreach (var tweak in _cbTweaks)
            {
                _сontrolWriter.Checkbox[(int)tweak.Key] = tweak.Value.Check();
            }

            foreach (var tweak in _tglTweaks)
            {
                _сontrolWriter.ToggleButton[(int)tweak.Key] = tweak.Value.Check();
            }
        }

        internal void Apply(string controlName, string value)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, controlName, value);

            if (controlName.StartsWith("ColorPicker") && int.TryParse(controlName.Substring(11), out int index))
            {
                if (_colorTweaks.TryGetValue((InterfaceColor)index, out var action))
                {
                    Task.Run(() => action.Apply(value));
                }
            }
        }

        internal void Apply(string controlName, bool state)
        {
            INIManager.TempWrite(INIManager.TempTweaksIntf, controlName, state);

            if (controlName.StartsWith("TglButton") && int.TryParse(controlName.Substring(9), out int tglIndex))
            {
                if (_tglTweaks.TryGetValue((InterfaceToggle)tglIndex, out var action))
                {
                    Task.Run(() => action.Apply(state));
                }
            }
            else if (controlName.StartsWith("Checkbox") && int.TryParse(controlName.Substring(8), out int cbIndex))
            {
                if (_cbTweaks.TryGetValue((InterfaceCheckbox)cbIndex, out var action))
                {
                    Task.Run(() => action.Apply(state));
                }
            }
        }
    }
}
