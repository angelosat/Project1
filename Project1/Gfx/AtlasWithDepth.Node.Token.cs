using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class AtlasWithDepth
    {
        public partial class Node : Inspectable
        {
            public class Token : IAtlasNodeToken
            {
                public string Name { get; set; }
                public Node Node { get; set; }
                
                public Texture2D Texture { get; set; }
                public Texture2D DepthTexture { get; set; }
                public Texture2D DepthMask { get; set; }

                public bool Billboard;
                public float BillboardLayer;
                public bool GeneratedDepth;

                public void OnDeviceReset()
                {
                    if (!this.GeneratedDepth)
                        return;
                    if (this.Billboard)
                        this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.BillboardLayer);
                    else
                        this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.DepthMask);
                }

                public void SetRectangle(Rectangle rect)
                {
                    this.Rectangle = rect;
                    this.TopLeftUV = new Vector2(rect.Left / (float)AtlasWithDepth.Size, rect.Top / (float)AtlasWithDepth.Size);
                    this.TopRightUV = new Vector2(rect.Right / (float)AtlasWithDepth.Size, rect.Top / (float)AtlasWithDepth.Size);
                    this.BottomLeftUV = new Vector2(rect.Left / (float)AtlasWithDepth.Size, rect.Bottom / (float)AtlasWithDepth.Size);
                    this.BottomRightUV = new Vector2(rect.Right / (float)AtlasWithDepth.Size, rect.Bottom / (float)AtlasWithDepth.Size);
                }

                public Color[] ColorArray { get; set; }
                public Token(AtlasWithDepth atlas, string name)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.GeneratedDepth = true;
                    this.Billboard = true;
                    this.BillboardLayer = 0.5f; // WARNING
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.BillboardLayer);
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, string depthname)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + depthname);
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, Texture2D depthtexture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthtexture);
                    this.DepthMask = depthtexture;
                    this.GeneratedDepth = true;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasWithDepth atlas, string name, float depthLayer)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthLayer);
                    this.GeneratedDepth = true;
                    this.Billboard = true;
                    this.BillboardLayer = depthLayer;
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                Color[] CreateColorArray(Texture2D tex)
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
