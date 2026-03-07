using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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

        internal static void LogWritingFile(Exception ex, bool isFatal = false, [CallerMemberName] string memberName = "") => Task.Run(() => SaveDumpToFile(ex, isFatal, memberName)).Wait();

        private static async Task EnsureLogFileAssociation()
        {
            try
            {
                string logAssocOutput = await CommandExecutor.GetCommandOutput("/c assoc .log", false);

                if (logAssocOutput.IndexOf("=", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    string txtAssocOutput = await CommandExecutor.GetCommandOutput("/c assoc .txt", false);

                    if (txtAssocOutput.IndexOf("=", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string txtApp = txtAssocOutput.Split('=')[1].Trim();
                        CommandExecutor.RunCommand($"/c assoc .log={txtApp}");
                    }
                }
            }
            catch (Exception ex) { LogDebug(ex); }
        }

        private static async Task SaveDumpToFile(Exception ex, bool isFatal, string memberName)
        {
            try
            {
                using (FileStream stream = new FileStream(PathLocator.Files.ErrorLog, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    string separator = "---------------------------------------------------------";
                    Exception currentEx = ex;
                    byte exLevel = 1;

                    await writer.WriteLineAsync($"{(isFatal ? "GTweak has crashed!" : "GTweak has encountered an error.")}\n{separator}\nIf you wish to report this, please open an issue here:\n{PathLocator.Links.GitHub}/GTweak/issues\n{separator}\n");
                    await writer.WriteLineAsync($"{separator}\n[{DateTime.Now}]\nOS: {(string.IsNullOrEmpty(HardwareData.OS?.Name) ? "Unknown" : HardwareData.OS.Name)}\nVersion: {(string.IsNullOrEmpty(HardwareData.OS?.Version) ? "Unknown" : HardwareData.OS.Version)}\nRelease: {SettingsEngine.currentRelease}\n{separator}\n");

                    while (currentEx != null)
                    {
                        await writer.WriteLineAsync($"Exception Level: {exLevel}");

                        if (exLevel == 1)
                        {
                            await writer.WriteLineAsync($"Member: {memberName}");
                        }

                        await writer.WriteLineAsync($"Type: {currentEx.GetType().FullName}");
                        await writer.WriteLineAsync($"Error: {currentEx.Message}");

                        StackTrace stackTrace = new StackTrace(currentEx, true);

                        if (stackTrace.FrameCount > 0)
                        {
                            StackFrame frame = stackTrace.GetFrame(0);

                            if (frame?.GetMethod() is MethodBase method)
                            {
                                await writer.WriteLineAsync($"Class: {method.DeclaringType?.FullName}\nMethod: {method.Name}");

                                int fileLine = frame.GetFileLineNumber();
                                if (fileLine > 0)
                                {
                                    await writer.WriteLineAsync($"Line: {fileLine}");
                                }

                                ParameterInfo[] parameters = method.GetParameters();
                                if (parameters.Length > 0)
                                {
                                    await writer.WriteLineAsync("Parameters:");

                                    foreach (ParameterInfo param in parameters)
                                    {
                                        await writer.WriteLineAsync($"  - Name: {param.Name} | Type: {param.ParameterType}");
                                    }
                                }
                            }
                        }

                        await writer.WriteLineAsync($"Stack Trace:\n{currentEx.StackTrace}");
                        await writer.WriteLineAsync($"\n{separator}\n");

                        currentEx = currentEx.InnerException;
                        exLevel++;
                    }

                    await writer.FlushAsync();
                }

                await EnsureLogFileAssociation();

                if (File.Exists(PathLocator.Files.ErrorLog))
                {
                    try
                    {
                        await Task.Delay(50);
                        Process.Start(new ProcessStartInfo { FileName = PathLocator.Files.ErrorLog, UseShellExecute = true });
                    }
                    catch (Exception processEx) { LogDebug(processEx); }
                }
            }
            catch (Exception fileEx) { LogDebug(fileEx); }
        }
    }
}