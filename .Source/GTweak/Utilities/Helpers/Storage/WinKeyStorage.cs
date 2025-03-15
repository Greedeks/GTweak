using System.Collections.Generic;

namespace GTweak.Utilities.Helpers.Storage
{
    internal class WinKeyStorage
    {
        internal static readonly Dictionary<(string pattern, byte words), string> keysKMS = new Dictionary<(string pattern, byte words), string>
        {
            { ("Home|Single|Language", 3), @"7HNRX-D7KGG-3K4RQ-4WPJ4-YTDFH" },
            { ("Home", 1), @"TX9XD-98N7V-6WMQ6-BX7FG-H8Q99" },
            { ("Education", 1), @"NW6C2-QMPVW-D7KKK-3GKT6-VCFB2" },
            { ("Enterprise|LSTB", 2), @"7YMNV-PG77F-K66KT-KG9VQ-TCQGB" },
            { ("Enterprise|N|LTSC", 3), @"92NFX-8DJQP-P6BBQ-THF9C-7CG2H" },
            { ("Enterprise|N", 2), @"DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4" },
            { ("Enterprise|G", 2), @"YYVX9-NTFWV-6MDM3-9PT4T-4M68B" },
            { ("Enterprise", 1), @"ND4DX-39KJY-FYWQ9-X6XKT-VCFCF" },
            { ("Core|Single|Language", 3), @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT" },
            { ("Core", 1), @"KTNPV-KTRK4-3RRR8-39X6W-W44T3" },
            { ("Pro", 1), @"W269N-WFGWX-YVC9B-4J6C9-T83GX" }
        };

        internal static readonly Dictionary<(string pattern, byte words), string> keysHWID = new Dictionary<(string pattern, byte words), string>
        {
            { ("Education", 1), @"YNMGQ-8RYV3-4PGQ3-C8XTP-7CFBY" },
            { ("Education|N", 2), @"84NGF-MHBT6-FXBX8-QWJK7-DRR8H" },
            { ("Enterprise", 1), @"XGVPP-NMH47-7TTHJ-W3FW7-8HV2C" },
            { ("Enterprise|N", 2), @"3V6Q6-NQXCX-V8YXR-9QCYV-QPFCT" },
            { ("Enterprise|LTSB|2015", 3), @"FWN7H-PF93Q-4GGP8-M8RF3-MDWWW" },
            { ("Enterprise|LTSB|2016", 3), @"NK96Y-D9CD8-W44CQ-R8YTK-DYJWX" },
            { ("Enterprise|LTSC|2019", 3), @"43TBQ-NH92J-XKTM7-KT3KK-P39PB" },
            { ("Enterprise|N|LTSB|2015", 4), @"NTX6B-BRYC2-K6786-F6MVQ-M7V2X" },
            { ("Enterprise|N|LTSB|2016", 4), @"2DBW3-N2PJG-MVHW3-G7TDK-9HKR4" },
            { ("Home", 1), @"YTMG3-N6DKC-DKB77-7M9GH-8HVX7" },
            { ("Home|N", 2), @"4CPRK-NM3K3-X6XXQ-RXX86-WXCHW" },
            { ("Home|China", 2), @"N2434-X9D7W-8PF6X-8DV9T-8TYMD" },
            { ("Home|Single|Language", 3), @"BT79Q-G7N6G-PGBYW-4YWX6-6F4BT" },
            { ("IoT|Enterprise", 2), @"XQQYW-NFFMW-XJPBH-K8732-CKFFD" },
            { ("IoT|Enterprise|Subscription", 3), @"P8Q7T-WNK7X-PMFXY-VXHBG-RRK69" },
            { ("IoT|Enterprise|LTSC|2021", 4), @"QPM6N-7J2WJ-P88HH-P3YRH-YY74H" },
            { ("IoT|Enterprise|LTSC|2024", 4), @"CGK42-GYN6Y-VD22B-BX98W-J8JXD" },
            { ("IoT|Enterprise|LTSC|Subscription|2024", 5), @"N979K-XWD77-YW3GB-HBGH6-D32MH" },
            { ("Pro", 1), @"VK7JG-NPHTM-C97JM-9MPGT-3V66T" },
            { ("Pro|N", 2), @"2B87N-8KFHP-DKV6R-Y2C8J-PKCKT" },
            { ("Pro|Education", 2), @"8PTT6-RNW4C-6V7J2-C2D3X-MHBPB" },
            { ("Pro|Education|N", 3), @"GJTYN-HDMQY-FRR76-HVGC7-QPF8P" },
            { ("Pro|for|Workstations", 3), @"DXG7C-N36C4-C4HTG-X4T3X-2YV77" },
            { ("Pro|N|for|Workstations", 4), @"WYPNQ-8C467-V2W6J-TX4WX-WT2RQ" },
            { ("S", 1), @"V3WVW-N2PV2-CGWC3-34QGF-VMJ2C" },
            { ("S|N", 2), @"NH9J3-68WK7-6FB93-4K3DF-DJ4F6" },
            { ("SE", 1), @"KY7PN-VR6RX-83W6Y-6DDYQ-T6R4W" },
            { ("SE|N", 2), @"K9VKN-3BGWV-Y624W-MCRMQ-BHDCD" },
            { ("Team", 1), @"XKCNC-J26Q9-KFHD2-FKTHY-KD72Y" }
        };
    }
}
