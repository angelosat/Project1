namespace Start_a_Town_
{
    class Tool : Entity
    {
        public Tool(ItemDef def)
            : base(def)
        {
            this.AddComponent(new ResourcesComponent(ResourceDefOf.Durability));
            this.AddComponent(new OwnershipComponent());
            this.AddComponent(new ToolAbilityComponent());
        }
        public override GameObject Create()
        {
            return new Tool(this.Def);
        }
    }
}
