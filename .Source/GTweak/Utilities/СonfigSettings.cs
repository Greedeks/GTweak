using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GTweak.Utilities
{
    internal sealed class СonfigSettings
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string _default, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileSection(string section, StringBuilder retVal, int size, string filePath);

        private readonly string pathToConfig;
        internal static Dictionary<string, string> configConfidentiality = new Dictionary<string, string>(), 
            configInterface = new Dictionary<string, string>(),
            configServices = new Dictionary<string, string>(),
            configSystem = new Dictionary<string, string>();

        internal СonfigSettings(string iniPath) => pathToConfig = new FileInfo(iniPath).FullName.ToString();

        internal string ReadConfig(string section, string key)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retValue, 255, pathToConfig);
            return retValue.ToString();
        }

        internal bool IsThereSection(string section)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileSection(section, retValue, 255, pathToConfig);
            return !string.IsNullOrEmpty(retValue.ToString());
        }

        internal void WriteConfig(string section, string key, string value) => WritePrivateProfileString(section, key, value, pathToConfig);
    }
}
