using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Project1.Framework.WorldGen
{
    public class MapCollection : Dictionary<Vector2, MapBase>
    {
        public override string ToString()
        {
            return Count.ToString();
        }
    }
}
