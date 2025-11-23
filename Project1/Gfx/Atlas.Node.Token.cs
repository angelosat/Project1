using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class Atlas
    {
        public partial class Node
        {
            public class Token : IAtlasNodeToken
            {
                public string Name { get; set; }
                public Node Node { get; set; }
                public Texture2D Texture { get; set; }

                public Color[] ColorArray { get; set; }

                public Token(Atlas atlas, string path, bool grayscale = false)
                {
                    this.Atlas = atlas;
                    this.Name = path + (grayscale ? "-grayscale" : "");
                    if (Borders.Thickness > 0)
                        this.Texture = Borders.GenerateOutline(Game1.Instance.Content.Load<Texture2D>(path));
                    else
                        this.Texture = Game1.Instance.Content.Load<Texture2D>(path);
                    if (grayscale)
                        this.Texture = this.Texture.ToGrayscale();
                    this.ColorArray = CreateColorArray(this.Texture);
                }

                public Token(Atlas atlas, string name, Texture2D texture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = texture;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                Color[] CreateColorArray(Texture2D tex, bool grayscale = false)
                {
                    Rectangle source = tex.Bounds;
                    Color[] spriteMap = new Color[source.Width * source.Height];
                    tex.GetData(0, source, spriteMap, 0, source.Width * source.Height);
                    return spriteMap;
                }
                public override string ToString()
                {
                    return this.Name;
                }
            }
        }
    }
}
