using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GTweak.View
{
    public partial class ApplicationsView : UserControl
    {
        private TimeSpan time = TimeSpan.FromSeconds(0);
        private string applicationName = string.Empty;

        public ApplicationsView()
        {
            InitializeComponent();

            App.LanguageChanged += delegate { new TypewriterAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(0)); };

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time.TotalSeconds % 2 == 0)
                    UpdateViewStateApps();
                else if (time.TotalSeconds % 5 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += delegate { new UninstallingApps().ViewInstalledPackages(); };
                    backgroundWorker.RunWorkerAsync();
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);
        }

        private async void ClickApp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image appImage = (Image)sender;
            applicationName = appImage.Name;

            switch (e.LeftButton)
            {
                case MouseButtonState.Pressed when appImage.Source == (DrawingImage)FindResource("A_DI_" + applicationName):
                    {
                        appImage.Source = (DrawingImage)FindResource("DI_Sandtime");

                        BackgroundQueue backgroundQueue = new BackgroundQueue();
                        await backgroundQueue.QueueTask(delegate { UninstallingApps.DeletingPackage(applicationName); });

                        if (backgroundQueue.IsQueueCompleted())
                            UpdateViewStateApps();
                        break;
                    }

                case MouseButtonState.Pressed when appImage.Source == (DrawingImage)FindResource("DA_DI_" + applicationName) && applicationName == "OneDrive":
                    {
                        appImage.Source = (DrawingImage)FindResource("DI_Sandtime");
                        new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("onedrive_notification"));

                        BackgroundQueue backgroundQueue = new BackgroundQueue();
                        await backgroundQueue.QueueTask(delegate { UninstallingApps.ResetOneDrive(); });

                        if (backgroundQueue.IsQueueCompleted())
                            UpdateViewStateApps();
                        break;
                    }
            }
        }

        private void Apps_MouseEnter(object sender, MouseEventArgs e)
        {
            Image imageApp = (Image)sender;

            if (TextDescription.Text != (string)FindResource(imageApp.Name + "_applications"))
            {
                string _descriptionApps = (string)FindResource(imageApp.Name + "_applications");
                new TypewriterAnimation(_descriptionApps, TextDescription, TimeSpan.FromMilliseconds(250));
            }
        }

        private void Apps_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextDescription.Text != (string)FindResource("defaultDescriptionApp"))
                new TypewriterAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Parallel.Invoke(UpdateViewStateApps);
            new TypewriterAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(300));
        }

        private void UpdateViewStateApps()
        {
            MicrosoftStore.Source = !UninstallingApps.IsAppUnavailable["MicrosoftStore"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsStore") ? (DrawingImage)FindResource("A_DI_MicrosoftStore") : (DrawingImage)FindResource("DA_DI_MicrosoftStore") : (DrawingImage)FindResource("DI_Sandtime");
            Todos.Source = !UninstallingApps.IsAppUnavailable["Todos"] ? UninstallingApps.UserPackages.Contains("Microsoft.Todos") ? (DrawingImage)FindResource("A_DI_Todos") : (DrawingImage)FindResource("DA_DI_Todos") : (DrawingImage)FindResource("DI_Sandtime");
            BingWeather.Source = !UninstallingApps.IsAppUnavailable["BingWeather"] ? UninstallingApps.UserPackages.Contains("Microsoft.BingWeather") ? (DrawingImage)FindResource("A_DI_BingWeather") : (DrawingImage)FindResource("DA_DI_BingWeather") : (DrawingImage)FindResource("DI_Sandtime");
            Microsoft3D.Source = !UninstallingApps.IsAppUnavailable["Microsoft3D"] ? UninstallingApps.UserPackages.Contains("Microsoft.Microsoft3DViewer") ? (DrawingImage)FindResource("A_DI_Microsoft3D") : (DrawingImage)FindResource("DA_DI_Microsoft3D") : (DrawingImage)FindResource("DI_Sandtime");
            Music.Source = !UninstallingApps.IsAppUnavailable["Music"] ? UninstallingApps.UserPackages.Contains("Microsoft.ZuneMusic") ? (DrawingImage)FindResource("A_DI_Music") : (DrawingImage)FindResource("DA_DI_Music") : (DrawingImage)FindResource("DI_Sandtime");
            GetHelp.Source = !UninstallingApps.IsAppUnavailable["GetHelp"] ? UninstallingApps.UserPackages.Contains("Microsoft.GetHelp") ? (DrawingImage)FindResource("A_DI_GetHelp") : (DrawingImage)FindResource("DA_DI_GetHelp") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftOfficeHub.Source = !UninstallingApps.IsAppUnavailable["MicrosoftOfficeHub"] ? UninstallingApps.UserPackages.Contains("Microsoft.MicrosoftOfficeHub") ? (DrawingImage)FindResource("A_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DA_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftSolitaireCollection.Source = !UninstallingApps.IsAppUnavailable["MicrosoftSolitaireCollection"] ? UninstallingApps.UserPackages.Contains("Microsoft.MicrosoftSolitaireCollection") ? (DrawingImage)FindResource("A_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DA_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DI_Sandtime"); MixedReality.Source = !UninstallingApps.IsAppUnavailable["MixedReality"] ? UninstallingApps.UserPackages.Contains("Microsoft.MixedReality.Portal") ? (DrawingImage)FindResource("A_DI_MixedReality") : (DrawingImage)FindResource("DA_DI_MixedReality") : (DrawingImage)FindResource("DI_Sandtime");
            Xbox.Source = !UninstallingApps.IsAppUnavailable["Xbox"] ? UninstallingApps.UserPackages.Contains("Microsoft.XboxApp") || UninstallingApps.UserPackages.Contains("Microsoft.GamingApp") || UninstallingApps.UserPackages.Contains("Microsoft.XboxGamingOverlay") ||
               UninstallingApps.UserPackages.Contains("Microsoft.XboxGameOverlay") || UninstallingApps.UserPackages.Contains("Microsoft.XboxIdentityProvider") || UninstallingApps.UserPackages.Contains("Microsoft.Xbox.TCUI") ||
               UninstallingApps.UserPackages.Contains("Microsoft.XboxSpeechToTextOverlay") ? (DrawingImage)FindResource("A_DI_Xbox") : (DrawingImage)FindResource("DA_DI_Xbox") : (DrawingImage)FindResource("DI_Sandtime");
            Paint3D.Source = !UninstallingApps.IsAppUnavailable["Paint3D"] ? UninstallingApps.UserPackages.Contains("Microsoft.MSPaint") ? (DrawingImage)FindResource("A_DI_Paint3D") : (DrawingImage)FindResource("DA_DI_Paint3D") : (DrawingImage)FindResource("DI_Sandtime");
            OneNote.Source = !UninstallingApps.IsAppUnavailable["OneNote"] ? UninstallingApps.UserPackages.Contains("Microsoft.Office.OneNote") ? (DrawingImage)FindResource("A_DI_OneNote") : (DrawingImage)FindResource("DA_DI_OneNote") : (DrawingImage)FindResource("DI_Sandtime");
            People.Source = !UninstallingApps.IsAppUnavailable["People"] ? UninstallingApps.UserPackages.Contains("Microsoft.People") ? (DrawingImage)FindResource("A_DI_People") : (DrawingImage)FindResource("DA_DI_People") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftStickyNotes.Source = !UninstallingApps.IsAppUnavailable["MicrosoftStickyNotes"] ? UninstallingApps.UserPackages.Contains("Microsoft.MicrosoftStickyNotes") ? (DrawingImage)FindResource("A_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DA_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DI_Sandtime");
            Widgets.Source = !UninstallingApps.IsAppUnavailable["Widgets"] ? UninstallingApps.UserPackages.Contains("MicrosoftWindows.Client.WebExperience") ? (DrawingImage)FindResource("A_DI_Widgets") : (DrawingImage)FindResource("DA_DI_Widgets") : (DrawingImage)FindResource("DI_Sandtime");
            ScreenSketch.Source = !UninstallingApps.IsAppUnavailable["ScreenSketch"] ? UninstallingApps.UserPackages.Contains("Microsoft.ScreenSketch") ? (DrawingImage)FindResource("A_DI_ScreenSketch") : (DrawingImage)FindResource("DA_DI_ScreenSketch") : (DrawingImage)FindResource("DI_Sandtime");
            Phone.Source = !UninstallingApps.IsAppUnavailable["Phone"] ? UninstallingApps.UserPackages.Contains("Microsoft.YourPhone") ? (DrawingImage)FindResource("A_DI_Phone") : (DrawingImage)FindResource("DA_DI_Phone") : (DrawingImage)FindResource("DI_Sandtime");
            Photos.Source = !UninstallingApps.IsAppUnavailable["Photos"] ? UninstallingApps.UserPackages.Contains("Microsoft.Windows.Photos") ? (DrawingImage)FindResource("A_DI_Photos") : (DrawingImage)FindResource("DA_DI_Photos") : (DrawingImage)FindResource("DI_Sandtime");
            FeedbackHub.Source = !UninstallingApps.IsAppUnavailable["FeedbackHub"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsFeedbackHub") ? (DrawingImage)FindResource("A_DI_FeedbackHub") : (DrawingImage)FindResource("DA_DI_FeedbackHub") : (DrawingImage)FindResource("DI_Sandtime");
            SoundRecorder.Source = !UninstallingApps.IsAppUnavailable["SoundRecorder"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsSoundRecorder") ? (DrawingImage)FindResource("A_DI_SoundRecorder") : (DrawingImage)FindResource("DA_DI_SoundRecorder") : (DrawingImage)FindResource("DI_Sandtime");
            Alarms.Source = !UninstallingApps.IsAppUnavailable["Alarms"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsAlarms") ? (DrawingImage)FindResource("A_DI_Alarms") : (DrawingImage)FindResource("DA_DI_Alarms") : (DrawingImage)FindResource("DI_Sandtime");
            SkypeApp.Source = !UninstallingApps.IsAppUnavailable["SkypeApp"] ? UninstallingApps.UserPackages.Contains("Microsoft.SkypeApp") ? (DrawingImage)FindResource("A_DI_SkypeApp") : (DrawingImage)FindResource("DA_DI_SkypeApp") : (DrawingImage)FindResource("DI_Sandtime");
            Maps.Source = !UninstallingApps.IsAppUnavailable["Maps"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsMaps") ? (DrawingImage)FindResource("A_DI_Maps") : (DrawingImage)FindResource("DA_DI_Maps") : (DrawingImage)FindResource("DI_Sandtime");
            Camera.Source = !UninstallingApps.IsAppUnavailable["Camera"] ? UninstallingApps.UserPackages.Contains("Microsoft.WindowsCamera") ? (DrawingImage)FindResource("A_DI_Camera") : (DrawingImage)FindResource("DA_DI_Camera") : (DrawingImage)FindResource("DI_Sandtime");
            Video.Source = !UninstallingApps.IsAppUnavailable["Video"] ? UninstallingApps.UserPackages.Contains("Microsoft.ZuneVideo") ? (DrawingImage)FindResource("A_DI_Video") : (DrawingImage)FindResource("DA_DI_Video") : (DrawingImage)FindResource("DI_Sandtime");
            BingNews.Source = !UninstallingApps.IsAppUnavailable["BingNews"] ? UninstallingApps.UserPackages.Contains("Microsoft.BingNews") ? (DrawingImage)FindResource("A_DI_BingNews") : (DrawingImage)FindResource("DA_DI_BingNews") : (DrawingImage)FindResource("DI_Sandtime");
            Mail.Source = !UninstallingApps.IsAppUnavailable["Mail"] ? UninstallingApps.UserPackages.Contains("microsoft.windowscommunicationsapps") ? (DrawingImage)FindResource("A_DI_Mail") : (DrawingImage)FindResource("DA_DI_Mail") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftTeams.Source = !UninstallingApps.IsAppUnavailable["MicrosoftTeams"] ? UninstallingApps.UserPackages.Contains("MicrosoftTeams") || UninstallingApps.UserPackages.Contains("MSTeams") ? (DrawingImage)FindResource("A_DI_MicrosoftTeams") : (DrawingImage)FindResource("DA_DI_MicrosoftTeams") : (DrawingImage)FindResource("DI_Sandtime");
            PowerAutomateDesktop.Source = !UninstallingApps.IsAppUnavailable["PowerAutomateDesktop"] ? UninstallingApps.UserPackages.Contains("Microsoft.PowerAutomateDesktop") ? (DrawingImage)FindResource("A_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DA_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DI_Sandtime");
            Cortana.Source = !UninstallingApps.IsAppUnavailable["Cortana"] ? UninstallingApps.UserPackages.Contains("Microsoft.549981C3F5F10") ? (DrawingImage)FindResource("A_DI_Cortana") : (DrawingImage)FindResource("DA_DI_Cortana") : (DrawingImage)FindResource("DI_Sandtime");
            ClipChamp.Source = !UninstallingApps.IsAppUnavailable["ClipChamp"] ? UninstallingApps.UserPackages.Contains("Clipchamp.Clipchamp") ? (DrawingImage)FindResource("A_DI_ClipChamp") : (DrawingImage)FindResource("DA_DI_ClipChamp") : (DrawingImage)FindResource("DI_Sandtime");
            GetStarted.Source = !UninstallingApps.IsAppUnavailable["GetStarted"] ? UninstallingApps.UserPackages.Contains("Microsoft.Getstarted") ? (DrawingImage)FindResource("A_DI_GetStarted") : (DrawingImage)FindResource("DA_DI_GetStarted") : (DrawingImage)FindResource("DI_Sandtime");
            OneDrive.Source = !UninstallingApps.IsAppUnavailable["OneDrive"] ? UninstallingApps.IsOneDriveInstalled ? (DrawingImage)FindResource("A_DI_OneDrive") : (DrawingImage)FindResource("DA_DI_OneDrive") : (DrawingImage)FindResource("DI_Sandtime");
            BingSports.Source = !UninstallingApps.IsAppUnavailable["BingSports"] ? UninstallingApps.UserPackages.Contains("Microsoft.BingSports") ? (DrawingImage)FindResource("A_DI_BingSports") : (DrawingImage)FindResource("DA_DI_BingSports") : (DrawingImage)FindResource("DI_Sandtime");
            BingFinance.Source = !UninstallingApps.IsAppUnavailable["BingFinance"] ? UninstallingApps.UserPackages.Contains("Microsoft.BingFinance") ? (DrawingImage)FindResource("A_DI_BingFinance") : (DrawingImage)FindResource("DA_DI_BingFinance") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftFamily.Source = !UninstallingApps.IsAppUnavailable["MicrosoftFamily"] ? UninstallingApps.UserPackages.Contains("MicrosoftFamily") ? (DrawingImage)FindResource("A_DI_MicrosoftFamily") : (DrawingImage)FindResource("DA_DI_MicrosoftFamily") : (DrawingImage)FindResource("DI_Sandtime");
            BingSearch.Source = !UninstallingApps.IsAppUnavailable["BingSearch"] ? UninstallingApps.UserPackages.Contains("BingSearch") ? (DrawingImage)FindResource("A_DI_BingSearch") : (DrawingImage)FindResource("DA_DI_BingSearch") : (DrawingImage)FindResource("DI_Sandtime");
            Outlook.Source = !UninstallingApps.IsAppUnavailable["Outlook"] ? UninstallingApps.UserPackages.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_Outlook") : (DrawingImage)FindResource("DA_DI_Outlook") : (DrawingImage)FindResource("DI_Sandtime");
            QuickAssist.Source = !UninstallingApps.IsAppUnavailable["QuickAssist"] ? UninstallingApps.UserPackages.Contains("MicrosoftCorporationII.QuickAssist") ? (DrawingImage)FindResource("A_DI_QuickAssist") : (DrawingImage)FindResource("DA_DI_QuickAssist") : (DrawingImage)FindResource("DI_Sandtime");
            DevHome.Source = !UninstallingApps.IsAppUnavailable["DevHome"] ? UninstallingApps.UserPackages.Contains("DevHome") ? (DrawingImage)FindResource("A_DI_DevHome") : (DrawingImage)FindResource("DA_DI_DevHome") : (DrawingImage)FindResource("DI_Sandtime");
            WindowsTerminal.Source = !UninstallingApps.IsAppUnavailable["WindowsTerminal"] ? UninstallingApps.UserPackages.Contains("WindowsTerminal") ? (DrawingImage)FindResource("A_DI_WindowsTerminal") : (DrawingImage)FindResource("DA_DI_WindowsTerminal") : (DrawingImage)FindResource("DI_Sandtime");
        }
    }
}
