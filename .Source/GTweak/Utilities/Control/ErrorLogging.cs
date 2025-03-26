using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GTweak.Utilities.Control
{
    internal class ErrorLogging
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GTweak_Error.log");

        internal static void LogWritingFile(Exception ex) => Task.Run(() => LogToFile(ex)).Wait();

        internal static void LogDebug(Exception ex) => Debug.WriteLine($"Debug: {ex.Message}\nStack Trace: {ex.StackTrace}");

        internal static async Task LogToFile(Exception ex)
        {
            try
            {
                using var stream = new FileStream(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                await writer.WriteLineAsync($"[{DateTime.Now}] Error: {ex.Message}\nStack Trace:\n{ex.StackTrace}\n");

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