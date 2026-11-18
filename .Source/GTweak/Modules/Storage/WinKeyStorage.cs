using System;
using System.Collections.Generic;

namespace GTweak.Modules.Storage
{
    internal class WinKeyStorage
    {
        protected WinKeyStorage() { }

        protected static readonly Dictionary<string, string> keysKMS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Home", @"TX9XD-98N7V-6WMQ6-BX7FG-H8Q99" },
            { "Home N", @"3KHY7-WNT83-DGQKR-F7HPR-844BM" },
            { "Home Single Language", @"7HNRX-D7KGG-3K4RQ-4WPJ4-YTDFH" },
            { "Home China", @"PVMJN-6DFY6-9CCP6-7BKTT-D3WVR" },
            { "Pro", @"W269N-WFGWX-YVC9B-4J6C9-T83GX" },
            { "Pro N", @"MH37W-N47XK-V7XM9-C7227-GCQG9" },
            { "Pro for Workstations", @"NRG8B-VKK3Q-CXVCJ-9G2XF-6Q84J" },
            { "Pro N for Workstations", @"9FNHH-K3HBT-3W4TD-6383H-6XYWF" },
            { "Pro Education", @"6TP4R-GNPTD-KYYHQ-7B7DP-J447Y" },
            { "Pro Education N", @"YVWGF-BXNMC-HTQYQ-CPQ99-66QFC" },
            { "Education", @"NW6C2-QMPVW-D7KKK-3GKT6-VCFB2" },
            { "Education N", @"2WH4N-8QGBV-H22JP-CT43Q-MDWWJ" },
            { "Enterprise", @"NPPR9-FWDCX-D2C8J-H872K-2YT43" },
            { "Enterprise N", @"DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4" },
            { "Enterprise G", @"YYVX9-NTFWV-6MDM3-9PT4T-4M68B" },
            { "Enterprise G N", @"44RPN-FTY23-9VTTB-MP9BX-T84FV" },
            { "Enterprise LTSC 2021", @"M7XTQ-FN8P6-TTKYV-9D4CC-J462D" },
            { "Enterprise LTSC 2024", @"M7XTQ-FN8P6-TTKYV-9D4CC-J462D" },
            { "Enterprise N LTSC 2021", @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H" },
            { "Enterprise N LTSC 2024", @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H" },
            { "IoT Enterprise LTSC 2021", @"KBN8V-HFGQ4-MGXVD-347P6-PDQGT" },
            { "IoT Enterprise LTSC 2024", @"KBN8V-HFGQ4-MGXVD-347P6-PDQGT" },
            { "Lean", @"NBTWJ-3DR69-3C4V8-C26MC-GQ9M6" },
            { "SE", @"37D7F-N49CB-WQR8W-TBJ73-FM8RX" },
            { "SE N", @"6XN7V-PCBDC-BDBRH-8DQY7-G6R44" },
            { "Enterprise multi-session", @"CPWHC-NT2C7-VYW78-DHDB2-PG3GK" },
            { "Server 2025 Standard", @"TVRH6-WHNXV-R9WG3-9XRFY-MY832" },
            { "Server 2025 Datacenter", @"D764K-2NDRG-47T6Q-P8T8W-YP6DF" },
            { "Server 2025 Azure Core", @"FCNV3-279Q9-BQB46-FTKXX-9HPRH" },
            { "Server 2025 Azure Datacenter", @"XGN3F-F394H-FD2MY-PP6FD-8MCRC" },
            { "Server 2022 Datacenter", @"WX4NM-KYWYW-QJJR4-XV3QB-6VM33" },
            { "Server 2022 Standard", @"VDYBN-27WPP-V4HQT-9VMD4-VMK7H" },
            { "Server 2022 Azure Core", @"6N379-GGTMK-23C6M-XVVTC-CKFRQ" },
            { "Server 2022 Azure Datacenter", @"NTBV8-9K7Q8-V27C6-M2BTV-KHMXV" },
            { "Server Datacenter", @"WX4NM-KYWYW-QJJR4-XV3QB-6VM33" },
            { "Server DatacenterACor", @"6NMRW-2C8FM-D24W7-TQWMY-CWH2D" },
            { "Server StandardACor", @"N2KJX-J94YW-TQVFB-DG9YT-724CC" }
        };

        protected static readonly Dictionary<string, string> keysHWID = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Home", @"YTMG3-N6DKC-DKB77-7M9GH-8HVX7" },
            { "Home N", @"4CPRK-NM3K3-X6XXQ-RXX86-WXCHW" },
            { "Home China", @"N2434-X9D7W-8PF6X-8DV9T-8TYMD" },
            { "Home Single Language", @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT" },
            { "Pro", @"VK7JG-NPHTM-C97JM-9MPGT-3V66T" },
            { "Pro N", @"2B87N-8KFHP-DKV6R-Y2C8J-PKCKT" },
            { "Pro Education", @"8PTT6-RNW4C-6V7J2-C2D3X-MHBPB" },
            { "Pro Education N", @"GJTYN-HDMQY-FRR76-HVGC7-QPF8P" },
            { "Pro for Workstations", @"DXG7C-N36C4-C4HTG-X4T3X-2YV77" },
            { "Pro N for Workstations", @"WYPNQ-8C467-V2W6J-TX4WX-WT2RQ" },
            { "Education", @"YNMGQ-8RYV3-4PGQ3-C8XTP-7CFBY" },
            { "Education N", @"84NGF-MHBT6-FXBX8-QWJK7-DRR8H" },
            { "Enterprise", @"XGVPP-NMH47-7TTHJ-W3FW7-8HV2C" },
            { "Enterprise N", @"3V6Q6-NQXCX-V8YXR-9QCYV-QPFCT" },
            { "Enterprise LTSC 2021", @"QPM6N-7J2WJ-P88HH-P3YRH-YY74H" },
            { "Enterprise LTSC 2024", @"CGK42-GYN6Y-VD22B-BX98W-J8JXD" },
            { "IoT Enterprise", @"XQQYW-NFFMW-XJPBH-K8732-CKFFD" },
            { "IoT Enterprise Subscription", @"P8Q7T-WNK7X-PMFXY-VXHBG-RRK69" },
            { "IoT Enterprise LTSC 2021", @"QPM6N-7J2WJ-P88HH-P3YRH-YY74H" },
            { "IoT Enterprise LTSC 2024", @"CGK42-GYN6Y-VD22B-BX98W-J8JXD" },
            { "IoT Enterprise LTSC Subscription 2024", @"N979K-XWD77-YW3GB-HBGH6-D32MH" },
            { "S", @"V3WVW-N2PV2-CGWC3-34QGF-VMJ2C" },
            { "S N", @"NH9J3-68WK7-6FB93-4K3DF-DJ4F6" },
            { "SE", @"KY7PN-VR6RX-83W6Y-6DDYQ-T6R4W" },
            { "SE N", @"K9VKN-3BGWV-Y624W-MCRMQ-BHDCD" },
            { "Team", @"XKCNC-J26Q9-KFHD2-FKTHY-KD72Y" }
        };
    }
}