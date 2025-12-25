namespace Start_a_Town_
{
    class Tool : Entity
    {
        public Tool()
        {
            
        }
        public Tool(ItemDef def, int amount)
            : base(def, amount)
        {
            this.AddComponent(new ResourcesComponent(ResourceDefOf.Durability));
            this.AddComponent(new OwnershipComponent());
            this.AddComponent(new ToolComp());
        }
        //public override GameObject Create()
        //{
        //    return new Tool(this.Def);
        //}
    }
}
