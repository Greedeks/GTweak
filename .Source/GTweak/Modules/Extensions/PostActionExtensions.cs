using System;
using System.Reflection;
using GTweak.Modules.Common;
using GTweak.Modules.Managers;

namespace GTweak.Modules.Extensions
{
    internal static class PostActionExtensions
    {
        private static readonly char[] Digits = "0123456789".ToCharArray();

        internal static bool HasAlert(this PostActionAttribute action) => action.Alert != NotificationManager.AlertType.None;
        internal static bool HasShell(this PostActionAttribute action) => action.Shell != ExplorerManager.ShellType.None;

        internal static PostActionAttribute GetPostAction(this Enum value)
        {
            return value.GetType().GetField(value.ToString())?.GetCustomAttribute<PostActionAttribute>() ?? new PostActionAttribute();
        }

        internal static PostActionAttribute GetPostAction(this string controlName, Type enumType)
        {
            if (string.IsNullOrEmpty(controlName))
            {
                return new PostActionAttribute();
            }

            int digitIndex = controlName.IndexOfAny(Digits);

            if (digitIndex >= 0 && int.TryParse(controlName.Substring(digitIndex), out int index))
            {
                return ((Enum)Enum.ToObject(enumType, index)).GetPostAction();
            }

            return new PostActionAttribute();
        }

        internal static NotificationManager.AlertType GetAlert(this Enum value) => value.GetPostAction().Alert;
        internal static ExplorerManager.ShellType GetShell(this Enum value) => value.GetPostAction().Shell;
    }
}