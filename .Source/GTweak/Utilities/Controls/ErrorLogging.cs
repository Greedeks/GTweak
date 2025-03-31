using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GTweak.Utilities.Controls
{
    internal class ErrorLogging
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTweak_Error.log");

        internal static void LogWritingFile(Exception ex, [CallerMemberName] string memberName = "") => Task.Run(() => LogToFile(ex, memberName)).Wait();

        internal static void LogDebug(Exception ex, [CallerMemberName] string memberName = "") => Debug.WriteLine($"Debug: {ex.Message}\nStack Trace: {ex.StackTrace}\nMember: {memberName}\n");

        internal static async Task LogToFile(Exception ex, string memberName)
        {
            try
            {
                using var stream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                await writer.WriteLineAsync($"[{DateTime.Now}]\nMember: {memberName}\nError: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n");
                await writer.FlushAsync();
                Process.Start(new ProcessStartInfo
                {
                    FileName = logFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception fileEx)
            {
                LogDebug(fileEx);
            }
        }
    }
}