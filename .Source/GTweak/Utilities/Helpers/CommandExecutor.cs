using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GTweak.Utilities.Controls;

namespace GTweak.Utilities.Helpers
{
    internal static class CommandExecutor
    {
        internal static int PID = 0;

        internal static async Task<string> GetCommandOutput(string command, bool isPowerShell = true)
        {
            return await Task.Run(async () =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = isPowerShell ? PathLocator.Executable.PowerShell : PathLocator.Executable.CommandShell,
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System)
                };

                using Process process = new Process { StartInfo = startInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                await process.WaitForExitAsync().ConfigureAwait(false);

                if (process.ExitCode == 0)
                {
                    return string.Join(Environment.NewLine, output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    Debug.WriteLine($"{process.ExitCode}: {error}");
                    return string.Empty;
                }
            });
        }

        internal static void RunCommandAsTrustedInstaller(string command, bool isPowerShell = false) =>
            TrustedInstaller.CreateProcessAsTrustedInstaller(PID, isPowerShell ? $"{PathLocator.Executable.PowerShell} -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command.Replace("\"", "`\"")}\"" : $"{PathLocator.Executable.CommandShell} {command}");

        internal static async void RunCommand(string command, bool isPowerShell = false)
        {
            await Task.Run(() =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = isPowerShell ? PathLocator.Executable.PowerShell : PathLocator.Executable.CommandShell,
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = startInfo };
                try { process.Start(); }
                catch (Exception ex) { ErrorLogging.LogDebug(ex); }
            }).ConfigureAwait(false);
        }

        internal static async void RunCommandShow(string fileName, string arguments = "", bool isElevationRequired = false)
        {
            await Task.Run(() =>
            {
                if (isElevationRequired)
                {
                    TrustedInstaller.CreateProcessAsTrustedInstaller(PID, $"{fileName} {arguments}", true);
                }
                else
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        WindowStyle = ProcessWindowStyle.Normal,
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = false
                    };

                    using Process process = new Process() { StartInfo = startInfo };
                    try { process.Start(); }
                    catch (Exception ex) { ErrorLogging.LogDebug(ex); }
                }
            }).ConfigureAwait(false);
        }

        internal static async Task InvokeRunCommand(string command, bool isPowerShell = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = isPowerShell ? PathLocator.Executable.PowerShell : PathLocator.Executable.CommandShell,
                Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new Process { StartInfo = startInfo };
            try
            {
                process.Start();
                await process.WaitForExitAsync().ConfigureAwait(false);
            }
            catch (Exception ex) { ErrorLogging.LogDebug(ex); }
        }

        internal static Task WaitForExitAsync(this Process process)
        {
            if (!process.HasExited)
            {
                var tcs = new TaskCompletionSource<object>();

                void Handler(object s, EventArgs e) => tcs.TrySetResult(null);

                process.EnableRaisingEvents = true;
                process.Exited += Handler;

                tcs.Task.ContinueWith(_ => process.Exited -= Handler, TaskScheduler.Default);

                if (process.HasExited)
                {
                    tcs.TrySetResult(null);
                }

                return tcs.Task;
            }

            return Task.CompletedTask;
        }


        internal static string CleanCommand(string rawCommand)
        {
            if (string.IsNullOrWhiteSpace(rawCommand))
            {
                return string.Empty;
            }

            var lines = string.IsNullOrEmpty(rawCommand) ? new List<string>() : rawCommand.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).Select(line => Regex.Replace(line, @"\s+", " ")).ToList();

            if (lines.Count == 0)
            {
                return "";
            }

            if (lines.Count == 1)
            {
                return lines[0];
            }

            string separator = rawCommand?.Contains("&&") == true ? " && " : rawCommand?.Contains("&") == true ? " & " : " && ";

            return string.Join(separator, lines);
        }
    }
}