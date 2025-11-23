using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System.Linq;

namespace Start_a_Town_.AI
{
    sealed class AIManager : GameComponent
    {
        public override void Initialize()
        {
            AITask.Initialize();
        }

        public override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
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
                        Behaviors.Threat thr = st.Threats.FirstOrDefault(t => t.Entity == attacker);
                        if (thr == null)
                        {
                            thr = new Behaviors.Threat(target, dmg, attacker);
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

        internal static void Interact(Actor entity, Interaction action, TargetArgs target)
        {
            if (entity.Net is Server) // interactions only initiated server-side?
            {
                entity.Work.Perform(action, target);
                PacketEntityInteract.Send(Server.Instance, entity, action, target);
            }
        }

        internal static void EndInteraction(Actor entity, bool success = false)
        {
            entity.Work.End(success);
            PacketEntityInteract.EndInteraction(Server.Instance, entity, success);
        }
    }
}
