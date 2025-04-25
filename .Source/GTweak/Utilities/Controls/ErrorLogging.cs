using GTweak.Utilities.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                        CommandExecutor.RunCommand($"/c assoc .log={assocTxtFile.Split('=')[1].Trim()}");
                }
            }
            catch (Exception fileEx) { LogDebug(fileEx); }
        }

        private static async Task LogToFile(Exception ex, string memberName)
        {
            try
            {
                using (FileStream stream = new FileStream(StoragePaths.LogFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    await writer.WriteLineAsync($"[{DateTime.Now}]\nMember: {memberName}\nError: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n");
                    await writer.FlushAsync();
                }

                await EnsureAssociation();

                if (File.Exists(StoragePaths.LogFile))
                {
                    try
                    {
                        await Task.Delay(50);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = StoragePaths.LogFile,
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