using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using GTweak.Modules.Managers;
using GTweak.Modules.Storage;
using GTweak.Windows;
using Microsoft.Win32;

namespace GTweak.Modules.Maintenance
{
    internal sealed class WinLicenseHandler : WinKeyStorage
    {
        internal static bool IsWindowsActivated = false;

        private static readonly byte[] _clientLockboxKey = new byte[]
        {
            0x07,0x02,0x00,0x00,0x00,0xA4,0x00,0x00,0x52,0x53,0x41,0x32,0x00,0x04,0x00,0x00,
            0x01,0x00,0x01,0x00,0x29,0x87,0xBA,0x3F,0x52,0x90,0x57,0xD8,0x12,0x26,0x6B,0x38,
            0xB2,0x3B,0xF9,0x67,0x08,0x4F,0xDD,0x8B,0xF5,0xE3,0x11,0xB8,0x61,0x3A,0x33,0x42,
            0x51,0x65,0x05,0x86,0x1E,0x00,0x41,0xDE,0xC5,0xDD,0x44,0x60,0x56,0x3D,0x14,0x39,
            0xB7,0x43,0x65,0xE9,0xF7,0x2B,0xA5,0xF0,0xA3,0x65,0x68,0xE9,0xE4,0x8B,0x5C,0x03,
            0x2D,0x36,0xFE,0x28,0x4C,0xD1,0x3C,0x3D,0xC1,0x90,0x75,0xF9,0x6E,0x02,0xE0,0x58,
            0x97,0x6A,0xCA,0x80,0x02,0x42,0x3F,0x6C,0x15,0x85,0x4D,0x83,0x23,0x6A,0x95,0x9E,
            0x38,0x52,0x59,0x38,0x6A,0x99,0xF0,0xB5,0xCD,0x53,0x7E,0x08,0x7C,0xB5,0x51,0xD3,
            0x8F,0xA3,0x0D,0xA0,0xFA,0x8D,0x87,0x3C,0xFC,0x59,0x21,0xD8,0x2E,0xD9,0x97,0x8B,
            0x40,0x60,0xB1,0xD7,0x2B,0x0A,0x6E,0x60,0xB5,0x50,0xCC,0x3C,0xB1,0x57,0xE4,0xB7,
            0xDC,0x5A,0x4D,0xE1,0x5C,0xE0,0x94,0x4C,0x5E,0x28,0xFF,0xFA,0x80,0x6A,0x13,0x53,
            0x52,0xDB,0xF3,0x04,0x92,0x43,0x38,0xB9,0x1B,0xD9,0x85,0x54,0x7B,0x14,0xC7,0x89,
            0x16,0x8A,0x4B,0x82,0xA1,0x08,0x02,0x99,0x23,0x48,0xDD,0x75,0x9C,0xC8,0xC1,0xCE,
            0xB0,0xD7,0x1B,0xD8,0xFB,0x2D,0xA7,0x2E,0x47,0xA7,0x18,0x4B,0xF6,0x29,0x69,0x44,
            0x30,0x33,0xBA,0xA7,0x1F,0xCE,0x96,0x9E,0x40,0xE1,0x43,0xF0,0xE0,0x0D,0x0A,0x32,
            0xB4,0xEE,0xA1,0xC3,0x5E,0x9B,0xC7,0x7F,0xF5,0x9D,0xD8,0xF2,0x0F,0xD9,0x8F,0xAD,
            0x75,0x0A,0x00,0xD5,0x25,0x43,0xF7,0xAE,0x51,0x7F,0xB7,0xDE,0xB7,0xAD,0xFB,0xCE,
            0x83,0xE1,0x81,0xFF,0xDD,0xA2,0x77,0xFE,0xEB,0x27,0x1F,0x10,0xFA,0x82,0x37,0xF4,
            0x7E,0xCC,0xE2,0xA1,0x58,0xC8,0xAF,0x1D,0x1A,0x81,0x31,0x6E,0xF4,0x8B,0x63,0x34,
            0xF3,0x05,0x0F,0xE1,0xCC,0x15,0xDC,0xA4,0x28,0x7A,0x9E,0xEB,0x62,0xD8,0xD8,0x8C,
            0x85,0xD7,0x07,0x87,0x90,0x2F,0xF7,0x1C,0x56,0x85,0x2F,0xEF,0x32,0x37,0x07,0xAB,
            0xB0,0xE6,0xB5,0x02,0x19,0x35,0xAF,0xDB,0xD4,0xA2,0x9C,0x36,0x80,0xC6,0xDC,0x82,
            0x08,0xE0,0xC0,0x5F,0x3C,0x59,0xAA,0x4E,0x26,0x03,0x29,0xB3,0x62,0x58,0x41,0x59,
            0x3A,0x37,0x43,0x35,0xE3,0x9F,0x34,0xE2,0xA1,0x04,0x97,0x12,0x9D,0x8C,0xAD,0xF7,
            0xFB,0x8C,0xA1,0xA2,0xE9,0xE4,0xEF,0xD9,0xC5,0xE5,0xDF,0x0E,0xBF,0x4A,0xE0,0x7A,
            0x1E,0x10,0x50,0x58,0x63,0x51,0xE1,0xD4,0xFE,0x57,0xB0,0x9E,0xD7,0xDA,0x8C,0xED,
            0x7D,0x82,0xAC,0x2F,0x25,0x58,0x0A,0x58,0xE6,0xA4,0xF4,0x57,0x4B,0xA4,0x1B,0x65,
            0xB9,0x4A,0x87,0x46,0xEB,0x8C,0x0F,0x9A,0x48,0x90,0xF9,0x9F,0x76,0x69,0x03,0x72,
            0x77,0xEC,0xC1,0x42,0x4C,0x87,0xDB,0x0B,0x3C,0xD4,0x74,0xEF,0xE5,0x34,0xE0,0x32,
            0x45,0xB0,0xF8,0xAB,0xD5,0x26,0x21,0xD7,0xD2,0x98,0x54,0x8F,0x64,0x88,0x20,0x2B,
            0x14,0xE3,0x82,0xD5,0x2A,0x4B,0x8F,0x4E,0x35,0x20,0x82,0x7E,0x1B,0xFE,0xFA,0x2C,
            0x79,0x6C,0x6E,0x66,0x94,0xBB,0x0A,0xEB,0xBA,0xD9,0x70,0x61,0xE9,0x47,0xB5,0x82,
            0xFC,0x18,0x3C,0x66,0x3A,0x09,0x2E,0x1F,0x61,0x74,0xCA,0xCB,0xF6,0x7A,0x52,0x37,
            0x1D,0xAC,0x8D,0x63,0x69,0x84,0x8E,0xC7,0x70,0x59,0xDD,0x2D,0x91,0x1E,0xF7,0xB1,
            0x56,0xED,0x7A,0x06,0x9D,0x5B,0x33,0x15,0xDD,0x31,0xD0,0xE6,0x16,0x07,0x9B,0xA5,
            0x94,0x06,0x7D,0xC1,0xE9,0xD6,0xC8,0xAF,0xB4,0x1E,0x2D,0x88,0x06,0xA7,0x63,0xB8,
            0xCF,0xC8,0xA2,0x6E,0x84,0xB3,0x8D,0xE5,0x47,0xE6,0x13,0x63,0x8E,0xD1,0x7F,0xD4,
            0x81,0x44,0x38,0xBF
        };

        private static string GetEditionKey(Dictionary<string, string> keys) => keys.TryGetValue(Regex.Replace(RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", string.Empty), @"^Windows\s+(\d+\s+)?", string.Empty, RegexOptions.IgnoreCase).Trim(), out string key) ? key : string.Empty;

        private static string Slmgr(string args) => $@"/c cscript //nologo //b %windir%\system32\slmgr.vbs {args}";

        internal static async Task VerifyActivation()
        {
            try
            {
                using ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\cimv2", "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID = '55c92734-d682-4d71-983e-d6ec3f16059f' and LicenseStatus = 1");
                foreach (ManagementObject managementObj in searcher.Get().Cast<ManagementObject>())
                {
                    using (managementObj)
                    {
                        IsWindowsActivated = (uint)managementObj["LicenseStatus"] == 1;
                    }
                }
            }
            catch (COMException)
            {
                try
                {
                    string output = await CommandExecutor.GetCommandOutput("cscript //b ($env:windir + '\\system32\\slmgr.vbs') /ato; $code = $LASTEXITCODE; ($code -eq 0 -or $code -eq -2147024773)", true);
                    bool.TryParse(output, out IsWindowsActivated);
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        internal static async Task StartActivation()
        {
            string hwidKey = GetEditionKey(keysHWID);
            string kmsKey = GetEditionKey(keysKMS);

            if (string.IsNullOrEmpty(hwidKey) && string.IsNullOrEmpty(kmsKey))
            {
                NotificationManager.Warn("keynotfound_noty").WithDelay(300).Perform();
                return;
            }

            NotificationManager.Warn("win_activate_noty").Perform();

            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.Show();

            try
            {
                if (!string.IsNullOrEmpty(hwidKey))
                {
                    await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr($"/ipk {hwidKey}"));

                    await CommandExecutor.InvokeRunCommandAsTrustedInstaller($@"/c del /f /q {PathTargets.Folders.SystemDrive}ProgramData\Microsoft\Windows\ClipSVC\GenuineTicket\*.xml & del /f /q {PathTargets.Folders.SystemDrive}ProgramData\Microsoft\Windows\ClipSVC\Install\Migration\*.xml");

                    string productPfn = RegistryHelper.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\ProductOptions", "OSProductPfn", string.Empty);
                    string originalGeo = RegistryHelper.GetValue(@"HKEY_CURRENT_USER\Control Panel\International\Geo", "Name", CultureInfo.InstalledUICulture.Name.Split('-')[1].ToUpperInvariant());
                    RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", "US", RegistryValueKind.String);

                    XDocument ticketsDoc = XDocument.Parse(Properties.Resources.Tickets);
                    XElement foundTicket = FindTicket(ticketsDoc, productPfn);

                    if (foundTicket != null)
                    {
                        XDocument genuineXml = XDocument.Parse(foundTicket.Element("content")?.Value.Trim());
                        genuineXml.Save(PathTargets.Files.GenuineTicket);

                        await CommandExecutor.InvokeRunCommandAsTrustedInstaller("clipup -v -o", true);
                        await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr("/ato"));

                        await VerifyActivation();
                    }

                    if (IsWindowsActivated == false)
                    {
                        GenerateTicket(productPfn);

                        await CommandExecutor.InvokeRunCommandAsTrustedInstaller("clipup -v -o", true);
                        await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr("/ato"));

                        await VerifyActivation();
                    }

                    RegistryHelper.Write(Registry.CurrentUser, @"Control Panel\International\Geo", "Name", originalGeo, RegistryValueKind.String);

                    if (IsWindowsActivated)
                    {
                        NotificationManager.Info("success_activate_noty").WithDelay(300).Restart();
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(kmsKey))
                {
                    await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr($"/ipk {kmsKey}"));
                    await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr("/skms kms.digiboy.ir"));
                    await CommandExecutor.InvokeRunCommandAsTrustedInstaller(Slmgr("/ato"));

                    await VerifyActivation();
                }

                NotificationManager.Info(IsWindowsActivated ? "success_activate_noty" : "error_activate_noty").WithDelay(300).Perform(IsWindowsActivated ? NotificationManager.AlertType.Restart : default);
            }
            catch { NotificationManager.Warn("error_activate_noty").WithDelay(300).Perform(); }
            finally { overlayWindow.Close(); }
        }

        private static XElement FindTicket(XDocument ticketsDoc, string productPfn)
        {
            string pfn = productPfn.StartsWith("Microsoft.Windows.", StringComparison.OrdinalIgnoreCase) ? productPfn.Substring("Microsoft.Windows.".Length) : productPfn;

            int underscore = pfn.IndexOf('_');
            if (underscore >= 0)
            {
                pfn = pfn.Substring(0, underscore);
            }

            int dash = pfn.LastIndexOf('-');
            string edition = dash < 0 ? pfn : pfn.Substring(0, dash);
            string build = dash < 0 ? string.Empty : pfn.Substring(dash + 1);

            List<XElement> tickets = ticketsDoc.Descendants("Ticket").Where(ticket => ticket.Element("product")?.Value.StartsWith(edition + "-", StringComparison.OrdinalIgnoreCase) == true).ToList();

            if (tickets.Count == 0)
            {
                return null;
            }

            if (tickets.Count == 1)
            {
                return tickets[0];
            }

            return tickets.FirstOrDefault(ticket => ticket.Element("product").Value.EndsWith("-" + build, StringComparison.OrdinalIgnoreCase));
        }

        private static void GenerateTicket(string osProductPfn)
        {
            try
            {
                string sessionId = $"OSMajorVersion=5;OSMinorVersion=1;OSPlatformId=2;PP=0;Pfn={osProductPfn};PKeyIID=221306452340115677963964261259250411589493550039199940431586886;";
                string signedProperties = $"OA3xOriginalProductId=;OA3xOriginalProductKey=;SessionId={Convert.ToBase64String(Encoding.Unicode.GetBytes(sessionId + "\0"))};TimeStampClient=2022-10-11T12:00:00Z";

                using RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportCspBlob(_clientLockboxKey);
                rsa.PersistKeyInCsp = false;

                string xml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><genuineAuthorization xmlns=\"http://www.microsoft.com/DRM/SL/GenuineAuthorization/1.0\"><version>1.0</version><genuineProperties origin=\"sppclient\"><properties>{signedProperties}</properties><signatures><signature name=\"clientLockboxKey\" method=\"rsa-sha256\">{SignProperties(signedProperties, rsa)}</signature></signatures></genuineProperties></genuineAuthorization>";
                File.WriteAllText(PathTargets.Files.GenuineTicket, xml, Encoding.ASCII);
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }
        }

        private static string SignProperties(string properties, RSACryptoServiceProvider rsa)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] signature = rsa.SignHash(sha256.ComputeHash(Encoding.UTF8.GetBytes(properties)), CryptoConfig.MapNameToOID("SHA256"));
            return Convert.ToBase64String(signature);
        }
    }
}
