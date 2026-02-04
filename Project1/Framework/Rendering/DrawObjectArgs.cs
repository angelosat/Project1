using Microsoft.Xna.Framework;
using Project1.Framework.Input;
using Project1.Framework.WorldGen;
using Project1.Framework.Entities;

namespace Project1.Framework.Rendering
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
