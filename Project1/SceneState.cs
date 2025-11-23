using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
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
