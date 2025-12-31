namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class RoleMetaDefOf
    {
        public readonly static RoleMetaDef TownMember = new("TownMember", typeof(RoleTownMemberData), typeof(RoleTownMemberWorker), [NeedDefOf.Work]);
        public readonly static RoleMetaDef Adventurer = new("Adventurer", typeof(RoleAdventurerData), typeof(RoleAdventurerWorker), [
            //typeof(TaskGiverBeTalkedTo),
            //typeof(TaskGiverQuestComplete),
            //typeof(TaskGiverGetQuests),
            //typeof(TaskGiverTavernCustomer),
            //typeof(TaskGiverDepart)
            AdventurerNeedsDefOf.Adventuring
            ]);
        public readonly static RoleMetaDef Npc = new("Npc", typeof(RoleNpcData), typeof(RoleNpcWorker), []);


        //public readonly static NeedDef Lodging = new("Lodging", typeof(NeedLodging)) { TaskGiver = new TaskGiverLodging() };

        static RoleMetaDefOf()
        {
            Def.Register(typeof(RoleMetaDefOf));
        }
    }
    
}
