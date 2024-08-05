using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GTweak.Utilities
{
    internal sealed class СonfigSettings
    {
        internal static Dictionary<string, string> configConfidentiality = new Dictionary<string, string>();
        internal static Dictionary<string, string> configInterface = new Dictionary<string, string>();
        internal static Dictionary<string, string> configServices = new Dictionary<string, string>();
        internal static Dictionary<string, string> configSystem = new Dictionary<string, string>();
        private readonly string pathToConfig; 

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, string key, string _default, StringBuilder retVal, int size, string filePath);

        internal СonfigSettings(string iniPath) => pathToConfig = new FileInfo(iniPath).FullName.ToString();

        internal string ReadConfig(string section, string key)
        {
            StringBuilder retValue = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retValue, 255, pathToConfig);
            return retValue.ToString();
        }

        internal void WriteConfig(string section, string key, string value) => WritePrivateProfileString(section, key, value, pathToConfig);
    }
}
