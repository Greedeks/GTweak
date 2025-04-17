using System.Collections.Generic;
using System.Linq;

namespace GTweak.Utilities.Helpers.Storage
{
    internal static class NotifActionsStorage
    {
        internal static readonly Dictionary<string, string> ConfNotifActions = new[]
        {
            new { Button = "TglButton8", Action = "restart" },
            new { Button = "TglButton15", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);

        internal static readonly Dictionary<string, string> IntfNotifActions = new[]
        {
            new { Button = "TglButton1", Action = "logout" },
            new { Button = "TglButton2", Action = "logout" },
            new { Button = "TglButton3", Action = "logout" },
            new { Button = "TglButton4", Action = "logout" },
            new { Button = "TglButton5", Action = "logout" },
            new { Button = "TglButton10", Action = "logout" },
            new { Button = "TglButton11", Action = "logout" },
            new { Button = "TglButton12", Action = "logout" },
            new { Button = "TglButton26", Action = "logout" },
            new { Button = "TglButton27", Action = "logout" },
            new { Button = "TglButton22", Action = "restart" },
            new { Button = "TglButton20", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);

        internal static readonly Dictionary<string, string> SysNotifActions = new[]
        {
            new { Button = "TglButton2", Action = "logout" },
            new { Button = "TglButton7", Action = "restart" },
            new { Button = "TglButton9", Action = "restart" },
            new { Button = "TglButton10", Action = "restart" },
            new { Button = "TglButton12", Action = "restart" },
            new { Button = "TglButton13", Action = "restart" },
            new { Button = "TglButton14", Action = "restart" },
            new { Button = "TglButton15", Action = "restart" },
            new { Button = "TglButton20", Action = "restart" },
            new { Button = "TglButton23", Action = "restart" }
        }.ToDictionary(x => x.Button, x => x.Action);
    }
}
