using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Networking.Entities;
using Project1.Core.UI;
using Project1.Framework.UI;

namespace Project1.Core.AI
{
    sealed class AIManager : GameSystem
    {
        public override void OnTooltipCreated(ITooltippable item, Tooltip t)
        {
            if (item is not TargetArgs target)
                return;
            if (target.Type != TargetType.Entity)
                return;
            var obj = target.Object;
            if (obj == null)
                return;
        }

        internal static void EndInteraction(Actor entity, bool success = false)
        {
            entity.Work.End(success);
            PacketEntityInteract.EndInteraction(Server.Instance, entity, success);
        }
    }
}
