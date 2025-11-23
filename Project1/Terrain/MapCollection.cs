using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class MapCollection : Dictionary<Vector2, MapBase>
    {
        public override string ToString()
        {
            return Count.ToString();
        }
    }
}
