using Microsoft.Xna.Framework;
using Project1.Core.Simulation;
using Project1.Framework;
using System;
using System.Linq;

namespace Project1.Core.Entities
{
    public abstract class EntityBase : Inspectable
    {
        public abstract MapBase Map { get; }
        public abstract Vector3 Global { get; }
    }
}
