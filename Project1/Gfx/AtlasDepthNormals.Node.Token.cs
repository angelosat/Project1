using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class AtlasDepthNormals
    {
        public partial class Node : Inspectable
        {
            public class Token : IAtlasNodeToken
            {
                public string Name { get; set; }
                public Node Node { get; set; }
                public Texture2D Texture { get; set; }
                public Texture2D DepthTexture { get; set; }
                public Texture2D NormalTexture { get; set; }

                public Texture2D DepthMask { get; set; }
                public Texture2D NormalMask { get; set; }

                public bool Billboard;
                public float BillboardLayer;
                public bool GeneratedDepthTexture;
                public bool GeneratedNormalTexture;

                public void OnDeviceReset()
                {
                    GenerateDepth();
                    GenerateNormal();
                }

                private void GenerateNormal()
                {
                    if (this.GeneratedNormalTexture)
                        this.NormalTexture = Borders.GenerateDepthTexture(this.Texture, this.NormalMask);
                }

                public void GenerateDepth()
                {
                    if (this.GeneratedDepthTexture)
                    {
                        if (this.Billboard)
                            this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.BillboardLayer);
                        else
                            this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, this.DepthMask);
                    }
                }
               
                public void SetRectangle(Rectangle rect)
                {
                    this.Rectangle = rect;
                    this.TopLeftUV = new Vector2(rect.Left / (float)AtlasDepthNormals.Size, rect.Top / (float)AtlasDepthNormals.Size);
                    this.TopRightUV = new Vector2(rect.Right / (float)AtlasDepthNormals.Size, rect.Top / (float)AtlasDepthNormals.Size);
                    this.BottomLeftUV = new Vector2(rect.Left / (float)AtlasDepthNormals.Size, rect.Bottom / (float)AtlasDepthNormals.Size);
                    this.BottomRightUV = new Vector2(rect.Right / (float)AtlasDepthNormals.Size, rect.Bottom / (float)AtlasDepthNormals.Size);
                }

                public Color[] ColorArray { get; set; }
                
                public Token(AtlasDepthNormals atlas, string name, string depthname, string normalmap)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + depthname);
                    this.NormalTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + normalmap);
                    this.ColorArray = CreateColorArray(this.Texture);
                }
                public Token(AtlasDepthNormals atlas, string name, string depthname, Texture2D normaltexture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + depthname);
                    this.NormalTexture = Borders.GenerateNormalTexture(this.Texture, normaltexture);
                    this.ColorArray = CreateColorArray(this.Texture);
                    this.GeneratedDepthTexture = true;
                    this.DepthMask = this.DepthTexture;
                    this.GeneratedNormalTexture = true;
                    this.NormalMask = normaltexture;
                }
                public Token(AtlasDepthNormals atlas, string name, Texture2D depthtexture, Texture2D normaltexture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = Game1.Instance.Content.Load<Texture2D>("Graphics/Items/" + name);
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthtexture);
                    this.NormalTexture = Borders.GenerateNormalTexture(this.Texture, normaltexture);
                    this.ColorArray = CreateColorArray(this.Texture);
                    this.GeneratedDepthTexture = true;
                    this.DepthMask = depthtexture;
                    this.GeneratedNormalTexture = true;
                    this.NormalMask = normaltexture;
                }
                public Token(AtlasDepthNormals atlas, string name, Texture2D texture, Texture2D depthtexture, Texture2D normaltexture)
                {
                    this.Atlas = atlas;
                    this.Name = name;
                    this.Texture = texture;
                    this.DepthTexture = Borders.GenerateDepthTexture(this.Texture, depthtexture);
                    this.NormalTexture = Borders.GenerateNormalTexture(this.Texture, normaltexture);
                    this.ColorArray = CreateColorArray(this.Texture);
                    this.GeneratedDepthTexture = true;
                    this.DepthMask = depthtexture;
                    this.GeneratedNormalTexture = true;
                    this.NormalMask = normaltexture;
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
