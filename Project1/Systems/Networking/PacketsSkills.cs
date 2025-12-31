using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsSkills
    {
        static int _pTypeIdModifySkill;

        static PacketsSkills()
        {
            _pTypeIdModifySkill = Registry.PacketHandlers.Register(Receive);

            Registry.MapEventHooksServer.Register<SkillIncreaseEvent>(SendSkillIncrease);
        }

        private static void SendSkillIncrease(SkillIncreaseEvent e)
        {
            Server.Instance.BeginPacket(_pTypeIdModifySkill)
                .Write(e.Actor.RefId)
                .Write(e.Skill)
                .Write(e.Delta);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var skill = r.ReadDef<SkillDef>();
            var delta = r.ReadInt32();

            actor.Skills.Increase(skill, delta);
        }
    }
}
