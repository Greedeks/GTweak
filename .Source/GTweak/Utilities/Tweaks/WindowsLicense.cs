using GTweak.Windows;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class WindowsLicense
    {
        internal static bool IsWindowsActivated = false;
        private static bool IsVersionWindows(string pattern, byte words) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled).Matches(SystemData.СomputerСonfiguration.WindowsClientVersion).Count == words;

        internal void LicenseStatus()
        {
            Parallel.Invoke(() =>
            {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' and LicenseStatus = 1").Get())
                    IsWindowsActivated = (uint)managementObj["LicenseStatus"] == 1;
            });
        }

        internal static async void StartActivation()
        {
            WaitingWindow waitingWindow = new WaitingWindow();

            string keyWindow = string.Empty, kmsArguments = @"/c slmgr.vbs //b /skms kms.xspace.in";

            if (IsVersionWindows("Home|Single|Language", 3))
                keyWindow = @"7HNRX-D7KGG-3K4RQ-4WPJ4-YTDFH";
            else if (IsVersionWindows("Home", 1))
                keyWindow = @"TX9XD-98N7V-6WMQ6-BX7FG-H8Q99";
            else if (IsVersionWindows("Education", 1))
                keyWindow = @"NW6C2-QMPVW-D7KKK-3GKT6-VCFB2";
            else if (IsVersionWindows("Enterprise|LSTB", 2))
                keyWindow = @"7YMNV-PG77F-K66KT-KG9VQ-TCQGB";
            else if (IsVersionWindows("Enterprise|N|LTSC", 3))
                keyWindow = @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H";
            else if (IsVersionWindows("Enterprise|N", 2))
                keyWindow = @"DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4";
            else if (IsVersionWindows("Enterprise|G", 2))
                keyWindow = @"YYVX9-NTFWV-6MDM3-9PT4T-4M68B";
            else if (IsVersionWindows("Enterprise", 1))
                keyWindow = @"ND4DX-39KJY-FYWQ9-X6XKT-VCFCF";
            else if (IsVersionWindows("Core|Single|Language", 3))
                keyWindow = @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT";
            else if (IsVersionWindows("Core", 1))
                keyWindow = @"KTNPV-KTRK4-3RRR8-39X6W-W44T3";
            else if (IsVersionWindows("Pro", 1))
            {
                keyWindow = @"W269N-WFGWX-YVC9B-4J6C9-T83GX";
                kmsArguments = @"/c slmgr.vbs //b /skms kms.digiboy.ir";
            }

            Process cmdProcess = new Process()
            {
                StartInfo = {
                        FileName = "cmd.exe",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    },
            };

            using (cmdProcess)
            {
                waitingWindow.Show();

                if (SystemData.СomputerСonfiguration.WindowsClientVersion.Contains("10"))
                {
                    cmdProcess.StartInfo.Arguments = $"/c assoc .vbs=VBSFile";
                    cmdProcess.Start();
                    await Task.Delay(200);
                }

                cmdProcess.StartInfo.Arguments = $"/c slmgr.vbs //b /ipk {keyWindow}";
                cmdProcess.Start();

                await Task.Delay(4000);

                cmdProcess.StartInfo.Arguments = kmsArguments;
                cmdProcess.Start();

                await Task.Delay(7000);

                cmdProcess.StartInfo.Arguments = "/c slmgr.vbs //b /ato";
                cmdProcess.Start();

                await Task.Delay(3500);

                new WindowsLicense().LicenseStatus();

                await Task.Delay(1500);

                waitingWindow.Close();

                if (IsWindowsActivated)
                    new ViewNotification(300).Show("restart", "warn", (string)Application.Current.Resources["successactivate_notification"]);
                else
                    new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["notsuccessactivate_notification"]);
            }

        }

    }
}
