using GTweak.Windows;
using System.Diagnostics;
using System.Management;
using System.Threading.Tasks;
using System.Windows;

namespace GTweak.Utilities.Tweaks
{
    internal sealed class WindowsLicense
    {
        internal static uint statusLicense = 0;

        internal void LicenseStatus()
        {
            Parallel.Invoke(() => {
                foreach (var managementObj in new ManagementObjectSearcher(@"root\cimv2", "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' and LicenseStatus = 1").Get())
                    statusLicense = (uint)managementObj["LicenseStatus"];
            });
        }

        internal static async void StartActivation()
        {
            WaitingWindow waitingWindow = new WaitingWindow();

            string keyWindow = string.Empty, kmsArguments = @"/c slmgr.vbs //b /skms kms.xspace.in";

            if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Home") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("Single") &
                SystemData.СomputerСonfiguration.clientWinVersion.Contains("Language"))
                keyWindow = @"7HNRX-D7KGG-3K4RQ-4WPJ4-YTDFH";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Home"))
                keyWindow = @"TX9XD-98N7V-6WMQ6-BX7FG-H8Q99";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Education"))
                keyWindow = @"NW6C2-QMPVW-D7KKK-3GKT6-VCFB2";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Enterprise") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("LSTB"))
                keyWindow = @"7YMNV-PG77F-K66KT-KG9VQ-TCQGB";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Enterprise") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("N LTSC"))
                keyWindow = @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Enterprise") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("N"))
                keyWindow = @"DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Enterprise") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("G"))
                keyWindow = @"YYVX9-NTFWV-6MDM3-9PT4T-4M68B";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Enterprise"))
                keyWindow = @"ND4DX-39KJY-FYWQ9-X6XKT-VCFCF";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Core") & SystemData.СomputerСonfiguration.clientWinVersion.Contains("Single") &
                SystemData.СomputerСonfiguration.clientWinVersion.Contains("Language"))
                keyWindow = @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Core"))
                keyWindow = @"KTNPV-KTRK4-3RRR8-39X6W-W44T3";
            else if (SystemData.СomputerСonfiguration.clientWinVersion.Contains("Pro"))
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

                cmdProcess.StartInfo.Arguments = $"/c slmgr.vbs //b " + "/ipk " + keyWindow;
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

                App.ViewLang();

                if (statusLicense == 1)
                    new ViewNotification().Show("restart", (string)Application.Current.Resources["title0_notification"], (string)Application.Current.Resources["successactivate_notification"]);
                else if (statusLicense != 1)
                    new ViewNotification().Show("", (string)Application.Current.Resources["title0_notification"], (string)Application.Current.Resources["notsuccessactivate_notification"]);
            }

        }

    }
}
