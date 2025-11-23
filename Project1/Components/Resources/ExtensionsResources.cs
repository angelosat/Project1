using Start_a_Town_.Components;

namespace Start_a_Town_
{
    static class ExtensionsResources
    {
        static public bool HasResource(this GameObject entity, ResourceDef type)
        {
            return entity.GetResource(type) is not null;
        }
        static public Resource GetResource(this GameObject entity, ResourceDef def) => entity.GetComponent<ResourcesComponent>()?.GetResource(def);
        static public void SyncAdjustResource(this GameObject entity, ResourceDef def, float val) => entity.GetComponent<ResourcesComponent>()?.GetResource(def).SyncAdjust(entity as Entity, val);

        static public Resource GetHealth(this Actor actor) => actor.GetResource(ResourceDefOf.Health);
        static public void AdjustHealth(this Actor actor, int value) => actor.GetResource(ResourceDefOf.Health).Adjust(value);
        static public Resource GetStamina(this Actor actor) => actor.GetResource(ResourceDefOf.Stamina);
    }
}
