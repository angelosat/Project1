using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Skills;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal static class PacketsSkills
    {
        static int _pTypeIdModifySkill;

        static PacketsSkills()
        {
            _pTypeIdModifySkill = Registry.PacketHandlers.Register(Receive);

            Registry.WorldEventHooksServer.Register<SkillAdjustedEvent>(SendSkillIncrease);
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
            var actor = client.World.Get<Actor>(r.ReadInt32());
            var skill = r.ReadDef<SkillDef>();
            var level = r.ReadInt32();
            var xp = r.ReadInt32();
            actor.Skills.SetValue(skill, level, xp);
        }
    }
}
