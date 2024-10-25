using GTweak.Utilities;
using GTweak.Utilities.Helpers;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Linq;
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
                    backgroundWorker.DoWork += delegate { new UninstallingApps().ViewInstalledApp(); };
                    backgroundWorker.RunWorkerAsync();
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);
        }

        private async void ClickApp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image appImage = (Image)sender;
            applicationName = appImage.Name;

            if (e.LeftButton == MouseButtonState.Pressed && appImage.Source == (DrawingImage)FindResource("A_DI_" + appImage.Name))
            {
                appImage.Source = (DrawingImage)FindResource("DI_Sandtime");

                BackgroundQueue backgroundQueue = new BackgroundQueue();
                await backgroundQueue.QueueTask(delegate
                {
                    UninstallingApps.IsAppDeletedList[applicationName] = true;
                    UninstallingApps.DeletedApp(applicationName);
                });
                if (backgroundQueue.IsQueueCompleted())
                {
                    await Task.Delay(35000);
                    UninstallingApps.IsAppDeletedList[appImage.Name] = false;
                    UpdateViewStateApps();
                }
            }

            if (e.LeftButton == MouseButtonState.Pressed && appImage.Source == (DrawingImage)FindResource("DA_DI_" + appImage.Name) && applicationName == "OneDrive")
            {
                appImage.Source = (DrawingImage)FindResource("DI_Sandtime");
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("onedrive_notification"));

                BackgroundQueue backgroundQueue = new BackgroundQueue();
                await backgroundQueue.QueueTask(delegate
                {
                    UninstallingApps.ResetOneDrive();
                });
                if (backgroundQueue.IsQueueCompleted())
                {
                    await Task.Delay(6000);
                    UninstallingApps.IsAppDeletedList[appImage.Name] = false;
                    UpdateViewStateApps();
                    UpdateViewStateApps();
                }
            }
        }

        private void BtnDelete_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (string key in UninstallingApps.IsAppDeletedList.Keys.ToList())
                {
                    UninstallingApps.IsAppDeletedList[key] = true;
                    UpdateViewStateApps();
                }

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += delegate
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("appsdelete_notification"));
                    Parallel.Invoke(UninstallingApps.DeletedAllApps);
                };
                backgroundWorker.RunWorkerCompleted += async delegate
                {
                    await Task.Delay(80000);
                    foreach (string key in UninstallingApps.IsAppDeletedList.Keys.ToList())
                    {
                        UninstallingApps.IsAppDeletedList[key] = false;
                        UpdateViewStateApps();
                    }
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
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
            MicrosoftStore.Source = !UninstallingApps.IsAppDeletedList["MicrosoftStore"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsStore") ? (DrawingImage)FindResource("A_DI_MicrosoftStore") : (DrawingImage)FindResource("DA_DI_MicrosoftStore") : (DrawingImage)FindResource("DI_Sandtime");
            Todos.Source = !UninstallingApps.IsAppDeletedList["Todos"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Todos") ? (DrawingImage)FindResource("A_DI_Todos") : (DrawingImage)FindResource("DA_DI_Todos") : (DrawingImage)FindResource("DI_Sandtime");
            BingWeather.Source = !UninstallingApps.IsAppDeletedList["BingWeather"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingWeather") ? (DrawingImage)FindResource("A_DI_BingWeather") : (DrawingImage)FindResource("DA_DI_BingWeather") : (DrawingImage)FindResource("DI_Sandtime");
            Microsoft3D.Source = !UninstallingApps.IsAppDeletedList["Microsoft3D"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Microsoft3DViewer") ? (DrawingImage)FindResource("A_DI_Microsoft3D") : (DrawingImage)FindResource("DA_DI_Microsoft3D") : (DrawingImage)FindResource("DI_Sandtime");
            Music.Source = !UninstallingApps.IsAppDeletedList["Music"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ZuneMusic") ? (DrawingImage)FindResource("A_DI_Music") : (DrawingImage)FindResource("DA_DI_Music") : (DrawingImage)FindResource("DI_Sandtime");
            GetHelp.Source = !UninstallingApps.IsAppDeletedList["GetHelp"] ? UninstallingApps.UserAppsList.Contains("Microsoft.GetHelp") ? (DrawingImage)FindResource("A_DI_GetHelp") : (DrawingImage)FindResource("DA_DI_GetHelp") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftOfficeHub.Source = !UninstallingApps.IsAppDeletedList["MicrosoftOfficeHub"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftOfficeHub") || UninstallingApps.UserAppsList.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DA_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftSolitaireCollection.Source = !UninstallingApps.IsAppDeletedList["MicrosoftSolitaireCollection"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftSolitaireCollection") ? (DrawingImage)FindResource("A_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DA_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DI_Sandtime"); MixedReality.Source = !UninstallingApps.IsAppDeletedList["MixedReality"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MixedReality.Portal") ? (DrawingImage)FindResource("A_DI_MixedReality") : (DrawingImage)FindResource("DA_DI_MixedReality") : (DrawingImage)FindResource("DI_Sandtime");
            Xbox.Source = !UninstallingApps.IsAppDeletedList["Xbox"] ? UninstallingApps.UserAppsList.Contains("Microsoft.XboxApp") || UninstallingApps.UserAppsList.Contains("Microsoft.GamingApp") || UninstallingApps.UserAppsList.Contains("Microsoft.XboxGamingOverlay") ||
               UninstallingApps.UserAppsList.Contains("Microsoft.XboxGameOverlay") || UninstallingApps.UserAppsList.Contains("Microsoft.XboxIdentityProvider") || UninstallingApps.UserAppsList.Contains("Microsoft.Xbox.TCUI") ||
               UninstallingApps.UserAppsList.Contains("Microsoft.XboxSpeechToTextOverlay") ? (DrawingImage)FindResource("A_DI_Xbox") : (DrawingImage)FindResource("DA_DI_Xbox") : (DrawingImage)FindResource("DI_Sandtime");
            Paint3D.Source = !UninstallingApps.IsAppDeletedList["Paint3D"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MSPaint") ? (DrawingImage)FindResource("A_DI_Paint3D") : (DrawingImage)FindResource("DA_DI_Paint3D") : (DrawingImage)FindResource("DI_Sandtime");
            OneNote.Source = !UninstallingApps.IsAppDeletedList["OneNote"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Office.OneNote") ? (DrawingImage)FindResource("A_DI_OneNote") : (DrawingImage)FindResource("DA_DI_OneNote") : (DrawingImage)FindResource("DI_Sandtime");
            People.Source = !UninstallingApps.IsAppDeletedList["People"] ? UninstallingApps.UserAppsList.Contains("Microsoft.People") ? (DrawingImage)FindResource("A_DI_People") : (DrawingImage)FindResource("DA_DI_People") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftStickyNotes.Source = !UninstallingApps.IsAppDeletedList["MicrosoftStickyNotes"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftStickyNotes") ? (DrawingImage)FindResource("A_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DA_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DI_Sandtime");
            Widgets.Source = !UninstallingApps.IsAppDeletedList["Widgets"] ? UninstallingApps.UserAppsList.Contains("MicrosoftWindows.Client.WebExperience") ? (DrawingImage)FindResource("A_DI_Widgets") : (DrawingImage)FindResource("DA_DI_Widgets") : (DrawingImage)FindResource("DI_Sandtime");
            ScreenSketch.Source = !UninstallingApps.IsAppDeletedList["ScreenSketch"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ScreenSketch") ? (DrawingImage)FindResource("A_DI_ScreenSketch") : (DrawingImage)FindResource("DA_DI_ScreenSketch") : (DrawingImage)FindResource("DI_Sandtime");
            Phone.Source = !UninstallingApps.IsAppDeletedList["Phone"] ? UninstallingApps.UserAppsList.Contains("Microsoft.YourPhone") ? (DrawingImage)FindResource("A_DI_Phone") : (DrawingImage)FindResource("DA_DI_Phone") : (DrawingImage)FindResource("DI_Sandtime");
            Photos.Source = !UninstallingApps.IsAppDeletedList["Photos"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Windows.Photos") ? (DrawingImage)FindResource("A_DI_Photos") : (DrawingImage)FindResource("DA_DI_Photos") : (DrawingImage)FindResource("DI_Sandtime");
            FeedbackHub.Source = !UninstallingApps.IsAppDeletedList["FeedbackHub"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsFeedbackHub") ? (DrawingImage)FindResource("A_DI_FeedbackHub") : (DrawingImage)FindResource("DA_DI_FeedbackHub") : (DrawingImage)FindResource("DI_Sandtime");
            SoundRecorder.Source = !UninstallingApps.IsAppDeletedList["SoundRecorder"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsSoundRecorder") ? (DrawingImage)FindResource("A_DI_SoundRecorder") : (DrawingImage)FindResource("DA_DI_SoundRecorder") : (DrawingImage)FindResource("DI_Sandtime");
            Alarms.Source = !UninstallingApps.IsAppDeletedList["Alarms"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsAlarms") ? (DrawingImage)FindResource("A_DI_Alarms") : (DrawingImage)FindResource("DA_DI_Alarms") : (DrawingImage)FindResource("DI_Sandtime");
            SkypeApp.Source = !UninstallingApps.IsAppDeletedList["SkypeApp"] ? UninstallingApps.UserAppsList.Contains("Microsoft.SkypeApp") ? (DrawingImage)FindResource("A_DI_SkypeApp") : (DrawingImage)FindResource("DA_DI_SkypeApp") : (DrawingImage)FindResource("DI_Sandtime");
            Maps.Source = !UninstallingApps.IsAppDeletedList["Maps"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsMaps") ? (DrawingImage)FindResource("A_DI_Maps") : (DrawingImage)FindResource("DA_DI_Maps") : (DrawingImage)FindResource("DI_Sandtime");
            Camera.Source = !UninstallingApps.IsAppDeletedList["Camera"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsCamera") ? (DrawingImage)FindResource("A_DI_Camera") : (DrawingImage)FindResource("DA_DI_Camera") : (DrawingImage)FindResource("DI_Sandtime");
            Video.Source = !UninstallingApps.IsAppDeletedList["Video"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ZuneVideo") ? (DrawingImage)FindResource("A_DI_Video") : (DrawingImage)FindResource("DA_DI_Video") : (DrawingImage)FindResource("DI_Sandtime");
            BingNews.Source = !UninstallingApps.IsAppDeletedList["BingNews"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingNews") ? (DrawingImage)FindResource("A_DI_BingNews") : (DrawingImage)FindResource("DA_DI_BingNews") : (DrawingImage)FindResource("DI_Sandtime");
            Mail.Source = !UninstallingApps.IsAppDeletedList["Mail"] ? UninstallingApps.UserAppsList.Contains("microsoft.windowscommunicationsapps") ? (DrawingImage)FindResource("A_DI_Mail") : (DrawingImage)FindResource("DA_DI_Mail") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftTeams.Source = !UninstallingApps.IsAppDeletedList["MicrosoftTeams"] ? UninstallingApps.UserAppsList.Contains("MicrosoftTeams") || UninstallingApps.UserAppsList.Contains("MSTeams") ? (DrawingImage)FindResource("A_DI_MicrosoftTeams") : (DrawingImage)FindResource("DA_DI_MicrosoftTeams") : (DrawingImage)FindResource("DI_Sandtime");
            PowerAutomateDesktop.Source = !UninstallingApps.IsAppDeletedList["PowerAutomateDesktop"] ? UninstallingApps.UserAppsList.Contains("Microsoft.PowerAutomateDesktop") ? (DrawingImage)FindResource("A_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DA_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DI_Sandtime");
            Cortana.Source = !UninstallingApps.IsAppDeletedList["Cortana"] ? UninstallingApps.UserAppsList.Contains("Microsoft.549981C3F5F10") ? (DrawingImage)FindResource("A_DI_Cortana") : (DrawingImage)FindResource("DA_DI_Cortana") : (DrawingImage)FindResource("DI_Sandtime");
            ClipChamp.Source = !UninstallingApps.IsAppDeletedList["ClipChamp"] ? UninstallingApps.UserAppsList.Contains("Clipchamp.Clipchamp") ? (DrawingImage)FindResource("A_DI_ClipChamp") : (DrawingImage)FindResource("DA_DI_ClipChamp") : (DrawingImage)FindResource("DI_Sandtime");
            GetStarted.Source = !UninstallingApps.IsAppDeletedList["GetStarted"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Getstarted") ? (DrawingImage)FindResource("A_DI_GetStarted") : (DrawingImage)FindResource("DA_DI_GetStarted") : (DrawingImage)FindResource("DI_Sandtime");
            OneDrive.Source = !UninstallingApps.IsAppDeletedList["OneDrive"] ? UninstallingApps.IsOneDriveInstalled ? (DrawingImage)FindResource("A_DI_OneDrive") : (DrawingImage)FindResource("DA_DI_OneDrive") : (DrawingImage)FindResource("DI_Sandtime");
            BingSports.Source = !UninstallingApps.IsAppDeletedList["BingSports"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingSports") ? (DrawingImage)FindResource("A_DI_BingSports") : (DrawingImage)FindResource("DA_DI_BingSports") : (DrawingImage)FindResource("DI_Sandtime");
            BingFinance.Source = !UninstallingApps.IsAppDeletedList["BingFinance"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingFinance") ? (DrawingImage)FindResource("A_DI_BingFinance") : (DrawingImage)FindResource("DA_DI_BingFinance") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftFamily.Source = !UninstallingApps.IsAppDeletedList["MicrosoftFamily"] ? UninstallingApps.UserAppsList.Contains("MicrosoftFamily") ? (DrawingImage)FindResource("A_DI_MicrosoftFamily") : (DrawingImage)FindResource("DA_DI_MicrosoftFamily") : (DrawingImage)FindResource("DI_Sandtime");
            BingSearch.Source = !UninstallingApps.IsAppDeletedList["BingSearch"] ? UninstallingApps.UserAppsList.Contains("BingSearch") ? (DrawingImage)FindResource("A_DI_BingSearch") : (DrawingImage)FindResource("DA_DI_BingSearch") : (DrawingImage)FindResource("DI_Sandtime");
            Outlook.Source = !UninstallingApps.IsAppDeletedList["Outlook"] ? UninstallingApps.UserAppsList.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_Outlook") : (DrawingImage)FindResource("DA_DI_Outlook") : (DrawingImage)FindResource("DI_Sandtime");
            QuickAssist.Source = !UninstallingApps.IsAppDeletedList["QuickAssist"] ? UninstallingApps.UserAppsList.Contains("MicrosoftCorporationII.QuickAssist") ? (DrawingImage)FindResource("A_DI_QuickAssist") : (DrawingImage)FindResource("DA_DI_QuickAssist") : (DrawingImage)FindResource("DI_Sandtime");
        }
    }
}
