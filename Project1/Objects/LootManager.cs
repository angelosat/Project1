using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    internal class LootManager
    {
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
