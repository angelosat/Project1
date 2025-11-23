using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class Atlas : AtlasBase
    {
        public HashSet<Node.Token> ToLoad = new();
       
        public string Name { get; protected set; }
        public Atlas(string name)
        {
            this.Name = name;
        }

        readonly Dictionary<string, Node.Token> Tokens = new();

        public Node.Token Load(string assetPath, bool grayscale = false)
        {
            if (this.Tokens.TryGetValue(assetPath + (grayscale ? "-grayscale" : ""), out Node.Token token))
                return token;
            token = new Node.Token(this, assetPath, grayscale);
            this.Tokens.Add(token.Name, token);
            this.ToLoad.Add(token);
            return token;
        }
       
        static readonly int Size = 1024;//512
                               //int Width, Height;

        Node RootNode = new(0, 0, Size, Size);
        
        public void Add(Node.Token token)
        {
            this.AddTexture(token.Texture, out token.Rectangle);
        }

        public Node AddTexture(Texture2D tex, out Rectangle rect)
        {
            Node emptyNode = this.RootNode.FindNode(tex.Width, tex.Height); // find first empty node that can fit it
            if (emptyNode == null)
                throw new Exception("Could not pack texture to atlas!");
            emptyNode.Texture = tex;
            emptyNode.Split(tex.Width, tex.Height);
            rect = new Rectangle(emptyNode.X, emptyNode.Y, tex.Width, tex.Height);
            return emptyNode;
        }

        List<Node.Token> Sort()
        {
            var sorted = this.ToLoad.ToList();
            sorted.Sort((tex1, tex2) =>
            {
                int volume1 = tex1.Texture.Width * tex1.Texture.Height;
                int volume2 = tex2.Texture.Width * tex2.Texture.Height;
                if (volume1 <= volume2)
                    return 1;
                else
                    return -1;
            });
            return sorted;
        }

        public void OnDeviceLost()
        {
            this.Bake();
        }

        internal void Initialize()
        {
            this.RootNode = new Node(0, 0, Size, Size);
            var sorted = this.Sort();
            foreach (var tokens in sorted)
                this.Add(tokens);
            this.Bake();
        }

        internal void Bake()
        {
            var gfx = Game1.Instance.GraphicsDevice;
            var texture = new RenderTarget2D(gfx, Size, Size);
            gfx.SetRenderTarget(texture);
            gfx.Clear(Color.Transparent);
            var sb = new SpriteBatch(gfx);
            sb.Begin();
            var nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                if (node.Texture == null)
                    continue;
                Vector2 loc = new(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.Texture, loc, Color.White);
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new(GlobalVars.SaveDir + this.Name + "Atlas.png", FileMode.OpenOrCreate))
            {
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                stream.Close();
            }
            this.Texture = texture.ToTexture();
        }
    }
}
