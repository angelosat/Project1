using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Framework.Entities;

namespace Project1.Framework.Base
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
