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

            App.LanguageChanged += (s, e) => { WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(0)); };

            DispatcherTimer timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                if (time.TotalSeconds % 2 == 0)
                    UpdateViewStateApps();
                else if (time.TotalSeconds % 5 == 0)
                {
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += (s, e) => { Parallel.Invoke(new UninstallingApps().ViewInstalledApp); };
                    backgroundWorker.RunWorkerAsync();
                }

                time = time.Add(TimeSpan.FromSeconds(+1));
            }, Application.Current.Dispatcher);
        }

        private void ClickApp_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Image appImage = (Image)sender;
            applicationName = appImage.Name;

            if (e.LeftButton == MouseButtonState.Pressed && appImage.Source == (DrawingImage)FindResource("A_DI_" + appImage.Name))
            {
                appImage.Source = (DrawingImage)FindResource("DI_Sandtime");

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, _) =>
                {
                    if (applicationName != "YandexMusic")
                    {
                        UninstallingApps.isAppDeletedList[applicationName] = true;
                        UninstallingApps.DeletedApp(applicationName);
                    }
                    else
                    {
                        UninstallingApps.isAppDeletedList["Yandex.Music"] = true;
                        UninstallingApps.DeletedApp("Yandex.Music");
                    }
                };
                backgroundWorker.RunWorkerCompleted += async (s, _) =>
                {
                    await Task.Delay(25000);
                    if (applicationName != "YandexMusic")
                        UninstallingApps.isAppDeletedList[appImage.Name] = false;
                    else
                        UninstallingApps.isAppDeletedList["Yandex.Music"] = false;
                    UpdateViewStateApps();
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }

            if (e.LeftButton == MouseButtonState.Pressed && appImage.Source == (DrawingImage)FindResource("DA_DI_" + appImage.Name) && applicationName == "OneDrive")
            {
                appImage.Source = (DrawingImage)FindResource("DI_Sandtime");
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("onedrive_notification"));

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, _) => UninstallingApps.ResetOneDrive();
                backgroundWorker.RunWorkerCompleted += async (s, _) =>
                {
                    await Task.Delay(15000);
                    UninstallingApps.isAppDeletedList[appImage.Name] = false;
                    UpdateViewStateApps();
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void BtnDelete_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                foreach (string key in UninstallingApps.isAppDeletedList.Keys.ToList())
                {
                    UninstallingApps.isAppDeletedList[key] = true;
                    UpdateViewStateApps();
                }

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, _) =>
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("appsdelete_notification"));
                    Parallel.Invoke(UninstallingApps.DeletedApp_All);
                };
                backgroundWorker.RunWorkerCompleted += async (s, _) =>
                {
                    await Task.Delay(60000);
                    foreach (string key in UninstallingApps.isAppDeletedList.Keys.ToList())
                    {
                        UninstallingApps.isAppDeletedList[key] = false;
                        UpdateViewStateApps();
                    }
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }
        }


        private void Apps_MouseEnter(object sender, MouseEventArgs e)
        {
            StackPanel stackPanel = (StackPanel)sender;

            if (TextDescription.Text != (string)FindResource(stackPanel.Name + "_applications"))
            {
                string _descriptionApps = (string)FindResource(stackPanel.Name + "_applications");
                WorkWithText.TypeWriteAnimation(_descriptionApps, TextDescription, TimeSpan.FromMilliseconds(250));
            }
        }

        private void Apps_MouseLeave(object sender, MouseEventArgs e)
        {
            if (TextDescription.Text != (string)FindResource("defaultDescriptionApp"))
                WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(250));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Parallel.Invoke(UpdateViewStateApps);
            WorkWithText.TypeWriteAnimation((string)FindResource("defaultDescriptionApp"), TextDescription, TimeSpan.FromMilliseconds(300));
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }

        private void UpdateViewStateApps()
        {
            MicrosoftStore.Source = !UninstallingApps.isAppDeletedList["MicrosoftStore"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsStore") ? (DrawingImage)FindResource("A_DI_MicrosoftStore") : (DrawingImage)FindResource("DA_DI_MicrosoftStore") : (DrawingImage)FindResource("DI_Sandtime");
            Todos.Source = !UninstallingApps.isAppDeletedList["Todos"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Todos") ? (DrawingImage)FindResource("A_DI_Todos") : (DrawingImage)FindResource("DA_DI_Todos") : (DrawingImage)FindResource("DI_Sandtime");
            BingWeather.Source = !UninstallingApps.isAppDeletedList["BingWeather"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingWeather") ? (DrawingImage)FindResource("A_DI_BingWeather") : (DrawingImage)FindResource("DA_DI_BingWeather") : (DrawingImage)FindResource("DI_Sandtime");
            Microsoft3D.Source = !UninstallingApps.isAppDeletedList["Microsoft3D"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Microsoft3DViewer") ? (DrawingImage)FindResource("A_DI_Microsoft3D") : (DrawingImage)FindResource("DA_DI_Microsoft3D") : (DrawingImage)FindResource("DI_Sandtime");
            Music.Source = !UninstallingApps.isAppDeletedList["Music"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ZuneMusic") ? (DrawingImage)FindResource("A_DI_Music") : (DrawingImage)FindResource("DA_DI_Music") : (DrawingImage)FindResource("DI_Sandtime");
            GetHelp.Source = !UninstallingApps.isAppDeletedList["GetHelp"] ? UninstallingApps.UserAppsList.Contains("Microsoft.GetHelp") ? (DrawingImage)FindResource("A_DI_GetHelp") : (DrawingImage)FindResource("DA_DI_GetHelp") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftOfficeHub.Source = !UninstallingApps.isAppDeletedList["MicrosoftOfficeHub"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftOfficeHub") || UninstallingApps.UserAppsList.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DA_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftSolitaireCollection.Source = !UninstallingApps.isAppDeletedList["MicrosoftSolitaireCollection"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftSolitaireCollection") ? (DrawingImage)FindResource("A_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DA_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DI_Sandtime"); MixedReality.Source = !UninstallingApps.isAppDeletedList["MixedReality"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MixedReality.Portal") ? (DrawingImage)FindResource("A_DI_MixedReality") : (DrawingImage)FindResource("DA_DI_MixedReality") : (DrawingImage)FindResource("DI_Sandtime");
            Xbox.Source = !UninstallingApps.isAppDeletedList["Xbox"] ? UninstallingApps.UserAppsList.Contains("Microsoft.XboxApp") || UninstallingApps.UserAppsList.Contains("Microsoft.GamingApp") || UninstallingApps.UserAppsList.Contains("Microsoft.XboxGamingOverlay") ||
               UninstallingApps.UserAppsList.Contains("Microsoft.XboxGameOverlay") || UninstallingApps.UserAppsList.Contains("Microsoft.XboxIdentityProvider") || UninstallingApps.UserAppsList.Contains("Microsoft.Xbox.TCUI") || 
               UninstallingApps.UserAppsList.Contains("Microsoft.XboxSpeechToTextOverlay") ? (DrawingImage)FindResource("A_DI_Xbox") : (DrawingImage)FindResource("DA_DI_Xbox") : (DrawingImage)FindResource("DI_Sandtime");
            Paint3D.Source = !UninstallingApps.isAppDeletedList["Paint3D"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MSPaint") || UninstallingApps.UserAppsList.Contains("Microsoft.Paint") ? (DrawingImage)FindResource("A_DI_Paint3D") : (DrawingImage)FindResource("DA_DI_Paint3D") : (DrawingImage)FindResource("DI_Sandtime");
            OneNote.Source = !UninstallingApps.isAppDeletedList["OneNote"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Office.OneNote") ? (DrawingImage)FindResource("A_DI_OneNote") : (DrawingImage)FindResource("DA_DI_OneNote") : (DrawingImage)FindResource("DI_Sandtime");
            People.Source = !UninstallingApps.isAppDeletedList["People"] ? UninstallingApps.UserAppsList.Contains("Microsoft.People") ? (DrawingImage)FindResource("A_DI_People") : (DrawingImage)FindResource("DA_DI_People") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftStickyNotes.Source = !UninstallingApps.isAppDeletedList["MicrosoftStickyNotes"] ? UninstallingApps.UserAppsList.Contains("Microsoft.MicrosoftStickyNotes") ? (DrawingImage)FindResource("A_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DA_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DI_Sandtime");
            Widgets.Source = !UninstallingApps.isAppDeletedList["Widgets"] ? UninstallingApps.UserAppsList.Contains("MicrosoftWindows.Client.WebExperience") ? (DrawingImage)FindResource("A_DI_Widgets") : (DrawingImage)FindResource("DA_DI_Widgets") : (DrawingImage)FindResource("DI_Sandtime");
            ScreenSketch.Source = !UninstallingApps.isAppDeletedList["ScreenSketch"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ScreenSketch") ? (DrawingImage)FindResource("A_DI_ScreenSketch") : (DrawingImage)FindResource("DA_DI_ScreenSketch") : (DrawingImage)FindResource("DI_Sandtime");
            Phone.Source = !UninstallingApps.isAppDeletedList["Phone"] ? UninstallingApps.UserAppsList.Contains("Microsoft.YourPhone") ? (DrawingImage)FindResource("A_DI_Phone") : (DrawingImage)FindResource("DA_DI_Phone") : (DrawingImage)FindResource("DI_Sandtime");
            Photos.Source = !UninstallingApps.isAppDeletedList["Photos"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Windows.Photos") ? (DrawingImage)FindResource("A_DI_Photos") : (DrawingImage)FindResource("DA_DI_Photos") : (DrawingImage)FindResource("DI_Sandtime");
            FeedbackHub.Source = !UninstallingApps.isAppDeletedList["FeedbackHub"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsFeedbackHub") ? (DrawingImage)FindResource("A_DI_FeedbackHub") : (DrawingImage)FindResource("DA_DI_FeedbackHub") : (DrawingImage)FindResource("DI_Sandtime");
            SoundRecorder.Source = !UninstallingApps.isAppDeletedList["SoundRecorder"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsSoundRecorder") ? (DrawingImage)FindResource("A_DI_SoundRecorder") : (DrawingImage)FindResource("DA_DI_SoundRecorder") : (DrawingImage)FindResource("DI_Sandtime");
            Alarms.Source = !UninstallingApps.isAppDeletedList["Alarms"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsAlarms") ? (DrawingImage)FindResource("A_DI_Alarms") : (DrawingImage)FindResource("DA_DI_Alarms") : (DrawingImage)FindResource("DI_Sandtime");
            SkypeApp.Source = !UninstallingApps.isAppDeletedList["SkypeApp"] ? UninstallingApps.UserAppsList.Contains("Microsoft.SkypeApp") ? (DrawingImage)FindResource("A_DI_SkypeApp") : (DrawingImage)FindResource("DA_DI_SkypeApp") : (DrawingImage)FindResource("DI_Sandtime");
            Maps.Source = !UninstallingApps.isAppDeletedList["Maps"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsMaps") ? (DrawingImage)FindResource("A_DI_Maps") : (DrawingImage)FindResource("DA_DI_Maps") : (DrawingImage)FindResource("DI_Sandtime");
            Camera.Source = !UninstallingApps.isAppDeletedList["Camera"] ? UninstallingApps.UserAppsList.Contains("Microsoft.WindowsCamera") ? (DrawingImage)FindResource("A_DI_Camera") : (DrawingImage)FindResource("DA_DI_Camera") : (DrawingImage)FindResource("DI_Sandtime");
            Video.Source = !UninstallingApps.isAppDeletedList["Video"] ? UninstallingApps.UserAppsList.Contains("Microsoft.ZuneVideo") ? (DrawingImage)FindResource("A_DI_Video") : (DrawingImage)FindResource("DA_DI_Video") : (DrawingImage)FindResource("DI_Sandtime");
            BingNews.Source = !UninstallingApps.isAppDeletedList["BingNews"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingNews") ? (DrawingImage)FindResource("A_DI_BingNews") : (DrawingImage)FindResource("DA_DI_BingNews") : (DrawingImage)FindResource("DI_Sandtime");
            Mail.Source = !UninstallingApps.isAppDeletedList["Mail"] ? UninstallingApps.UserAppsList.Contains("microsoft.windowscommunicationsapps") ? (DrawingImage)FindResource("A_DI_Mail") : (DrawingImage)FindResource("DA_DI_Mail") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftTeams.Source = !UninstallingApps.isAppDeletedList["MicrosoftTeams"] ? UninstallingApps.UserAppsList.Contains("MicrosoftTeams") ? (DrawingImage)FindResource("A_DI_MicrosoftTeams") : (DrawingImage)FindResource("DA_DI_MicrosoftTeams") : (DrawingImage)FindResource("DI_Sandtime");
            PowerAutomateDesktop.Source = !UninstallingApps.isAppDeletedList["PowerAutomateDesktop"] ? UninstallingApps.UserAppsList.Contains("Microsoft.PowerAutomateDesktop") ? (DrawingImage)FindResource("A_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DA_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DI_Sandtime");
            Cortana.Source = !UninstallingApps.isAppDeletedList["Cortana"] ? UninstallingApps.UserAppsList.Contains("Microsoft.549981C3F5F10") ? (DrawingImage)FindResource("A_DI_Cortana") : (DrawingImage)FindResource("DA_DI_Cortana") : (DrawingImage)FindResource("DI_Sandtime");
            ClipChamp.Source = !UninstallingApps.isAppDeletedList["ClipChamp"] ? UninstallingApps.UserAppsList.Contains("Clipchamp.Clipchamp") ? (DrawingImage)FindResource("A_DI_ClipChamp") : (DrawingImage)FindResource("DA_DI_ClipChamp") : (DrawingImage)FindResource("DI_Sandtime");
            GetStarted.Source = !UninstallingApps.isAppDeletedList["GetStarted"] ? UninstallingApps.UserAppsList.Contains("Microsoft.Getstarted") ? (DrawingImage)FindResource("A_DI_GetStarted") : (DrawingImage)FindResource("DA_DI_GetStarted") : (DrawingImage)FindResource("DI_Sandtime");
            OneDrive.Source = !UninstallingApps.isAppDeletedList["OneDrive"] ? UninstallingApps.IsOneDriveInstalled ? (DrawingImage)FindResource("A_DI_OneDrive") : (DrawingImage)FindResource("DA_DI_OneDrive") : (DrawingImage)FindResource("DI_Sandtime");
            BingSports.Source = !UninstallingApps.isAppDeletedList["BingSports"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingSports") ? (DrawingImage)FindResource("A_DI_BingSports") : (DrawingImage)FindResource("DA_DI_BingSports") : (DrawingImage)FindResource("DI_Sandtime");
            BingFinance.Source = !UninstallingApps.isAppDeletedList["BingFinance"] ? UninstallingApps.UserAppsList.Contains("Microsoft.BingFinance") ? (DrawingImage)FindResource("A_DI_BingFinance") : (DrawingImage)FindResource("DA_DI_BingFinance") : (DrawingImage)FindResource("DI_Sandtime");
            YandexMusic.Source = !UninstallingApps.isAppDeletedList["Yandex.Music"] ? UninstallingApps.UserAppsList.Contains("Yandex.Music") ? (DrawingImage)FindResource("A_DI_YandexMusic") : (DrawingImage)FindResource("DA_DI_YandexMusic") : (DrawingImage)FindResource("DI_Sandtime");
            Netflix.Source = !UninstallingApps.isAppDeletedList["Netflix"] ? UninstallingApps.UserAppsList.Contains("Netflix") ? (DrawingImage)FindResource("A_DI_Netflix") : (DrawingImage)FindResource("DA_DI_Netflix") : (DrawingImage)FindResource("DI_Sandtime");
            Outlook.Source = !UninstallingApps.isAppDeletedList["Outlook"] ? UninstallingApps.UserAppsList.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_Outlook") : (DrawingImage)FindResource("DA_DI_Outlook") : (DrawingImage)FindResource("DI_Sandtime");
            QuickAssist.Source = !UninstallingApps.isAppDeletedList["QuickAssist"] ? UninstallingApps.UserAppsList.Contains("MicrosoftCorporationII.QuickAssist") ? (DrawingImage)FindResource("A_DI_QuickAssist") : (DrawingImage)FindResource("DA_DI_QuickAssist") : (DrawingImage)FindResource("DI_Sandtime");
        }
    }
}
