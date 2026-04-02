using Project1.Core.AI.Thought;
using Project1.Core.Needs;
using Project1.Core.Towns.AI.Needs;
using Project1.Framework;

namespace Project1.Core.AI.MetaRoles
{
    [EnsureStaticCtorCall]
    internal class RoleMetaDefOf
    {
        public readonly static RoleMetaDef TownMember = new("TownMember", typeof(RoleTownMemberData), typeof(RoleTownMemberWorker), [NeedDefOf.Work], 
            [typeof(ThoughtItemEvaluatorTownMember)]); 
        public readonly static RoleMetaDef Adventurer = new("Adventurer", typeof(RoleAdventurerData), typeof(RoleAdventurerWorker), [
            AdventurerNeedsDefOf.Adventuring],
            [typeof(ThoughtItemEvaluatorVisitor),
            typeof(ThoughtAdventuring)]);
        public readonly static RoleMetaDef Npc = new("Npc", typeof(RoleNpcData), typeof(RoleNpcWorker), [], []);

        static RoleMetaDefOf()
        {
            Def.Register(typeof(RoleMetaDefOf));
        }
    }
}
