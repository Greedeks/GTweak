using System;
using System.Collections.Generic;
using GTweak.Modules.Managers;

namespace GTweak.Modules.Storage
{
    internal sealed class PackagesInfo
    {
        internal string Alias { get; }
        internal bool IsUnavailable { get; set; }
        internal IReadOnlyList<string> Scripts { get; }
        internal ExplorerManager.ShellType ShellType { get; }

        internal PackagesInfo(string alias = null, IReadOnlyList<string> scripts = null, ExplorerManager.ShellType shellType = ExplorerManager.ShellType.None)
        {
            Alias = alias;
            Scripts = scripts;
            ShellType = shellType;
        }
    }

    internal static class PackageStorage
    {
        internal static readonly Dictionary<string, PackagesInfo> PackagesDetails = new Dictionary<string, PackagesInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["OneDrive"] = new PackagesInfo(),
            ["Alarms"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsAlarms" }),
            ["BingFinance"] = new PackagesInfo(scripts: new[] { "Microsoft.BingFinance" }),
            ["BingNews"] = new PackagesInfo(scripts: new[] { "Microsoft.BingNews" }),
            ["BingSearch"] = new PackagesInfo(scripts: new[] { "Microsoft.BingSearch" }),
            ["BingSports"] = new PackagesInfo(scripts: new[] { "Microsoft.BingSports" }),
            ["BingWeather"] = new PackagesInfo("MSWeather", new[] { "Microsoft.BingWeather" }),
            ["Builder3D"] = new PackagesInfo("3D Builder", new[] { "Microsoft.3DBuilder" }),
            ["Calculator"] = new PackagesInfo("Calculator", new[] { "Microsoft.WindowsCalculator" }),
            ["Camera"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsCamera" }),
            ["ClipChamp"] = new PackagesInfo("Clipchamp Video Editor", new[] { "Clipchamp.Clipchamp" }),
            ["Copilot"] = new PackagesInfo("M365Copilot", new[] { "Microsoft.Copilot" }),
            ["Cortana"] = new PackagesInfo(scripts: new[] { "Microsoft.549981C3F5F10" }),
            ["DevHome"] = new PackagesInfo(scripts: new[] { "Microsoft.Windows.DevHome" }),
            ["Disney"] = new PackagesInfo("Disney", new[] { "Disney.37853FC22B2CE" }),
            ["DolbyAccess"] = new PackagesInfo("DolbyAccess", new[] { "DolbyLaboratories.DolbyAccess" }),
            ["Edge"] = new PackagesInfo("MicrosoftEdge", new[] { "Microsoft.MicrosoftEdge.Stable", "Microsoft.MicrosoftEdgeDevToolsClient", "Microsoft.Copilot" }, ExplorerManager.ShellType.Restart),
            ["Facebook"] = new PackagesInfo("Facebook", new[] { "Facebook.Facebook" }),
            ["FeedbackHub"] = new PackagesInfo("feedback", new[] { "Microsoft.WindowsFeedbackHub" }),
            ["GetHelp"] = new PackagesInfo(scripts: new[] { "Microsoft.GetHelp" }),
            ["GetStarted"] = new PackagesInfo(scripts: new[] { "Microsoft.Getstarted" }),
            ["Hulu"] = new PackagesInfo("Hulu.Hulu", new[] { "HULULLC.HULUPLUS", "HuluLLC.HuluPlus" }),
            ["iHeartRadio"] = new PackagesInfo("iHeart", new[] { "iHeartRadio" }),
            ["Instagram"] = new PackagesInfo("Instagram", new[] { "Facebook.InstagramBeta" }),
            ["LinkedIn"] = new PackagesInfo("LinkedInforWindows", new[] { "Microsoft.LinkedIn", "7EE7776C.LinkedInforWindows" }),
            ["Mail"] = new PackagesInfo("communicationsapps", new[] { "microsoft.windowscommunicationsapps" }),
            ["Maps"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsMaps" }),
            ["Messaging"] = new PackagesInfo(scripts: new[] { "Microsoft.Messaging" }),
            ["Microsoft3D"] = new PackagesInfo("3DViewer", new[] { "Microsoft.Microsoft3DViewer" }),
            ["MicrosoftFamily"] = new PackagesInfo("FamilySafety", new[] { "MicrosoftCorporationII.MicrosoftFamily" }),
            ["MicrosoftOfficeHub"] = new PackagesInfo("officehub", new[] { "Microsoft.MicrosoftOfficeHub" }),
            ["MicrosoftSolitaireCollection"] = new PackagesInfo("solitaire", new[] { "Microsoft.MicrosoftSolitaireCollection", "Microsoft.SolitaireCollection" }),
            ["MicrosoftStickyNotes"] = new PackagesInfo("MSStickyNotes", new[] { "Microsoft.MicrosoftStickyNotes" }),
            ["MicrosoftStore"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsStore" }),
            ["MicrosoftSway"] = new PackagesInfo("Sway", new[] { "Microsoft.Office.Sway" }),
            ["MicrosoftTeams"] = new PackagesInfo("Teams", new[] { "MicrosoftTeams", "MSTeams" }),
            ["MixedReality"] = new PackagesInfo("MixedRealityPortal", new[] { "Microsoft.MixedReality.Portal" }),
            ["Music"] = new PackagesInfo("zunemusic", new[] { "Microsoft.ZuneMusic", "Microsoft.GrooveMusic" }),
            ["Netflix"] = new PackagesInfo("Netflix", new[] { "4DF9E0F8.Netflix" }),
            ["Notepad"] = new PackagesInfo("Notepad", new[] { "Microsoft.WindowsNotepad" }),
            ["OneConnect"] = new PackagesInfo("MobilePlans", new[] { "Microsoft.OneConnect" }),
            ["OneNote"] = new PackagesInfo("MSOneNote", new[] { "Microsoft.Office.OneNote", "Microsoft.OneNote" }),
            ["Outlook"] = new PackagesInfo("Outlook", new[] { "Microsoft.OutlookForWindows", "Microsoft.Office.Outlook" }),
            ["Paint"] = new PackagesInfo(scripts: new[] { "Microsoft.Paint" }),
            ["Paint3D"] = new PackagesInfo(scripts: new[] { "Microsoft.MSPaint" }),
            ["Pandora"] = new PackagesInfo("PandoraMediaInc", new[] { "PandoraMediaInc.29680B314EFC2" }),
            ["People"] = new PackagesInfo(scripts: new[] { "Microsoft.People" }),
            ["Phone"] = new PackagesInfo("PhoneLink", new[] { "Microsoft.YourPhone", "MicrosoftWindows.CrossDevice" }),
            ["Photos"] = new PackagesInfo("MSPhotos", new[] { "Microsoft.Windows.Photos" }),
            ["Picsart"] = new PackagesInfo(scripts: new[] { "PicsArt-PhotoStudio" }),
            ["Plex"] = new PackagesInfo("Plex", new[] { "CAF9E577.Plex" }),
            ["PowerAutomateDesktop"] = new PackagesInfo(scripts: new[] { "Microsoft.PowerAutomateDesktop" }),
            ["PrimeVideo"] = new PackagesInfo("PrimeVideo", new[] { "AmazonVideo.PrimeVideo" }),
            ["QuickAssist"] = new PackagesInfo(scripts: new[] { "MicrosoftCorporationII.QuickAssist" }),
            ["ScreenSketch"] = new PackagesInfo(scripts: new[] { "Microsoft.ScreenSketch" }),
            ["Shazam"] = new PackagesInfo("Shazam", new[] { "ShazamEntertainmentLtd.Shazam" }),
            ["SkypeApp"] = new PackagesInfo("Skype", new[] { "Microsoft.SkypeApp" }),
            ["SoundRecorder"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsSoundRecorder" }),
            ["Spotify"] = new PackagesInfo("Spotify", new[] { "SpotifyAB.SpotifyMusic" }),
            ["TikTok"] = new PackagesInfo("TikTok", new[] { "BytedancePte.Ltd.TikTok" }),
            ["Todos"] = new PackagesInfo("TodoList", new[] { "Microsoft.Todos", "Microsoft.ToDo" }),
            ["TuneInRadio"] = new PackagesInfo("TuneInRadio", new[] { "TuneIn.TuneInRadio" }),
            ["Twitter"] = new PackagesInfo("Twitter", new[] { "9E2F88E3.Twitter" }),
            ["Viber"] = new PackagesInfo(scripts: new[] { "Viber" }),
            ["Video"] = new PackagesInfo("zunevideo", new[] { "Microsoft.ZuneVideo" }),
            ["Wallet"] = new PackagesInfo("MSPay", new[] { "Microsoft.Wallet" }),
            ["WebMediaExtensions"] = new PackagesInfo(scripts: new[] { "Microsoft.WebMediaExtensions" }),
            ["WhatsApp"] = new PackagesInfo("WhatsAppDesktop", new[] { "5319275A.WhatsAppDesktop" }),
            ["WhiteBoard"] = new PackagesInfo(scripts: new[] { "Microsoft.Whiteboard" }),
            ["Widgets"] = new PackagesInfo("Windows.Client.WebExperience", new[] { "MicrosoftWindows.Client.WebExperience", "Microsoft.WidgetsPlatformRuntime", "Microsoft.StartExperiencesApp" }, ExplorerManager.ShellType.Restart),
            ["WindowsTerminal"] = new PackagesInfo(scripts: new[] { "Microsoft.WindowsTerminal" }),
            ["Xbox"] = new PackagesInfo(scripts: new[] { "Microsoft.XboxApp", "Microsoft.GamingApp", "Microsoft.XboxGamingOverlay", "Microsoft.XboxGameOverlay", "Microsoft.XboxIdentityProvider", "Microsoft.Xbox.TCUI", "Microsoft.XboxSpeechToTextOverlay" }),
            ["YandexMusic"] = new PackagesInfo(scripts: new[] { "A025C540.Yandex.Music" }),
        };
    }
}
