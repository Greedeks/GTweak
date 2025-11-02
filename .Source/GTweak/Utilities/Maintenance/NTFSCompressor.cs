using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace GTweak.Utilities.Maintenance
{
    internal sealed class NTFSCompressor
    {
        private const ushort COMPRESSION_FORMAT_NONE = 0x0000;
        private const ushort COMPRESSION_FORMAT_DEFAULT = 0x0001;
        private const uint FSCTL_SET_COMPRESSION = 0x9C040;
        private const FileAttributes BackupSemantics = (FileAttributes)0x02000000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, FileMode dwCreationDisposition, FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, ref ushort lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetVolumeInformation(string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize, out uint lpVolumeSerialNumber, out uint lpMaximumComponentLength, out uint lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, int nFileSystemNameSize);

        internal async Task<bool> IsSupportNtfs(string path)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string root = Path.GetPathRoot(Path.GetFullPath(path));
                    if (!string.IsNullOrEmpty(root))
                    {
                        StringBuilder fsName = new StringBuilder(261);
                        if (GetVolumeInformation(root, null, 0, out uint _, out uint _, out uint _, fsName, fsName.Capacity))
                        {
                            return string.Equals(fsName.ToString(), "NTFS", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                    return false;
                });
            }
            catch { return false; }
        }

        internal void SetCompression(string directoryPath, bool compress)
        {
            Task.Run(async () =>
            {
                await UpdateCompressionState(directoryPath, compress);

                IEnumerable<string> directories = Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories);
                Parallel.ForEach(directories, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async dir => { await UpdateCompressionState(dir, compress); });

                IEnumerable<string> files = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories);
                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, async file => { await UpdateCompressionState(file, compress); });
            });
        }

        private async Task UpdateCompressionState(string path, bool compress)
        {
            await Task.Run(() =>
            {
                FileAttributes attributes = File.GetAttributes(path);
                FileAttributes flags = (attributes & FileAttributes.Directory) == FileAttributes.Directory ? BackupSemantics : FileAttributes.Normal;
                using SafeFileHandle handle = CreateFile(path, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, flags, IntPtr.Zero);
                ushort compressionFormat = compress ? COMPRESSION_FORMAT_DEFAULT : COMPRESSION_FORMAT_NONE;
                DeviceIoControl(handle, FSCTL_SET_COMPRESSION, ref compressionFormat, sizeof(ushort), IntPtr.Zero, 0, out int bytesReturned, IntPtr.Zero);
            });
        }
    }
}
