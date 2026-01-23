namespace Start_a_Town_
{
    internal class InteractionHarvestLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient)
                return;
            var target = i.Context.Target;
            if (target.Object is not Plant plant)
                throw new System.Exception();
            plant.PlantComponent.Harvest(actor);
        }
    }
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
    }
}
