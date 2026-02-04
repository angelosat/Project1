using Project1.Framework.Attributes;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Net;
using Project1.Framework.Skills;
using Start_a_Town_;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsSkills
    {
        static int _pTypeIdModifySkill;

        static PacketsSkills()
        {
            _pTypeIdModifySkill = Registry.PacketHandlers.Register(Receive);

            Registry.MapEventHooksServer.Register<SkillAdjustedEvent>(SendSkillIncrease);
        }

        private static void SendSkillIncrease(SkillAdjustedEvent e)
        {
            Server.Instance.BeginPacket(_pTypeIdModifySkill)
                .Write(e.Actor.RefId)
                .Write(e.Skill.Def)
                .Write(e.Skill.Level)
                .Write(e.Skill.LvlProgress.Value);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var skill = r.ReadDef<SkillDef>();
            var level = r.ReadInt32();
            var xp = r.ReadInt32();
            actor.Skills.SetValue(skill, level, xp);
        }
    }
}
