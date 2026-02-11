using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Project1.Core.Entities
{
    public class SceneState
    {
        public readonly HashSet<GameObject> ObjectsDrawn = [];
        public readonly Dictionary<GameObject, Rectangle> ObjectBounds = [];
        
    }
}
