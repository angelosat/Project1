using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Project1.Core.Entities
{
    public class SceneState
    {
        public readonly HashSet<Entity> ObjectsDrawn = [];
        public readonly Dictionary<Entity, Rectangle> ObjectBounds = [];
        
    }
}
