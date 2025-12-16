namespace Start_a_Town_.Components
{
    public class InteractionHarvest : Interaction
    {
        public InteractionHarvest()
            : base("Harvest", 2)
        {
            this.Verb = "Harvesting";
        }
        
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            if (t.Object is not Plant plant)
                throw new System.Exception();
            plant.PlantComponent.Harvest(t.Object, a);
        }
        public override object Clone()
        {
            return new InteractionHarvest();
        }
    }
}
