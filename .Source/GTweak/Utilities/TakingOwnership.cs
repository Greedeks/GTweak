using System;
using System.Runtime.InteropServices;

namespace GTweak.Utilities
{
    internal class TakingOwnership
    {
        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct SID_IDENTIFIER_AUTHORITY
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.I1)]
            public byte[] Value;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct TRUSTEE
        {
            public System.IntPtr pMultipleTrustee;
            public MULTIPLE_TRUSTEE_OPERATION MultipleTrusteeOperation;
            public TRUSTEE_FORM TrusteeForm;
            public TRUSTEE_TYPE TrusteeType;
            public IntPtr ptstrName;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct EXPLICIT_ACCESS
        {
            public ACCESS_MASK grfAccessPermissions;
            public ACCESS_MODE grfAccessMode;
            public uint grfInheritance;
            public TRUSTEE Trustee;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        private enum TRUSTEE_TYPE
        {
            TRUSTEE_IS_GROUP,
        }

        private enum TRUSTEE_FORM
        {
            TRUSTEE_IS_SID,
        }

        private enum MULTIPLE_TRUSTEE_OPERATION { }

        public enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }

        [Flags]
        private enum ACCESS_MASK : uint
        {
            GENERIC_ALL = 0x10000000,
        }

        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            OWNER_SECURITY_INFORMATION = 0x00000001,
            DACL_SECURITY_INFORMATION = 0x00000004,
        }

        private enum ACCESS_MODE
        {
            SET_ACCESS
        }

        private const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";
        private static SID_IDENTIFIER_AUTHORITY SECURITY_NT_AUTHORITY =
            new SID_IDENTIFIER_AUTHORITY() { Value = new byte[] { 0, 0, 0, 0, 0, 5 } };

        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const int NO_INHERITANCE = 0x0;
        private const int SECURITY_BUILTIN_DOMAIN_RID = 0x00000020;
        private const int DOMAIN_ALIAS_RID_ADMINS = 0x00000220;
        private const int TOKEN_QUERY = 8;
        private const int SE_PRIVILEGE_ENABLED = 2;

        [DllImportAttribute("advapi32.dll", EntryPoint = "OpenProcessToken")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool OpenProcessToken([InAttribute] IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImportAttribute("advapi32.dll", EntryPoint = "AllocateAndInitializeSid")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool AllocateAndInitializeSid(
            [InAttribute] ref SID_IDENTIFIER_AUTHORITY pIdentifierAuthority,
            byte nSubAuthorityCount,
            uint nSubAuthority0,
            uint nSubAuthority1,
            uint nSubAuthority2,
            uint nSubAuthority3,
            uint nSubAuthority4,
            uint nSubAuthority5,
            uint nSubAuthority6,
            uint nSubAuthority7,
            ref IntPtr pSid);

        [DllImportAttribute("kernel32.dll", EntryPoint = "CloseHandle")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool CloseHandle([InAttribute] IntPtr hObject);

        [DllImportAttribute("kernel32.dll", EntryPoint = "GetCurrentProcess")]
        private static extern IntPtr GetCurrentProcess();

        [DllImportAttribute("advapi32.dll", EntryPoint = "FreeSid")]
        private static extern IntPtr FreeSid([InAttribute] IntPtr pSid);

        [DllImportAttribute("kernel32.dll", EntryPoint = "LocalFree")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImportAttribute("advapi32.dll", EntryPoint = "LookupPrivilegeValueA")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool LookupPrivilegeValueA([InAttribute] [MarshalAsAttribute(UnmanagedType.LPStr)] string lpSystemName, [InAttribute] [MarshalAsAttribute(UnmanagedType.LPStr)] string lpName, [OutAttribute] out LUID lpLuid);

        [DllImportAttribute("advapi32.dll", EntryPoint = "AdjustTokenPrivileges")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool AdjustTokenPrivileges([InAttribute()] IntPtr TokenHandle, [MarshalAsAttribute(UnmanagedType.Bool)] bool DisableAllPrivileges,
            [InAttribute()]
            ref TOKEN_PRIVILEGES NewState,
            uint BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int SetNamedSecurityInfo(
            string pObjectName,
            SE_OBJECT_TYPE ObjectType,
            SECURITY_INFORMATION SecurityInfo,
            IntPtr psidOwner,
            IntPtr psidGroup,
            IntPtr pDacl,
            IntPtr pSacl);

        [DllImport("Advapi32.dll", EntryPoint = "SetEntriesInAclA",
         CallingConvention = CallingConvention.Winapi,
         SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern int SetEntriesInAcl(
            int CountofExplicitEntries,
            ref EXPLICIT_ACCESS ea,
            IntPtr OldAcl,
            ref IntPtr NewAcl);

        internal static void GrantAdministratorsAccess(string name, SE_OBJECT_TYPE type)
        {
            SID_IDENTIFIER_AUTHORITY sidNTAuthority = SECURITY_NT_AUTHORITY;

            IntPtr sidAdmin = IntPtr.Zero;
            AllocateAndInitializeSid(ref sidNTAuthority, 2,
                                     SECURITY_BUILTIN_DOMAIN_RID,
                                     DOMAIN_ALIAS_RID_ADMINS,
                                     0, 0, 0, 0, 0, 0,
                                     ref sidAdmin);

            EXPLICIT_ACCESS[] explicitAccesss = new EXPLICIT_ACCESS[1];
            explicitAccesss[0].grfAccessPermissions = ACCESS_MASK.GENERIC_ALL;
            explicitAccesss[0].grfAccessMode = ACCESS_MODE.SET_ACCESS;
            explicitAccesss[0].grfInheritance = NO_INHERITANCE;
            explicitAccesss[0].Trustee.TrusteeForm = TRUSTEE_FORM.TRUSTEE_IS_SID;
            explicitAccesss[0].Trustee.TrusteeType = TRUSTEE_TYPE.TRUSTEE_IS_GROUP;
            explicitAccesss[0].Trustee.ptstrName = sidAdmin;

            IntPtr acl = IntPtr.Zero;
            SetEntriesInAcl(1, ref explicitAccesss[0], (IntPtr)0, ref acl);

            static void setPrivilege(string privilege, bool allow)
            {
                TOKEN_PRIVILEGES tokenPrivileges = new TOKEN_PRIVILEGES();
                OpenProcessToken(GetCurrentProcess(),
                    TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr token);

                if (allow)
                {
                    LookupPrivilegeValueA(null, privilege, out LUID luid);
                    tokenPrivileges.PrivilegeCount = 1;
                    tokenPrivileges.Privileges = new LUID_AND_ATTRIBUTES[1];
                    tokenPrivileges.Privileges[0].Luid = luid;
                    tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
                }

                AdjustTokenPrivileges(token, false, ref tokenPrivileges, 0,
                    IntPtr.Zero, IntPtr.Zero);
                CloseHandle(token);
            }

            setPrivilege(SE_TAKE_OWNERSHIP_NAME, true);

            SetNamedSecurityInfo(
                name,
                type,
                SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION,
                sidAdmin,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            setPrivilege(SE_TAKE_OWNERSHIP_NAME, false);

            SetNamedSecurityInfo(
                name,
                type,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                IntPtr.Zero, IntPtr.Zero,
                acl,
                IntPtr.Zero);

            FreeSid(sidAdmin);
            LocalFree(acl);
        }
    }
}

