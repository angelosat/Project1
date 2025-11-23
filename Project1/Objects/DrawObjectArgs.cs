using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class DrawObjectArgs
    {
        public Camera Camera;
        public Controller Controller;
        public MapBase Map;
        public Chunk Chunk;
        public Cell Cell;
        public Rectangle ScreenBounds, SpriteBounds;
        public GameObject Object;
        public float Depth;
        public Color Light;

        public DrawObjectArgs(Camera camera,
            Controller controller,
            MapBase map,
            Chunk chunk,
            Cell cell,
            Rectangle spriteBounds,
            Rectangle screenBounds,
            GameObject obj,
            Color color,
            float depth)
        {
            this.Camera = camera;
            this.Controller = controller;
            this.Map = map;
            this.Chunk = chunk;
            this.Cell = cell;
            this.SpriteBounds = spriteBounds;
            this.ScreenBounds = screenBounds;
            this.Object = obj;
            this.Depth = depth;
            this.Light = color;
        }
    }
}
