using GTweak.Utilities.Configuration;
using GTweak.Utilities.Control;
using GTweak.Windows;
using System.Collections.Generic;
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

        private static readonly Dictionary<(string pattern, byte words), string> windowsKeys = new Dictionary<(string pattern, byte words), string>
        {
            { ("Home|Single|Language", 3), @"7HNRX-D7KGG-3K4RQ-4WPJ4-YTDFH" },
            { ("Home", 1), @"TX9XD-98N7V-6WMQ6-BX7FG-H8Q99" },
            { ("Education", 1), @"NW6C2-QMPVW-D7KKK-3GKT6-VCFB2" },
            { ("Enterprise|LSTB", 2), @"7YMNV-PG77F-K66KT-KG9VQ-TCQGB" },
            { ("Enterprise|N|LTSC", 3), @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H" },
            { ("Enterprise|N", 2), @"DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4" },
            { ("Enterprise|G", 2), @"YYVX9-NTFWV-6MDM3-9PT4T-4M68B" },
            { ("Enterprise", 1), @"ND4DX-39KJY-FYWQ9-X6XKT-VCFCF" },
            { ("Core|Single|Language", 3), @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT" },
            { ("Core", 1), @"KTNPV-KTRK4-3RRR8-39X6W-W44T3" },
            { ("Pro", 1), @"W269N-WFGWX-YVC9B-4J6C9-T83GX" }
        };

        private static bool IsVersionWindows(string pattern, byte words) => new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled).Matches(SystemDiagnostics.WindowsClientVersion).Count == words;

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

            string keyWindow = string.Empty, kmsArguments = string.Empty;

            foreach (var key in windowsKeys)
            {
                if (IsVersionWindows(key.Key.pattern, key.Key.words))
                {
                    keyWindow = key.Value;
                    kmsArguments = key.Key.pattern == "Pro" ? @"/c slmgr.vbs //b /skms kms.digiboy.ir" : @"/c slmgr.vbs //b /skms kms.xspace.in";
                    break;
                }
            }

            Process cmdProcess = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            async Task RunCommand(string arguments, int delay)
            {
                cmdProcess.StartInfo.Arguments = arguments;
                cmdProcess.Start();
                await Task.Delay(delay);
            }

            using (cmdProcess)
            {
                waitingWindow.Show();

                if (SystemDiagnostics.WindowsClientVersion.Contains("10"))
                    await RunCommand("/c assoc .vbs=VBSFile", 500);

                await RunCommand($"/c slmgr.vbs //b /ipk {keyWindow}", 4000);
                await RunCommand(kmsArguments, 7000);
                await RunCommand("/c slmgr.vbs //b /ato", 3500);

                new WindowsLicense().LicenseStatus();

                await Task.Delay(2000);

                waitingWindow.Close();

                if (IsWindowsActivated)
                    new ViewNotification(300).Show("restart", "warn", (string)Application.Current.Resources["successactivate_notification"]);
                else
                    new ViewNotification(300).Show("", "warn", (string)Application.Current.Resources["notsuccessactivate_notification"]);
            }

        }

    }
}
