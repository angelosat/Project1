using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public partial class AtlasWithDepth : AtlasBase
    {
        public HashSet<Node.Token> ToLoad = new HashSet<Node.Token>();
        public string Name { get; protected set; }
        public AtlasWithDepth(string name)
        {
            this.Name = name;
        }

        readonly Dictionary<string, Node.Token> Tokens = new();
        
        public Node.Token Load(string assetName)
        {
            if (this.Tokens.TryGetValue(assetName, out Node.Token token))
                return token;
            token = new Node.Token(this, assetName);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, string depthMap)
        {
            if (this.Tokens.TryGetValue(assetName, out Node.Token token))
                return token;
            token = new Node.Token(this, assetName, depthMap);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, Texture2D depthtexture)
        {
            if (this.Tokens.TryGetValue(assetName, out Node.Token token))
            {
                return token;
            }
            token = new Node.Token(this, assetName, depthtexture);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }
        public Node.Token Load(string assetName, float depthLayer)
        {
            if (this.Tokens.TryGetValue(assetName, out Node.Token token))
            {
                return token;
            }
            token = new Node.Token(this, assetName, depthLayer);
            this.Tokens.Add(assetName, token);
            this.ToLoad.Add(token);
            return token;
        }

        static public readonly int Size = 1024;
        Node RootNode = new(0, 0, Size, Size);
       
        public void Add(Node.Token token)
        {
            this.AddTexture(token, out Rectangle rect);
            token.SetRectangle(rect);
            return;
        }

        public Node AddTexture(Node.Token token, out Rectangle rect)
        {
            Texture2D tex = token.Texture;
            Node emptyNode = this.RootNode.FindNode(tex.Width, tex.Height); // find first empty node that can fit it
            if (emptyNode == null)
                throw new Exception("Could not pack texture to atlas!");
            emptyNode.CurrentToken = token;
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

        internal void OnDeviceLost()
        {
            foreach (var token in this.Tokens.Values)
                token.OnDeviceReset();
            this.Bake();
        }

        internal void Initialize()
        {
            this.RootNode = new Node(0, 0, Size, Size);
            var sorted = this.Sort();
            foreach (var tokens in sorted)
                this.Add(tokens);
            //this.Bake(); // i currently bake the texture when the device resets, and the device resets when the game starts, so it will get baked anyway at the start of the first draw frame
        }

        internal void Bake()
        {
            var gfx = Game1.Instance.GraphicsDevice;
            var render = new RenderTarget2D(gfx, Size, Size);
            var depthrender = new RenderTarget2D(gfx, Size, Size);

            gfx.SetRenderTarget(render);
            gfx.Clear(Color.Transparent);
            SpriteBatch sb = new SpriteBatch(gfx);
            sb.Begin();
            var nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                if(node.CurrentToken is null)
                    continue;
                Vector2 loc = new(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.Texture, loc, Color.White);
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new FileStream(GlobalVars.SaveDir + this.Name + "Atlas.png", FileMode.OpenOrCreate))
            {
                render.SaveAsPng(stream, render.Width, render.Height);
                stream.Close();
            }

            //bake depth texture
            gfx.SetRenderTarget(depthrender);
            gfx.Clear(Color.Transparent);
            sb = new SpriteBatch(gfx);
            sb.Begin();
            nodes = this.RootNode.GetChildren();

            foreach (var node in nodes)
            {
                if (node.CurrentToken is null)
                    continue;
                Vector2 loc = new(node.Rectangle.X, node.Rectangle.Y);
                sb.Draw(node.DepthTexture, loc, Color.White);
            }
            sb.End();
            gfx.SetRenderTarget(null);
            using (FileStream stream = new FileStream(GlobalVars.SaveDir + this.Name + "AtlasDepth.png", FileMode.OpenOrCreate))
            {
                depthrender.SaveAsPng(stream, depthrender.Width, depthrender.Height);
                stream.Close();
            }
            this.DepthTexture = depthrender.ToTexture();
            this.Texture = render.ToTexture();
        }

        public void Begin(Effect fx)
        {
            fx.Parameters["s"].SetValue(this.Texture);
            fx.Parameters["s1"].SetValue(this.DepthTexture);
        }
    }
}
