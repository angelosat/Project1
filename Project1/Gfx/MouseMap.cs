using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public class MouseMap
    {
        public Color[][][] Map, MapBack;
        public bool Multifaceted;
        public Texture2D Texture, TextureBack;
        
        public MouseMap(Texture2D texture, Rectangle[][] rect, bool multiFaceted = false)
        {
            this.Texture = texture;
            this.TextureBack = this.Texture;
            this.Map = new Color[rect.Length][][];
            this.MapBack = new Color[rect.Length][][];
            for (int i = 0; i < rect.Length; i++)
            {
                this.Map[i] = new Color[rect[i].Length][];
                this.MapBack[i] = new Color[rect[i].Length][];
                for (int j = 0; j < rect[i].Length; j++)
                {
                    this.Map[i][j] = new Color[texture.Width * texture.Height];
                    this.MapBack[i][j] = this.Map[i][j];
                    texture.GetData(0, null, Map[i][j], 0, texture.Width * texture.Height);
                }
            }
            this.Multifaceted = multiFaceted;
        }
        public MouseMap(Texture2D texture, Rectangle rect, bool multiFaceted = false)
        {
            this.Texture = texture;
            this.TextureBack = this.Texture;
            this.Map = new Color[1][][];
            this.Map[0] = new Color[1][];
            this.MapBack = new Color[1][][];
            this.MapBack[0] = new Color[1][];
            this.Map[0][0] = new Color[rect.Width * rect.Height];
            this.MapBack = this.Map;
            texture.GetData(0, rect, this.Map[0][0], 0, rect.Width * rect.Height);
            this.Multifaceted = multiFaceted;
        }
        public MouseMap(Texture2D front, Texture2D back, Rectangle rect, bool multiFaceted = false)
        {
            this.Texture = front;
            this.TextureBack = back;
            this.Map = new Color[1][][];
            this.Map[0] = new Color[1][];
            this.Map[0][0] = new Color[rect.Width * rect.Height];
            this.MapBack = new Color[1][][];
            this.MapBack[0] = new Color[1][];
            this.MapBack[0][0] = new Color[rect.Width * rect.Height];
            front.GetData(0, rect, Map[0][0], 0, rect.Width * rect.Height);
            back.GetData(0, rect, MapBack[0][0], 0, rect.Width * rect.Height);
            this.Multifaceted = multiFaceted;
        }

        public bool HitTestEarly(int x, int y)
        {
            Color[][][] map = Map;
            Color c = map[0][0][y * Texture.Width + x];
            return c.A > 0;
        }
       
        public bool HitTest(bool alt, int x, int y, out Vector3 vector, int variation = 0, int orientation = 0)
        {
            Color[][][] map = alt ? MapBack : Map;
            Color c = map[variation][orientation][y * Texture.Width + x];
            // see if it's better to use the "behind" key to just return the bottom face
            var sampled = new Vector3(c.R, c.G, c.B);
            vector = alt ? -sampled : sampled;
            return c.A > 0;
        }
        public bool HitTest(int x, int y, out Vector3 vector, int variation = 0, int orientation = 0)
        {
            bool alt = InputState.IsKeyDown(System.Windows.Forms.Keys.Menu);
            Color[][][] map = alt ? MapBack : Map;
            Color c = map[variation][orientation][y * Texture.Width + x];
            // see if it's better to use the "behind" key to just return the bottom face
            var sampled = new Vector3(c.R, c.G, c.B);
            vector = alt ? -sampled : sampled;
            return c.A > 0;
        }
    }
}
