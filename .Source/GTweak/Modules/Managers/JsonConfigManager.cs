using System;
using System.Collections.Generic;
using System.IO;
using GTweak.Modules.Common;
using GTweak.Modules.Helpers;
using Newtonsoft.Json.Linq;

namespace GTweak.Modules.Managers
{
    internal static class JsonConfigManager
    {
        private static readonly (string Key, string Value)[] Signature =
        {
            ("Software", "GTweak"),
            ("Author", "Greedeks")
        };

        internal enum ConfigStatus { Invalid, Empty, Valid }

        internal enum Section { Confidentiality, Interface, Packages, Services, System }

        private static readonly object _fileLock = new object();

        internal static JObject Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            try { return JObject.Parse(File.ReadAllText(filePath)); }
            catch (Exception ex)
            {
                ErrorLogger.LogDebug(ex);
                return null;
            }
        }

        internal static void Save(JObject root, string targetPath)
        {
            if (root != null && !string.IsNullOrWhiteSpace(targetPath))
            {
                try { File.WriteAllText(targetPath, root.ToString()); }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            }
        }

        internal static void Write<T>(Section section, string tweakName, T value)
        {
            if (string.IsNullOrWhiteSpace(tweakName))
            {
                return;
            }

            lock (_fileLock)
            {
                try
                {
                    FileDirectoryHelper.CreateDirectory(PathTargets.Folders.Workspace);

                    JObject root = Load(PathTargets.Files.BackupConfig) ?? new JObject();

                    EnsureHeader(root);

                    string sectionName = section.ToString();

                    if (!(root[sectionName] is JObject sectionObject))
                    {
                        sectionObject = new JObject();
                        root[sectionName] = sectionObject;
                    }

                    sectionObject[tweakName] = value != null ? JToken.FromObject(value) : JValue.CreateNull();

                    File.WriteAllText(PathTargets.Files.BackupConfig, root.ToString());
                }
                catch (Exception ex) { ErrorLogger.LogDebug(ex); }
            }
        }

        internal static void Remove(Section section, string tweakName, JObject root)
        {
            if (root?[section.ToString()] is JObject sec)
            {
                sec.Remove(tweakName);
            }
        }

        internal static IEnumerable<(string Name, string Value)> GetEntries(Section section, JObject root)
        {
            if (root?[section.ToString()] is JObject sec)
            {
                foreach (JProperty prop in sec.Properties())
                {
                    yield return (prop.Name, prop.Value.ToString());
                }
            }
        }

        internal static bool SectionExists(Section section, JObject root) => root?[section.ToString()] is JObject sec && sec.HasValues;

        internal static ConfigStatus Validate(string filePath)
        {
            JObject root = Load(filePath);

            if (root == null)
            {
                return ConfigStatus.Invalid;
            }

            try
            {
                foreach ((string Key, string Value) in Signature)
                {
                    if (!string.Equals((string)root[Key], Value, StringComparison.OrdinalIgnoreCase))
                    {
                        return ConfigStatus.Invalid;
                    }
                }

                foreach (Section section in Enum.GetValues(typeof(Section)))
                {
                    if (SectionExists(section, root))
                    {
                        return ConfigStatus.Valid;
                    }
                }

                return ConfigStatus.Empty;
            }
            catch { return ConfigStatus.Invalid; }
        }

        private static void EnsureHeader(JObject root)
        {
            for (int i = Signature.Length - 1; i >= 0; i--)
            {
                if (root[Signature[i].Key] == null)
                {
                    root.AddFirst(new JProperty(Signature[i].Key, Signature[i].Value));
                }
            }
        }
    }
}