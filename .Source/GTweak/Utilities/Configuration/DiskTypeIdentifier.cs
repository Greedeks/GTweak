using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GTweak.Utilities.Configuration
{
    internal  static class DiskTypeIdentifier
    {
        private const uint IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
        private const uint FILE_SHARE_READ = 1;
        private const uint FILE_SHARE_WRITE = 2;
        private const uint OPEN_EXISTING = 3;

        private enum STORAGE_PROPERTY_ID
        {
            StorageDeviceProperty = 0,
            StorageDeviceSeekPenaltyProperty = 7
        }

        private enum STORAGE_QUERY_TYPE
        {
            PropertyStandardQuery = 0
        }

        private enum STORAGE_BUS_TYPE : uint
        {
            BusTypeUnknown = 0,
            BusTypeScsi = 1,
            BusTypeAtapi = 2,
            BusTypeAta = 3,
            BusTypeUsb = 7,
            BusTypeRAID = 8,
            BusTypeSas = 10,
            BusTypeSata = 11,
            BusTypeSd = 12,
            BusTypeVirtual = 14,
            BusTypeFileBackedVirtual = 15,
            BusTypeNvme = 17
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STORAGE_PROPERTY_QUERY
        {
            public STORAGE_PROPERTY_ID PropertyId;
            public STORAGE_QUERY_TYPE QueryType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public byte[] AdditionalParameters;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVICE_SEEK_PENALTY_DESCRIPTOR
        {
            public uint Version;
            public uint Size;
            [MarshalAs(UnmanagedType.U1)]
            public bool IncursSeekPenalty;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STORAGE_DEVICE_DESCRIPTOR
        {
            public uint Version;
            public uint Size;
            public byte DeviceType;
            public byte DeviceTypeModifier;
            [MarshalAs(UnmanagedType.U1)] public bool RemovableMedia;
            [MarshalAs(UnmanagedType.U1)] public bool CommandQueueing;
            public uint VendorIdOffset;
            public uint ProductIdOffset;
            public uint ProductRevisionOffset;
            public uint SerialNumberOffset;
            public STORAGE_BUS_TYPE BusType;
            public uint RawPropertiesLength;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, ref STORAGE_PROPERTY_QUERY lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, ref STORAGE_PROPERTY_QUERY lpInBuffer, int nInBufferSize, out DEVICE_SEEK_PENALTY_DESCRIPTOR lpOutBuffer, int nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        internal static string GetStorageKind(string physicalDrivePath)
        {
            try
            {
                using SafeFileHandle handle = CreateFile(physicalDrivePath, 0, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);

                if (handle.IsInvalid)
                {
                    return HardwareData.DiskTypeLabels.Unspecified;
                }

                STORAGE_PROPERTY_QUERY query = new STORAGE_PROPERTY_QUERY
                {
                    PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty,
                    QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery,
                    AdditionalParameters = new byte[1]
                };

                IntPtr buffer = Marshal.AllocHGlobal(1024);
                try
                {
                    if (DeviceIoControl(handle, IOCTL_STORAGE_QUERY_PROPERTY, ref query, Marshal.SizeOf(query), buffer, 1024, out _, IntPtr.Zero))
                    {
                        STORAGE_DEVICE_DESCRIPTOR descriptor = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>(buffer);

                        bool isSSD = false;
                        query.PropertyId = STORAGE_PROPERTY_ID.StorageDeviceSeekPenaltyProperty;

                        if (DeviceIoControl(handle, IOCTL_STORAGE_QUERY_PROPERTY, ref query, Marshal.SizeOf(query), out DEVICE_SEEK_PENALTY_DESCRIPTOR seek, Marshal.SizeOf<DEVICE_SEEK_PENALTY_DESCRIPTOR>(), out _, IntPtr.Zero))
                        {
                            isSSD = !seek.IncursSeekPenalty;
                        }

                        return descriptor.BusType switch
                        {
                            STORAGE_BUS_TYPE.BusTypeUnknown => HardwareData.DiskTypeLabels.Unspecified,
                            STORAGE_BUS_TYPE.BusTypeNvme => HardwareData.DiskTypeLabels.NVMe,
                            STORAGE_BUS_TYPE.BusTypeUsb => HardwareData.DiskTypeLabels.USB,
                            STORAGE_BUS_TYPE.BusTypeSd => HardwareData.DiskTypeLabels.SD,
                            STORAGE_BUS_TYPE.BusTypeVirtual => HardwareData.DiskTypeLabels.VHD,
                            STORAGE_BUS_TYPE.BusTypeFileBackedVirtual => HardwareData.DiskTypeLabels.VHDX,
                            _ => isSSD ? HardwareData.DiskTypeLabels.SSD : HardwareData.DiskTypeLabels.HDD
                        };
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }

                return HardwareData.DiskTypeLabels.Unspecified;
            }
            catch
            {
                return HardwareData.DiskTypeLabels.Unspecified;
            }
        }
    }
}
