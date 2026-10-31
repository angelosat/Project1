using Project1.Core.Entities;
using Project1.Core.Entities.Actors;

namespace Project1.Core.World.Frontiers
{
    public interface IWorldSpaceManager
    {
        //void Enter(Actor actor);
        void Exit(Actor actor);
        //FrontierDef PlaceAtRandom(Entity entity);
        //FrontierDef PlaceAt(Entity entity, WorldSpacePosition pos);
        FrontierDef PlaceAtRandom(Entity entity);
        void PlaceAt(Entity entity, WorldSpacePosition pos);
        FrontierRuntime GetFrontier(Entity entity);
        FrontierRuntime Get(FrontierDef def);
        void Tick();
        FrontierDef FrontierAt(WorldSpacePosition pos);
    }
}
