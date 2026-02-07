using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Entities;

namespace Project1.Core.Base
{
    public class SceneState
    {
        public HashSet<GameObject> ObjectsDrawn { get; set; }
        public Dictionary<GameObject, Rectangle> ObjectBounds { get; set; }
        public SceneState()
        {
            this.ObjectBounds = new Dictionary<GameObject, Rectangle>();
            this.ObjectsDrawn = new HashSet<GameObject>();
        }
    }
}
