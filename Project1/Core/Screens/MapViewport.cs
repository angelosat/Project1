using Project1.Core.Entities;
using Project1.Core.Input;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace Project1.Core.Screens
{
    internal class MapViewport
    {
        internal MapBase Map;
        internal Camera Camera;
        float FogT;
        Entity? FollowTarget;
        public MapViewport(MapBase map, Camera camera)
        {
            this.Map = map;
            this.Camera = camera;

        }

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
}
