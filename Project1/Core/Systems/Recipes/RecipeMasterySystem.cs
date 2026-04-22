using Project1.Core.Simulation;
using Project1.Core.Systems.Crafting;

namespace Project1.Core.Systems.Recipes;

internal class RecipeMasterySystem : WorldComp
{
    public RecipeMasterySystem(WorldBase world) : base(world)
    {
        world.Events.ListenTo<ActorFinishedCraftingEvent>(HandleActorFinishedCrafting);
    }

    private void HandleActorFinishedCrafting(ActorFinishedCraftingEvent e)
    {
        var actor = e.Actor;
        var order = e.Order;
        var product = e.Product;
        actor.GetComponent<RecipesComp>().Add(product.Profile);
    }
}
