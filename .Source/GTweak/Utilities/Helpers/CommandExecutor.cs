using System;
using System.Diagnostics;
using System.Text;
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
                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    ErrorLogging.LogDebug(ex);
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
            if (process.HasExited)
            {
                return Task.CompletedTask;
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}