using Microsoft.Xna.Framework;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Loot
{
    internal class LootSystem : SimulationSystem
    {
        public LootSystem(MapBase map) : base(map)
        {
            map.Events.ListenTo<LootDropEvent>(OnLootDropNew);
        }

        private void OnLootDropNew(LootDropEvent e)
        {
            if (e.Map.Net.IsClient)
                return;
            var rng = Server.Instance.GetRandom();
            var global = e.Global;
            var vel = e.Velocity;
            foreach (var entity in e.Entities)
            {
                Server.Instance.World.Register(entity);
                var velocity = vel + RandomPopVelocity(rng);
                Server.Instance.Map.Spawn(entity, global, velocity);
            }
        }

        static public Vector3 RandomPopVelocity(RandomThreaded random)
        {
            double angle = random.NextDouble() * (Math.PI + Math.PI);
            double w = Math.PI / 4f;

            float verticalForce = .3f;
            float horizontalForce = .1f;
            float x = horizontalForce * (float)(Math.Sin(w) * Math.Cos(angle));
            float y = horizontalForce * (float)(Math.Sin(w) * Math.Sin(angle));
            float z = verticalForce * (float)Math.Cos(w);

            var direction = new Vector3(x, y, z);
            return direction;
        }
    }
}