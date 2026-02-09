using Project1.Core.Plants;
using Project1.Core.Interactions;

namespace Project1.Core.Components.Plants
{
    internal class InteractionHarvestLogic : InteractionLogic
    {
        public override bool CanPerform(InteractionContext ctx)
        {
            var plant = ctx.Target.Object;
            if (plant.Map != ctx.Actor.Map)
                return false;
            if (!plant.TryGetComponent<PlantComponent>(out var comp))
                throw new System.Exception();
            if (!comp.IsHarvestable)
                return false;
            return true;
        }
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient)
                return;
            var plant = i.Context.Target.Object;
            if (!plant.TryGetComponent<PlantComponent>(out var comp))
                throw new System.Exception();
            //if (target.Object is not Plant plant)
            //    throw new System.Exception();
            //plant.PlantComponent.Harvest(actor);
            if(!comp.IsHarvestable)
                throw new System.Exception();
            comp.HarvestBy(actor);
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
            plant.PlantComponent.HarvestBy(a);
        }
    }
}
