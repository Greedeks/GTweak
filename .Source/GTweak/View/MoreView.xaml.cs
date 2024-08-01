using GTweak.Utilities;
using GTweak.Utilities.Tweaks;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace GTweak.View
{
    public partial class MoreView : UserControl
    {
        public MoreView()
        {
            InitializeComponent();
        }

        private async void BtnRestorePoint_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (RecoveryPoint.IsAlreadyPoint() == false)
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("createpoint_notification"));

                    await Task.Delay(100);

                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += (_s, _e) =>
                    {
                        try { RecoveryPoint.Create((string)FindResource("textpoint_more")); }
                        catch { new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("notsuccessfulpoint_notification")); }
                        finally { new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("successpoint_notification")); };
                    };
                    backgroundWorker.RunWorkerCompleted += (_s, _e) =>
                    {
                        new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("successpoint_notification"));
                        backgroundWorker.Dispose();
                    };
                    backgroundWorker.RunWorkerAsync();
                }
                else
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("readypoint_notification"));
            }
        }

        private void BtnRecoveyLaunch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { RecoveryPoint.Run(); }
                catch { new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("notsuccessfulrecovery_notification")); }
            }
        }

        private void BtnWinLicense_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowsLicense.statusLicense == 1)
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("readyactivate_notification"));

                else if (WindowsLicense.statusLicense != 1)
                {
                    new ViewNotification().Show("", (string)FindResource("title0_notification"), (string)FindResource("activatewin_notification"));
                    WindowsLicense.StartActivation();
                }
            }
        }

        private void BtnClear_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                ClearingMemory.StartMemoryCleanup();
        }

        private void BtnDisableRecovery_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!RecoveryPoint.IsSystemRestoreDisabled())
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += (_s, _e) =>
                {
                    try { RecoveryPoint.DisablePoint(); } catch (Exception ex) { Debug.WriteLine(ex.Message.ToString()); }
                };
                backgroundWorker.RunWorkerCompleted += (_s, _e) =>
                {
                    new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("disable_recovery_notification"));
                    backgroundWorker.Dispose();
                };
                backgroundWorker.RunWorkerAsync();
            }
            else
                new ViewNotification().Show("", (string)FindResource("title1_notification"), (string)FindResource("warndisable_recovery_notification"));
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                e.Handled = true;
        }
    }
}
