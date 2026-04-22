using Project1.Core.Entities.Actors;

namespace Project1.Core.Systems.Recipes;

static class Helpers_Recipes
{
    extension(Actor actor)
    {
        public RecipesComp Recipes => actor.GetComponent<RecipesComp>();
    }
}
