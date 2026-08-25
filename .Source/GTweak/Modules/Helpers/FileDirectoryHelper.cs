using System;
using System.IO;
using System.Threading;
using GTweak.Modules.Common;

namespace GTweak.Modules.Helpers
{
    internal static class FileDirectoryHelper
    {
        internal static void CreateDirectory(params string[] paths)
        {
            if (paths != null && paths.Length != 0)
            {
                foreach (string path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path) || Directory.Exists(path))
                    {
                        continue;
                    }

                    try { Directory.CreateDirectory(path); }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                }
            }
        }

        internal static void DeleteDirectory(params string[] paths)
        {
            if (paths != null && paths.Length != 0)
            {
                foreach (string path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    {
                        continue;
                    }

                    try { Directory.Delete(path, true); }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                }
            }
        }

        internal static void ForceDeleteDirectory(params string[] paths)
        {
            if (paths != null && paths.Length != 0)
            {
                foreach (string path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                    {
                        continue;
                    }

                    UnlockHandleHelper.UnlockDirectory(path);

                    CommandExecutor.RunCommandAsTrustedInstaller($@"/c takeown /f ""{path}"" /r /d y && icacls ""{path}"" /inheritance:r && icacls ""{path}"" /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18 && icacls ""{path}"" /grant ""{Environment.UserName}"":F /t && rd /s /q ""{path}""");

                    for (int i = 0; Directory.Exists(path) && i < 5; i++)
                    {
                        try
                        {
                            Directory.Delete(path, true);
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                        if (Directory.Exists(path))
                        {
                            CommandExecutor.RunCommand($"Remove-Item -LiteralPath '{path}' -Recurse -Force", true);
                            Thread.Sleep(300);
                        }
                    }
                }
            }
        }

        internal static void ForceDeleteDirectoryContents(params string[] directories)
        {
            if (directories != null && directories.Length != 0)
            {
                foreach (string directory in directories)
                {
                    if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                    {
                        continue;
                    }

                    try
                    {
                        UnlockHandleHelper.UnlockDirectory(directory);

                        foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                        {
                            ForceDeleteFile(file);
                        }
                    }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                }
            }
        }

        internal static void DeleteFile(params string[] paths)
        {
            if (paths != null && paths.Length != 0)
            {
                foreach (string path in paths)
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    try { File.Delete(path); }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                }
            }
        }

        internal static void ForceDeleteFile(params string[] paths)
        {
            if (paths != null && paths.Length != 0)
            {
                foreach (string path in paths)
                {
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    {
                        continue;
                    }

                    CommandExecutor.RunCommandAsTrustedInstaller($@"/c takeown /f ""{path}"" /a & icacls ""{path}"" /inheritance:r /remove *S-1-5-32-544 *S-1-5-11 *S-1-5-32-545 *S-1-5-18 & icacls ""{path}"" /grant ""{Environment.UserName}"":F & del /f /q ""{path}""");

                    for (int i = 0; File.Exists(path) && i < 5; i++)
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex) { ErrorLogger.LogDebug(ex); }

                        if (File.Exists(path))
                        {
                            CommandExecutor.RunCommand($"Remove-Item -LiteralPath '{path}' -Force", true);
                            Thread.Sleep(300);
                        }
                    }
                }
            }
        }
    }
}
