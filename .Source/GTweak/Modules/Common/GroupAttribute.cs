using System;

namespace GTweak.Modules.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class GroupAttribute : Attribute
    {
        internal string Key { get; }
        internal GroupAttribute(string key) => Key = key;
    }
}
