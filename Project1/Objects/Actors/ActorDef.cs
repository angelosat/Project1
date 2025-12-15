namespace Start_a_Town_
{
    public class ActorDef : ItemVariantDef
    {
        //public NeedDef[] Needs;
        //public AttributeDef[] Attributes;
        //public ResourceDef[] Resources;
        //public SkillDef[] Skills;
        //public TraitDef[] Traits;
        //public GearType[] GearSlots;

        public ActorDef(string name) : base(ActorDefOf.Npc, name) { }

        //protected override Entity ApplyVariantTo(Entity obj)
        //{
        //    var actor = obj as Actor;
        //    actor.Needs.ApplyDefaults(this.Needs);
        //    actor.Attributes.ApplyDefaults(this.Attributes);
        //    return actor;
        //}
    }
}
