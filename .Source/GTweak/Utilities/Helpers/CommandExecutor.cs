using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GTweak.Utilities.Helpers
{
    internal static class CommandExecutor
    {
        internal static async Task<string> GetCommandOutput(string arguments, bool isPowerShell = true)
        {
            return await Task.Run(async () =>
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = isPowerShell ? "powershell.exe" : "cmd.exe",
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{arguments}\"" : arguments,
                    Verb = "runas",
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

        internal static async void RunCommand(string arguments, bool isPowerShell = false)
        {
            await Task.Run(() =>
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = isPowerShell ? "powershell.exe" : "cmd.exe",
                    Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{arguments}\"" : arguments,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            });
        }

        internal static async Task InvokeRunCommand(string arguments, bool isPowerShell = false)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = isPowerShell ? "powershell.exe" : "cmd.exe",
                Arguments = isPowerShell ? $"-NoLogo -NonInteractive -NoProfile -ExecutionPolicy Bypass -Command \"{arguments}\"" : arguments,
                Verb = "runas",
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
