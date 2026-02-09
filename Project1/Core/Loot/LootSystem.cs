using System;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Core.Base;
using Project1.Core.Net;

namespace Project1.Core.Loot
{
    [EnsureStaticCtorCall]
    internal static class LootSystem
    {
        static LootSystem()
        {
            Registry.MapEventHooksServer.Register<LootPopEvent>(OnLootDrop);
        }

        private static void OnLootDrop(LootPopEvent e)
        {
            var rng = Server.Instance.GetRandom();
            var global = e.Source.Global;
            var sourceVelocity = e.Source.Velocity;
            foreach (var entity in e.Entities)
            {
                Server.Instance.World.Register(entity);//, true);
                var velocity = sourceVelocity + RandomPopVelocity(rng);
                Server.Instance.Map.Spawn(entity, global, velocity);//, true);
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
