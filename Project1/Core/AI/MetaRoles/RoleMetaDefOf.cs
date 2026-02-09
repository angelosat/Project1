using Project1.Core.Needs;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.Base;
using Project1.Core.Helpers;

namespace Project1.Core.AI.MetaRoles
{
    [EnsureStaticCtorCall]
    internal class RoleMetaDefOf
    {
        public readonly static RoleMetaDef TownMember = new("TownMember", typeof(RoleTownMemberData), typeof(RoleTownMemberWorker), [NeedDefOf.Work]);
        public readonly static RoleMetaDef Adventurer = new("Adventurer", typeof(RoleAdventurerData), typeof(RoleAdventurerWorker), [
            AdventurerNeedsDefOf.Adventuring
            ]);
        public readonly static RoleMetaDef Npc = new("Npc", typeof(RoleNpcData), typeof(RoleNpcWorker), []);

        static RoleMetaDefOf()
        {
            Def.Register(typeof(RoleMetaDefOf));
        }
    }
    
}
