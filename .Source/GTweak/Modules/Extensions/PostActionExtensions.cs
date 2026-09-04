using System;
using System.Reflection;
using GTweak.Modules.Common;
using GTweak.Modules.Managers;

namespace GTweak.Modules.Extensions
{
    internal static class PostActionExtensions
    {
        internal static bool HasAlert(this PostActionAttribute action) => action.Alert != NotificationManager.AlertType.None;
        internal static bool HasShell(this PostActionAttribute action) => action.Shell != ExplorerManager.ShellType.None;

        internal static PostActionAttribute GetPostAction(this Enum value)
        {
            return value.GetType().GetField(value.ToString())?.GetCustomAttribute<PostActionAttribute>() ?? new PostActionAttribute();
        }

        internal static PostActionAttribute GetPostAction(this string memberName, Type enumType)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                return new PostActionAttribute();
            }

            return enumType.GetField(memberName)?.GetCustomAttribute<PostActionAttribute>() ?? new PostActionAttribute();
        }

        internal static NotificationManager.AlertType GetAlert(this Enum value) => value.GetPostAction().Alert;
        internal static ExplorerManager.ShellType GetShell(this Enum value) => value.GetPostAction().Shell;
    }
}