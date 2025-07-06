using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal static class CommandExecutor
    {
        internal static int PID = 0;

        private static readonly string _cmdPath = Path.Combine(Environment.SystemDirectory, "cmd.exe");
        private static readonly string _powerShellPath = Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe");

        internal static async Task<string> GetCommandOutput(string command, bool isPowerShell = true)
        {
            return await Task.Run(async () =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = isPowerShell ? _powerShellPath : _cmdPath,
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process process = new Process { StartInfo = startInfo };
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                await process.WaitForExitAsync().ConfigureAwait(false);

                if (process.ExitCode == 0)
                    return string.Join(Environment.NewLine, output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                else
                {
                    Debug.WriteLine($"{process.ExitCode}: {error}");
                    return string.Empty;
                }
            });
        }

        internal static void RunCommandAsTrustedInstaller(string command, bool isPowerShell = false) =>
            TrustedInstaller.CreateProcessAsTrustedInstaller(PID, isPowerShell ? $"{_powerShellPath} -NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command.Replace("\"", "`\"")}\"" : $"{_cmdPath} {command}");

        internal static async void RunCommand(string command, bool isPowerShell = false)
        {
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = isPowerShell ? _powerShellPath : _cmdPath,
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true
                });
            });
        }

        internal static async Task InvokeRunCommand(string command, bool isPowerShell = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = isPowerShell ? _powerShellPath : _cmdPath,
                Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{command}\"" : command,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            await process.WaitForExitAsync().ConfigureAwait(false);
        }

        internal static Task WaitForExitAsync(this Process process)
        {
            if (process.HasExited)
                return Task.CompletedTask;

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            return tcs.Task;
        }
    }
}