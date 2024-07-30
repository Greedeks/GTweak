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
        private string _appName = default;
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
            Image img = (Image)sender;
            _appName = img.Name;

            if (e.LeftButton == MouseButtonState.Pressed && img.Source == (DrawingImage)FindResource("A_DI_"+img.Name))
            {
                img.Source = (DrawingImage)FindResource("DI_Sandtime");

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, _) =>
                {
                    if (_appName != "YandexMusic")
                    {
                        UninstallingApps.RemovalProcess[_appName] = true;
                        Parallel.Invoke(() => UninstallingApps.DeletedApp(_appName));
                    }
                    else
                    {
                        UninstallingApps.RemovalProcess["Yandex.Music"] = true;
                        Parallel.Invoke(() => UninstallingApps.DeletedApp("Yandex.Music"));
                    }
                };
                backgroundWorker.RunWorkerCompleted += async (s, _) =>
                {
                    await Task.Delay(25000);
                    if (_appName != "YandexMusic")
                        UninstallingApps.RemovalProcess[img.Name] = false;
                    else
                        UninstallingApps.RemovalProcess["Yandex.Music"] = false;
                    UpdateViewStateApps();
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }

            if (e.LeftButton == MouseButtonState.Pressed && img.Source == (DrawingImage)FindResource("DA_DI_" + img.Name) && _appName == "OneDrive")
            {
                img.Source = (DrawingImage)FindResource("DI_Sandtime");
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("onedrive_notification"));

                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (s, _) => Parallel.Invoke(UninstallingApps.ResetOneDrive);
                backgroundWorker.RunWorkerCompleted += async (s, _) =>
                {
                    await Task.Delay(15000);
                    UninstallingApps.RemovalProcess[img.Name] = false;
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
                foreach (string key in UninstallingApps.RemovalProcess.Keys.ToList())
                {
                    UninstallingApps.RemovalProcess[key] = true;
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
                    foreach (string key in UninstallingApps.RemovalProcess.Keys.ToList())
                    {
                        UninstallingApps.RemovalProcess[key] = false;
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
            MicrosoftStore.Source = !UninstallingApps.RemovalProcess["MicrosoftStore"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsStore") ? (DrawingImage)FindResource("A_DI_MicrosoftStore") : (DrawingImage)FindResource("DA_DI_MicrosoftStore") : (DrawingImage)FindResource("DI_Sandtime");
            Todos.Source = !UninstallingApps.RemovalProcess["Todos"] ? UninstallingApps.ListApps.Contains("Microsoft.Todos") ? (DrawingImage)FindResource("A_DI_Todos") : (DrawingImage)FindResource("DA_DI_Todos") : (DrawingImage)FindResource("DI_Sandtime");
            BingWeather.Source = !UninstallingApps.RemovalProcess["BingWeather"] ? UninstallingApps.ListApps.Contains("Microsoft.BingWeather") ? (DrawingImage)FindResource("A_DI_BingWeather") : (DrawingImage)FindResource("DA_DI_BingWeather") : (DrawingImage)FindResource("DI_Sandtime");
            Microsoft3D.Source = !UninstallingApps.RemovalProcess["Microsoft3D"] ? UninstallingApps.ListApps.Contains("Microsoft.Microsoft3DViewer") ? (DrawingImage)FindResource("A_DI_Microsoft3D") : (DrawingImage)FindResource("DA_DI_Microsoft3D") : (DrawingImage)FindResource("DI_Sandtime");
            Music.Source = !UninstallingApps.RemovalProcess["Music"] ? UninstallingApps.ListApps.Contains("Microsoft.ZuneMusic") ? (DrawingImage)FindResource("A_DI_Music") : (DrawingImage)FindResource("DA_DI_Music") : (DrawingImage)FindResource("DI_Sandtime");
            GetHelp.Source = !UninstallingApps.RemovalProcess["GetHelp"] ? UninstallingApps.ListApps.Contains("Microsoft.GetHelp") ? (DrawingImage)FindResource("A_DI_GetHelp") : (DrawingImage)FindResource("DA_DI_GetHelp") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftOfficeHub.Source = !UninstallingApps.RemovalProcess["MicrosoftOfficeHub"] ? UninstallingApps.ListApps.Contains("Microsoft.MicrosoftOfficeHub") || UninstallingApps.ListApps.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DA_DI_MicrosoftOfficeHub") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftSolitaireCollection.Source = !UninstallingApps.RemovalProcess["MicrosoftSolitaireCollection"] ? UninstallingApps.ListApps.Contains("Microsoft.MicrosoftSolitaireCollection") ? (DrawingImage)FindResource("A_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DA_DI_MicrosoftSolitaireCollection") : (DrawingImage)FindResource("DI_Sandtime"); MixedReality.Source = !UninstallingApps.RemovalProcess["MixedReality"] ? UninstallingApps.ListApps.Contains("Microsoft.MixedReality.Portal") ? (DrawingImage)FindResource("A_DI_MixedReality") : (DrawingImage)FindResource("DA_DI_MixedReality") : (DrawingImage)FindResource("DI_Sandtime");
            Xbox.Source = !UninstallingApps.RemovalProcess["Xbox"] ? UninstallingApps.ListApps.Contains("Microsoft.XboxApp") || UninstallingApps.ListApps.Contains("Microsoft.GamingApp") || UninstallingApps.ListApps.Contains("Microsoft.XboxGamingOverlay") ||
               UninstallingApps.ListApps.Contains("Microsoft.XboxGameOverlay") || UninstallingApps.ListApps.Contains("Microsoft.XboxIdentityProvider") || UninstallingApps.ListApps.Contains("Microsoft.Xbox.TCUI") || 
               UninstallingApps.ListApps.Contains("Microsoft.XboxSpeechToTextOverlay") ? (DrawingImage)FindResource("A_DI_Xbox") : (DrawingImage)FindResource("DA_DI_Xbox") : (DrawingImage)FindResource("DI_Sandtime");
            Paint3D.Source = !UninstallingApps.RemovalProcess["Paint3D"] ? UninstallingApps.ListApps.Contains("Microsoft.MSPaint") || UninstallingApps.ListApps.Contains("Microsoft.Paint") ? (DrawingImage)FindResource("A_DI_Paint3D") : (DrawingImage)FindResource("DA_DI_Paint3D") : (DrawingImage)FindResource("DI_Sandtime");
            OneNote.Source = !UninstallingApps.RemovalProcess["OneNote"] ? UninstallingApps.ListApps.Contains("Microsoft.Office.OneNote") ? (DrawingImage)FindResource("A_DI_OneNote") : (DrawingImage)FindResource("DA_DI_OneNote") : (DrawingImage)FindResource("DI_Sandtime");
            People.Source = !UninstallingApps.RemovalProcess["People"] ? UninstallingApps.ListApps.Contains("Microsoft.People") ? (DrawingImage)FindResource("A_DI_People") : (DrawingImage)FindResource("DA_DI_People") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftStickyNotes.Source = !UninstallingApps.RemovalProcess["MicrosoftStickyNotes"] ? UninstallingApps.ListApps.Contains("Microsoft.MicrosoftStickyNotes") ? (DrawingImage)FindResource("A_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DA_DI_MicrosoftStickyNotes") : (DrawingImage)FindResource("DI_Sandtime");
            Widgets.Source = !UninstallingApps.RemovalProcess["Widgets"] ? UninstallingApps.ListApps.Contains("MicrosoftWindows.Client.WebExperience") ? (DrawingImage)FindResource("A_DI_Widgets") : (DrawingImage)FindResource("DA_DI_Widgets") : (DrawingImage)FindResource("DI_Sandtime");
            ScreenSketch.Source = !UninstallingApps.RemovalProcess["ScreenSketch"] ? UninstallingApps.ListApps.Contains("Microsoft.ScreenSketch") ? (DrawingImage)FindResource("A_DI_ScreenSketch") : (DrawingImage)FindResource("DA_DI_ScreenSketch") : (DrawingImage)FindResource("DI_Sandtime");
            Phone.Source = !UninstallingApps.RemovalProcess["Phone"] ? UninstallingApps.ListApps.Contains("Microsoft.YourPhone") ? (DrawingImage)FindResource("A_DI_Phone") : (DrawingImage)FindResource("DA_DI_Phone") : (DrawingImage)FindResource("DI_Sandtime");
            Photos.Source = !UninstallingApps.RemovalProcess["Photos"] ? UninstallingApps.ListApps.Contains("Microsoft.Windows.Photos") ? (DrawingImage)FindResource("A_DI_Photos") : (DrawingImage)FindResource("DA_DI_Photos") : (DrawingImage)FindResource("DI_Sandtime");
            FeedbackHub.Source = !UninstallingApps.RemovalProcess["FeedbackHub"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsFeedbackHub") ? (DrawingImage)FindResource("A_DI_FeedbackHub") : (DrawingImage)FindResource("DA_DI_FeedbackHub") : (DrawingImage)FindResource("DI_Sandtime");
            SoundRecorder.Source = !UninstallingApps.RemovalProcess["SoundRecorder"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsSoundRecorder") ? (DrawingImage)FindResource("A_DI_SoundRecorder") : (DrawingImage)FindResource("DA_DI_SoundRecorder") : (DrawingImage)FindResource("DI_Sandtime");
            Alarms.Source = !UninstallingApps.RemovalProcess["Alarms"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsAlarms") ? (DrawingImage)FindResource("A_DI_Alarms") : (DrawingImage)FindResource("DA_DI_Alarms") : (DrawingImage)FindResource("DI_Sandtime");
            SkypeApp.Source = !UninstallingApps.RemovalProcess["SkypeApp"] ? UninstallingApps.ListApps.Contains("Microsoft.SkypeApp") ? (DrawingImage)FindResource("A_DI_SkypeApp") : (DrawingImage)FindResource("DA_DI_SkypeApp") : (DrawingImage)FindResource("DI_Sandtime");
            Maps.Source = !UninstallingApps.RemovalProcess["Maps"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsMaps") ? (DrawingImage)FindResource("A_DI_Maps") : (DrawingImage)FindResource("DA_DI_Maps") : (DrawingImage)FindResource("DI_Sandtime");
            Camera.Source = !UninstallingApps.RemovalProcess["Camera"] ? UninstallingApps.ListApps.Contains("Microsoft.WindowsCamera") ? (DrawingImage)FindResource("A_DI_Camera") : (DrawingImage)FindResource("DA_DI_Camera") : (DrawingImage)FindResource("DI_Sandtime");
            Video.Source = !UninstallingApps.RemovalProcess["Video"] ? UninstallingApps.ListApps.Contains("Microsoft.ZuneVideo") ? (DrawingImage)FindResource("A_DI_Video") : (DrawingImage)FindResource("DA_DI_Video") : (DrawingImage)FindResource("DI_Sandtime");
            BingNews.Source = !UninstallingApps.RemovalProcess["BingNews"] ? UninstallingApps.ListApps.Contains("Microsoft.BingNews") ? (DrawingImage)FindResource("A_DI_BingNews") : (DrawingImage)FindResource("DA_DI_BingNews") : (DrawingImage)FindResource("DI_Sandtime");
            Mail.Source = !UninstallingApps.RemovalProcess["Mail"] ? UninstallingApps.ListApps.Contains("microsoft.windowscommunicationsapps") ? (DrawingImage)FindResource("A_DI_Mail") : (DrawingImage)FindResource("DA_DI_Mail") : (DrawingImage)FindResource("DI_Sandtime");
            MicrosoftTeams.Source = !UninstallingApps.RemovalProcess["MicrosoftTeams"] ? UninstallingApps.ListApps.Contains("MicrosoftTeams") ? (DrawingImage)FindResource("A_DI_MicrosoftTeams") : (DrawingImage)FindResource("DA_DI_MicrosoftTeams") : (DrawingImage)FindResource("DI_Sandtime");
            PowerAutomateDesktop.Source = !UninstallingApps.RemovalProcess["PowerAutomateDesktop"] ? UninstallingApps.ListApps.Contains("Microsoft.PowerAutomateDesktop") ? (DrawingImage)FindResource("A_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DA_DI_PowerAutomateDesktop") : (DrawingImage)FindResource("DI_Sandtime");
            Cortana.Source = !UninstallingApps.RemovalProcess["Cortana"] ? UninstallingApps.ListApps.Contains("Microsoft.549981C3F5F10") ? (DrawingImage)FindResource("A_DI_Cortana") : (DrawingImage)FindResource("DA_DI_Cortana") : (DrawingImage)FindResource("DI_Sandtime");
            ClipChamp.Source = !UninstallingApps.RemovalProcess["ClipChamp"] ? UninstallingApps.ListApps.Contains("Clipchamp.Clipchamp") ? (DrawingImage)FindResource("A_DI_ClipChamp") : (DrawingImage)FindResource("DA_DI_ClipChamp") : (DrawingImage)FindResource("DI_Sandtime");
            GetStarted.Source = !UninstallingApps.RemovalProcess["GetStarted"] ? UninstallingApps.ListApps.Contains("Microsoft.Getstarted") ? (DrawingImage)FindResource("A_DI_GetStarted") : (DrawingImage)FindResource("DA_DI_GetStarted") : (DrawingImage)FindResource("DI_Sandtime");
            OneDrive.Source = !UninstallingApps.RemovalProcess["OneDrive"] ? UninstallingApps.IsOneDriveInstalled ? (DrawingImage)FindResource("A_DI_OneDrive") : (DrawingImage)FindResource("DA_DI_OneDrive") : (DrawingImage)FindResource("DI_Sandtime");
            BingSports.Source = !UninstallingApps.RemovalProcess["BingSports"] ? UninstallingApps.ListApps.Contains("Microsoft.BingSports") ? (DrawingImage)FindResource("A_DI_BingSports") : (DrawingImage)FindResource("DA_DI_BingSports") : (DrawingImage)FindResource("DI_Sandtime");
            BingFinance.Source = !UninstallingApps.RemovalProcess["BingFinance"] ? UninstallingApps.ListApps.Contains("Microsoft.BingFinance") ? (DrawingImage)FindResource("A_DI_BingFinance") : (DrawingImage)FindResource("DA_DI_BingFinance") : (DrawingImage)FindResource("DI_Sandtime");
            YandexMusic.Source = !UninstallingApps.RemovalProcess["Yandex.Music"] ? UninstallingApps.ListApps.Contains("Yandex.Music") ? (DrawingImage)FindResource("A_DI_YandexMusic") : (DrawingImage)FindResource("DA_DI_YandexMusic") : (DrawingImage)FindResource("DI_Sandtime");
            Netflix.Source = !UninstallingApps.RemovalProcess["Netflix"] ? UninstallingApps.ListApps.Contains("Netflix") ? (DrawingImage)FindResource("A_DI_Netflix") : (DrawingImage)FindResource("DA_DI_Netflix") : (DrawingImage)FindResource("DI_Sandtime");
            Outlook.Source = !UninstallingApps.RemovalProcess["Outlook"] ? UninstallingApps.ListApps.Contains("Microsoft.OutlookForWindows") ? (DrawingImage)FindResource("A_DI_Outlook") : (DrawingImage)FindResource("DA_DI_Outlook") : (DrawingImage)FindResource("DI_Sandtime");
            QuickAssist.Source = !UninstallingApps.RemovalProcess["QuickAssist"] ? UninstallingApps.ListApps.Contains("MicrosoftCorporationII.QuickAssist") ? (DrawingImage)FindResource("A_DI_QuickAssist") : (DrawingImage)FindResource("DA_DI_QuickAssist") : (DrawingImage)FindResource("DI_Sandtime");
        }
    }
}
