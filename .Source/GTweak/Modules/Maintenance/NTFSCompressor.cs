using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTweak.Modules.Common;
using Microsoft.Win32.SafeHandles;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace GTweak.Modules.Maintenance
{
    internal sealed class NTFSCompressor
    {
        private const uint INVALID_FILE_ATTRIBUTES = 0xFFFFFFFF;
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        private const uint FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
        private const uint FILE_FILE_COMPRESSION = 0x00000010;
        private const uint FSCTL_SET_COMPRESSION = 0x0009C040;
        private const ushort COMPRESSION_FORMAT_NONE = 0;
        private const ushort COMPRESSION_FORMAT_DEFAULT = 1;

        private const int FIND_EX_INFO_BASIC = 1;
        private const int FIND_EX_SEARCH_NAME_MATCH = 0;
        private const uint FIND_FIRST_EX_LARGE_FETCH = 0x00000002;

        private static readonly IntPtr _invalidHandleValue = new IntPtr(-1);

        private readonly struct CompressionTarget
        {
            internal readonly string Path;
            internal readonly bool IsDirectory;
            internal readonly uint Attributes;

            internal CompressionTarget(string path, bool isDirectory, uint attributes)
            {
                Path = path;
                IsDirectory = isDirectory;
                Attributes = attributes;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern SafeFileHandle CreateFileW(string lpFileName, uint dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, ref ushort lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetVolumeInformationW(string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize, out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength, out uint lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, int nFileSystemNameSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint GetFileAttributesW(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetFileAttributesW(string lpFileName, uint dwFileAttributes);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindFirstFileExW(string lpFileName, int fInfoLevelId, out WIN32_FIND_DATA lpFindFileData, int fSearchOp, IntPtr lpSearchFilter, uint dwAdditionalFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);

        private static string GetExtendedPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path.StartsWith(@"\\?\", StringComparison.Ordinal))
            {
                return path;
            }

            string fullPath = Path.GetFullPath(path);
            if (fullPath.Length > 3)
            {
                fullPath = fullPath.TrimEnd('\\');
            }

            return fullPath.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + fullPath.Substring(2) : @"\\?\" + fullPath;
        }

        internal bool IsCompressed(string path)
        {
            try
            {
                uint attributes = GetFileAttributesW(GetExtendedPath(path));
                return attributes != INVALID_FILE_ATTRIBUTES && (attributes & (uint)FileAttributes.Compressed) != 0;
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }

            return false;
        }

        internal async Task<bool> IsSupportNtfs(string path)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string root = Path.GetPathRoot(Path.GetFullPath(path));
                    if (!string.IsNullOrEmpty(root))
                    {
                        if (GetVolumeInformationW(root, null, 0, out _, out _, out uint fsFlags, null, 0))
                        {
                            return (fsFlags & FILE_FILE_COMPRESSION) != 0;
                        }
                    }
                    return false;
                });
            }
            catch (Exception ex) { ErrorLogger.LogDebug(ex); }

            return false;
        }

        internal async Task ManageCompression(string directoryPath, bool compress, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                string rootPath = GetExtendedPath(directoryPath);
                uint rootAttributes = GetFileAttributesW(rootPath);

                if (rootAttributes == INVALID_FILE_ATTRIBUTES)
                {
                    throw new DirectoryNotFoundException(directoryPath);
                }

                if ((rootAttributes & (uint)FileAttributes.ReparsePoint) != 0)
                {
                    return;
                }

                bool isRootDirectory = (rootAttributes & (uint)FileAttributes.Directory) != 0;
                bool isRootEncrypted = (rootAttributes & (uint)FileAttributes.Encrypted) != 0;
                bool isRootCompressed = (rootAttributes & (uint)FileAttributes.Compressed) != 0;

                if (!isRootEncrypted && isRootCompressed != compress)
                {
                    ChangeCompressionState(rootPath, isRootDirectory, rootAttributes, compress);
                }

                if (!isRootDirectory)
                {
                    return;
                }

                ParallelOptions parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = cancellationToken
                };

                Parallel.ForEach(GetCompressionTargets(rootPath, compress, cancellationToken), parallelOptions, target =>
                {
                    try { ChangeCompressionState(target.Path, target.IsDirectory, target.Attributes, compress); }
                    catch (Exception ex) { ErrorLogger.LogDebug(ex); }
                });
            }, cancellationToken);
        }

        private static IEnumerable<CompressionTarget> GetCompressionTargets(string extendedRoot, bool compress, CancellationToken cancellationToken)
        {
            Stack<string> pendingDirs = new Stack<string>();
            pendingDirs.Push(extendedRoot);

            while (pendingDirs.Count > 0 && !cancellationToken.IsCancellationRequested)
            {
                string currentDir = pendingDirs.Pop();
                string prefix = currentDir.EndsWith("\\", StringComparison.Ordinal) ? currentDir : currentDir + "\\";
                IntPtr findHandle = FindFirstFileExW(prefix + "*", FIND_EX_INFO_BASIC, out WIN32_FIND_DATA findData, FIND_EX_SEARCH_NAME_MATCH, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

                if (findHandle == _invalidHandleValue)
                {
                    continue;
                }

                try
                {
                    do
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            yield break;
                        }

                        string name = findData.cFileName;
                        if (name == "." || name == "..")
                        {
                            continue;
                        }

                        FileAttributes attr = findData.dwFileAttributes;
                        if ((attr & FileAttributes.ReparsePoint) != 0)
                        {
                            continue;
                        }

                        bool isDirectory = (attr & FileAttributes.Directory) != 0;
                        string fullChildPath = prefix + name;

                        if (isDirectory)
                        {
                            pendingDirs.Push(fullChildPath);
                        }

                        if ((attr & FileAttributes.Encrypted) != 0)
                        {
                            continue;
                        }

                        bool isAlreadyCompressed = (attr & FileAttributes.Compressed) != 0;
                        if (isAlreadyCompressed != compress)
                        {
                            yield return new CompressionTarget(fullChildPath, isDirectory, (uint)attr);
                        }
                    }
                    while (FindNextFileW(findHandle, out findData));
                }
                finally
                {
                    FindClose(findHandle);
                }
            }
        }

        private static void ChangeCompressionState(string extendedPath, bool isDirectory, uint originalAttributes, bool compress)
        {
            bool strippedReadOnly = false;
            if (!isDirectory && (originalAttributes & (uint)FileAttributes.ReadOnly) != 0)
            {
                strippedReadOnly = SetFileAttributesW(extendedPath, originalAttributes & ~(uint)FileAttributes.ReadOnly);
            }

            try
            {
                uint flags = isDirectory ? (FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OPEN_REPARSE_POINT) : ((uint)FileAttributes.Normal | FILE_FLAG_OPEN_REPARSE_POINT);
                using SafeFileHandle handle = CreateFileW(extendedPath, GENERIC_READ | GENERIC_WRITE, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, OPEN_EXISTING, flags, IntPtr.Zero);
                if (handle.IsInvalid)
                {
                    return;
                }

                ushort compressionFormat = compress ? COMPRESSION_FORMAT_DEFAULT : COMPRESSION_FORMAT_NONE;
                DeviceIoControl(handle, FSCTL_SET_COMPRESSION, ref compressionFormat, sizeof(ushort), IntPtr.Zero, 0, out _, IntPtr.Zero);
            }
            finally
            {
                if (strippedReadOnly)
                {
                    SetFileAttributesW(extendedPath, originalAttributes);
                }
            }
        }
    }
}