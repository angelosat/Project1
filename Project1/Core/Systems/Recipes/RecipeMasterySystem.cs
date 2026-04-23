using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Systems.Crafting;

namespace Project1.Core.Systems.Recipes;

public interface ICraftingPlugin
{
    void Handle(Actor actor, CraftingOrder order, Entity product);

    int GetQualityBonus(Actor actor, CraftingOrder order);
}

internal class CraftingPlugin_Skill : ICraftingPlugin
{
    public int GetQualityBonus(Actor actor, CraftingOrder order)
        => actor.Skills.GetLevel(order.WorkstationCapability.Worker.CraftingSkill);

    public void Handle(Actor actor, CraftingOrder order, Entity product)
    {
        //throw new System.NotImplementedException();
    }
}
internal class RecipeMasterySystem : WorldComp, ICraftingPlugin
{
    public RecipeMasterySystem():base(null)
    {
    }

    public RecipeMasterySystem(WorldBase world) : base(world)
    {
        //world.Events.ListenTo<ActorFinishedCraftingEvent>(HandleActorFinishedCrafting);
    }

    public int GetQualityBonus(Actor actor, CraftingOrder order)
        => actor.Recipes.Get(order.ProductDef);

    public void Handle(Actor actor, CraftingOrder order, Entity product)
    {
        actor.GetComponent<RecipesComp>().Add(product.Profile);
    }

    private void HandleActorFinishedCrafting(ActorFinishedCraftingEvent e)
    {
        //throw new System.Exception();
        //var actor = e.Actor;
        //var order = e.Order;
        //var product = e.Product;
        //actor.GetComponent<RecipesComp>().Add(product.Profile);
    }
}
