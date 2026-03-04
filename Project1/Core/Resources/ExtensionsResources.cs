using Project1.Core.Entities.Actors;
using Project1.Core.Entities;

namespace Project1.Core.Resources
{
    static class ExtensionsResources
    {
        static public bool HasResource(this Entity entity, ResourceDef type)
            => entity.HasResource(type);
        
        //static public Resource GetResource(this GameObject entity, ResourceDef def) => entity.GetComponent<ResourcesComponent>()?.GetResource(def);
        static public IResourceView GetResource(this Entity entity, ResourceDef def) => entity.GetComponent<ResourcesComponent>()?.View(def);

        static public IResourceView GetHealth(this Actor actor) => actor.GetResource(ResourceDefOf.Health);
        static public void AdjustHealth(this Actor actor, int value) => actor.GetResource(ResourceDefOf.Health).ApplyDelta(value);
        static public IResourceView GetStamina(this Actor actor) => actor.GetResource(ResourceDefOf.Stamina);
    }
}
