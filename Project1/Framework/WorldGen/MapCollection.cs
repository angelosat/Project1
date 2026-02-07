using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Simulation;

namespace Project1.Core.WorldGen
{
    public class MapCollection : Dictionary<Vector2, MapBase>
    {
        public override string ToString()
        {
            return Count.ToString();
        }
    }
}
