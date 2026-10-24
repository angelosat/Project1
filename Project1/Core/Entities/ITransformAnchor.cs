using Microsoft.Xna.Framework;
using Project1.Core.Simulation;
using System;
using System.Linq;

namespace Project1.Core.Entities
{
    public interface ITransformAnchor
    {
        public abstract MapBase Map { get; }
        public Vector3 Global { get; }
    }
}
