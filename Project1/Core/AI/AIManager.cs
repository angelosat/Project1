using Project1.Core.Components;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using Project1.Core.Networking;
using Project1.Core.Networking.Entities;
using Project1.Core.UI;
using Project1.Framework.Events;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.AI
{
    sealed class AIManager : GameSystem
    {
        public override void Initialize()
        {
            Plan.Initialize();
        }

        public override void OnGameEvent(GameEvent e)
        {
            switch ((Message.Types)e.Type)
            {
                case Message.Types.EntityAttacked:
                    var attacker = e.Parameters[0] as GameObject;
                    if (attacker.Net is Client)
                        break;
                    var target = e.Parameters[1] as GameObject;
                    var dmg = (int)e.Parameters[2];
                    if (!target.HasComponent<AIComponent>())
                        break;
                    var st = AIState.GetState(target);
                    if (st != null)
                    {
                        Threat thr = st.Threats.FirstOrDefault(t => t.Entity == attacker);
                        if (thr == null)
                        {
                            thr = new Threat(target, dmg, attacker);
                            st.Threats.Add(thr);
                        }
                        else
                            thr.Value += dmg;
                    }
                    break;

                default:
                    break;
            }
        }

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

        internal static void Interact(Actor entity, Interaction action, TargetArgs target, int count = -1)
        {
            if (entity.Net is Server) // interactions only initiated server-side?
            {
                entity.Work.Perform(action, target, count);
                PacketEntityInteract.Send(Server.Instance, entity, action, target, count);
            }
        }

        internal static void EndInteraction(Actor entity, bool success = false)
        {
            entity.Work.End(success);
            PacketEntityInteract.EndInteraction(Server.Instance, entity, success);
        }
    }
}
