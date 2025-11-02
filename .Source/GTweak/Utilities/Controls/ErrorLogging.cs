using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GTweak.Utilities.Configuration;
using GTweak.Utilities.Helpers;

namespace GTweak.Utilities.Controls
{
    internal static class ErrorLogging
    {
        [Conditional("DEBUG")]
        internal static void LogDebug(Exception ex, [CallerMemberName] string memberName = "") => Debug.WriteLine($"Debug: {ex.Message}\nStack Trace: {ex.StackTrace}\nMember: {memberName}\n");

        internal static void LogWritingFile(Exception ex, [CallerMemberName] string memberName = "") => Task.Run(() => LogToFile(ex, memberName)).Wait();

        private static async Task EnsureAssociation()
        {
            try
            {
                string assocLogFile = await CommandExecutor.GetCommandOutput("/c assoc .log", false);

                if (assocLogFile.IndexOf("=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    string assocTxtFile = await CommandExecutor.GetCommandOutput("/c assoc .txt", false);

                    if (assocTxtFile.IndexOf("=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        CommandExecutor.RunCommand($"/c assoc .log={assocTxtFile.Split('=')[1].Trim()}");
                    }
                }
            }
            catch (Exception fileEx) { LogDebug(fileEx); }
        }

        private static async Task LogToFile(Exception ex, string memberName)
        {
            try
            {
                using (FileStream stream = new FileStream(PathLocator.Files.ErrorLog, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    string headerLine = "---------------------------------------------------------";
                    Exception currentEx = ex;
                    byte exLevel = 1;

                    await writer.WriteLineAsync($"GTweak has crashed!\n{headerLine}\nIf you wish to report this, please open an issue here:\nhttps://github.com/Greedeks/GTweak/issues\n{headerLine}\n");
                    await writer.WriteLineAsync($"{headerLine}\n[{DateTime.Now}]\nOS: {HardwareData.OS.Name}\n{headerLine}\n");

                    while (currentEx != null)
                    {
                        await writer.WriteLineAsync($"Exception Level: {exLevel}");

                        if (exLevel == 1)
                        {
                            await writer.WriteLineAsync($"Member: {memberName}");
                        }

                        await writer.WriteLineAsync($"Type: {currentEx.GetType().FullName}");
                        await writer.WriteLineAsync($"Error: {currentEx.Message}");
                        await writer.WriteLineAsync($"Stack Trace:\n{currentEx.StackTrace}");
                        await writer.WriteLineAsync($"\n{headerLine}\n");

                        currentEx = currentEx.InnerException;
                        exLevel++;
                    }

                    await writer.FlushAsync();
                }

                await EnsureAssociation();

                if (File.Exists(PathLocator.Files.ErrorLog))
                {
                    try
                    {
                        await Task.Delay(50);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = PathLocator.Files.ErrorLog,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception processEx) { LogDebug(processEx); }
                }
            }
            catch (Exception fileEx) { LogDebug(fileEx); }
        }
    }
}