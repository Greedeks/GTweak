using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GTweak.Utilities.Helpers.Managers
{
    internal sealed class INIManager
    {
        [DllImport("kernel32.dll")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string _default, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string section, StringBuilder retVal, int size, string filePath);

        private readonly string _pathToConfig;

        internal static Dictionary<string, string>
            TempTweaksConf = new Dictionary<string, string>(),
            TempTweaksIntf = new Dictionary<string, string>(),
            TempTweaksSvc = new Dictionary<string, string>(),
            TempTweaksSys = new Dictionary<string, string>();

        internal static bool IsAllTempDictionaryEmpty => TempTweaksConf.Count == 0 && TempTweaksIntf.Count == 0 && TempTweaksSvc.Count == 0 && TempTweaksSys.Count == 0;

        internal const string SectionConf = "Confidentiality Tweaks";
        internal const string SectionIntf = "Interface Tweaks";
        internal const string SectionSvc = "Services Tweaks";
        internal const string SectionSys = "System Tweaks";

        internal INIManager(string iniPath) => _pathToConfig = new FileInfo(iniPath).FullName.ToString();

        internal string Read(string section, string key)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retValue, 255, _pathToConfig);
            return retValue.ToString();
        }

        internal void Write(string section, string key, string value) => WritePrivateProfileString(section, key, value, _pathToConfig);

        internal void WriteAll(string section, Dictionary<string, string> selectedDictionary)
        {
            if (selectedDictionary.Count == 0) return;

            foreach (KeyValuePair<string, string> addKeyValue in selectedDictionary)
                WritePrivateProfileString(section, addKeyValue.Key, addKeyValue.Value, _pathToConfig);
        }

        internal static void TempWrite<T>(Dictionary<string, string> selectedDictionary, string tweak, T value)
        {
            if (selectedDictionary.ContainsKey(tweak))
                selectedDictionary.Remove(tweak);

            selectedDictionary.Add(tweak, value.ToString());
        }

        internal bool IsThereSection(string section)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileSection(section, retValue, 255, _pathToConfig);
            return !string.IsNullOrEmpty(retValue.ToString());
        }

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string section, byte[] retVal, int size, string filePath);
        internal List<string> GetKeysOrValue(string section, bool isGetKey = true)
        {
            byte[] buffer = new byte[2048];
            GetPrivateProfileSection(section, buffer, 2048, _pathToConfig);
            string[] temp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            List<string> result = new List<string>();

            foreach (string data in temp)
            {
                int equalsIndex = data.IndexOf("=", StringComparison.InvariantCulture);
                if (equalsIndex > 0)
                    result.Add(isGetKey ? data.Substring(0, equalsIndex) : data.Substring(equalsIndex + 1));
            }
            return result;
        }
    }
}
