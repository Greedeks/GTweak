using System;
using GTweak.Modules.Managers;

namespace GTweak.Modules.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class PostActionAttribute : Attribute
    {
        internal NotificationManager.AlertType Alert { get; }
        internal ExplorerManager.ShellType Shell { get; }

        internal PostActionAttribute(NotificationManager.AlertType alert = NotificationManager.AlertType.None, ExplorerManager.ShellType shell = ExplorerManager.ShellType.None)
        {
            Alert = alert;
            Shell = shell;
        }
    }
}
