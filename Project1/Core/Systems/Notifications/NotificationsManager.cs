using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Systems.Notifications
{
    [EnsureStaticCtorCall]
    internal static class NotificationsManager
    {
        static readonly List<NotificationDef> defs = [.. Def.GetDefs<NotificationDef>()];
        static NotificationsManager()
        {
            foreach(var def in defs)
            {
                def.Worker.Hook();
            }
        }
    }
}
