using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GTweak.Utilities.Helpers
{
    internal sealed class INIManager
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string _default, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string section, StringBuilder retVal, int size, string filePath);

        private readonly string pathToConfig;
        internal static Dictionary<string, string> 
            userTweaksConfidentiality = new Dictionary<string, string>(), 
            userTweaksInterface = new Dictionary<string, string>(),
            userTweaksServices = new Dictionary<string, string>(),
            userTweaksSystem = new Dictionary<string, string>();

        internal INIManager(string iniPath) => pathToConfig = new FileInfo(iniPath).FullName.ToString();

        internal string Read(string section, string key)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retValue, 255, pathToConfig);
            return retValue.ToString();
        }

        internal void Write(string section, string key, string value) => WritePrivateProfileString(section, key, value, pathToConfig);

        internal void WriteAll(string section, Dictionary<string, string> userTweaks)
        {
            foreach (KeyValuePair<string, string> addKeyValue in userTweaks)
                WritePrivateProfileString(section, addKeyValue.Key, addKeyValue.Value, pathToConfig);
        }

        internal bool IsThereSection(string section)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileSection(section, retValue, 255, pathToConfig);
            return !string.IsNullOrEmpty(retValue.ToString());
        }

        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string section, byte[] retVal, int size, string filePath);
        internal List<string> GetKeysOrValue(string section, bool isGetKey = true)
        {
            byte[] buffer = new byte[2048];
            GetPrivateProfileSection(section, buffer, 2048, pathToConfig);
            string[] temp = Encoding.ASCII.GetString(buffer).Trim('\0').Split('\0');

            List<string> result = new List<string>();

            foreach (string data in temp)
            {
                switch (isGetKey)
                {
                    case true:
                        result.Add(data.Substring(0, data.IndexOf("=")));
                        break;
                    case false:
                        result.Add(data.Remove(0, data.IndexOf("=", StringComparison.InvariantCulture)).Substring(1));
                        break;
                }
            }

            return result;
        }
    }
}
