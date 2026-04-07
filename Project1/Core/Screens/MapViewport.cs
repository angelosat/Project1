using Project1.Core.Entities;
using Project1.Core.Simulation;

#nullable enable

namespace Project1.Core.Screens;

internal sealed class MapViewport(MapBase map, Camera camera)
{
    internal MapBase Map = map;
    internal Camera Camera = camera;
    float FogT;
    Entity? FollowTarget;

    internal void Update(int gameSpeed)
    {
        this.UpdateFog(gameSpeed);
    }
    internal void ToggleFollow(Entity entity)
    {
        //this.FollowTarget = entity;
        this.Camera.ToggleFollowing(entity);
    }
    void UpdateFog(int gameSpeed)
    {
        this.FogT = (this.FogT + 0.05f * gameSpeed) % 100;
    }
}
