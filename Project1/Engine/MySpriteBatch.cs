//////////////////////////////////////////////////////////////////////////////////////////////////
//   BEGIN MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class MySpriteBatch
    {
        public MyVertex[] vertices;
        int[] indices;
        int vertexCount = 0;
        int indexCount = 0;
        Texture2D texture;
        public GraphicsDevice Device;

        //  these should really be properties
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Effect Effect;
        public MySpriteBatch()
            : this(Game1.Instance.GraphicsDevice)
        {

        }
        public MySpriteBatch(GraphicsDevice device)
        {
            this.Device = device;
            this.vertices = new MyVertex[32]; //[256];
            this.indices = new int[vertices.Length * 3 / 2];
        }
        public MySpriteBatch(GraphicsDevice device, int size)
        {
            this.Device = device;
            this.vertices = new MyVertex[size * 4];
            this.indices = new int[vertices.Length * 3 / 2];
        }
        public void ResetMatrices(int width, int height)
        {
            this.World = Matrix.Identity;
            this.View = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            this.Projection = Matrix.CreateOrthographicOffCenter(
                0, width, -height, 0, 0, 1);
        }
        
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, float scale, Color color, SpriteEffects sprFx, float depth)
        {
            this.Draw(texture, screenPos, srcRectangle, rotation, origin, new Vector2(scale), color, sprFx, depth);
        }
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, Vector2 scale, Color color, SpriteEffects sprFx, float depth)
        {
            Matrix flip;
            if (sprFx == SpriteEffects.FlipHorizontally)
            {
                float flipOffset = (float)Math.Floor(srcRectangle.Width / 2f); 
                float flipOffset2 = (float)Math.Ceiling(srcRectangle.Width / 2f);
                flip = Matrix.CreateTranslation(new Vector3(-flipOffset, 0, 0)) * Matrix.CreateScale(new Vector3(-1, 1, 1)) * Matrix.CreateTranslation(new Vector3(flipOffset2, 0, 0));
            }
            else flip = Matrix.CreateScale(1);
            Matrix rotate = Matrix.CreateRotationZ(rotation);
            Matrix toScreen = Matrix.CreateTranslation(new Vector3(screenPos, 0));
            Matrix toOrigin = Matrix.CreateTranslation(new Vector3(-origin , 0));
            Matrix toScale = Matrix.CreateScale(scale.X, scale.Y, 1);
            Matrix final = flip * toOrigin * toScale * rotate * toScreen;
            
            Vector2 tl = new(0, 0);
            Vector2 tr = new(srcRectangle.Width, 0);
            Vector2 bl = new(0, srcRectangle.Height);
            Vector2 br = new(srcRectangle.Width, srcRectangle.Height);
            tl = Vector2.Transform(tl, final);
            tr = Vector2.Transform(tr, final);
            bl = Vector2.Transform(bl, final);
            br = Vector2.Transform(br, final);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            Color skylight = Color.White;
            Vector4 blocklight = Vector4.One;
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tl, depth)
                , color, skylight, blocklight, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tr, depth)
                , color, skylight, blocklight, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(br, depth)
                , color, skylight, blocklight, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(bl, depth)
                , color, skylight, blocklight, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, Vector2 scale, Color sky, Color block, Color color, Color fog, SpriteEffects sprFx, float depth)
        {
            Matrix flip;
            if (sprFx == SpriteEffects.FlipHorizontally)
            {
                float flipOffset = (float)Math.Floor(srcRectangle.Width / 2f);
                float flipOffset2 = (float)Math.Ceiling(srcRectangle.Width / 2f);
                flip = Matrix.CreateTranslation(new Vector3(-flipOffset, 0, 0)) * Matrix.CreateScale(new Vector3(-1, 1, 1)) * Matrix.CreateTranslation(new Vector3(flipOffset2, 0, 0));
            }
            else flip = Matrix.CreateScale(1);
            Matrix rotate = Matrix.CreateRotationZ(rotation);
            Matrix toScreen = Matrix.CreateTranslation(new Vector3(screenPos, 0));
            Matrix toOrigin = Matrix.CreateTranslation(new Vector3(-origin, 0));
            Matrix toScale = Matrix.CreateScale(scale.X, scale.Y, 1);
            Matrix final = flip * toOrigin * toScale * rotate * toScreen;
            Vector2 tl = new(0, 0);
            Vector2 tr = new(srcRectangle.Width, 0);
            Vector2 bl = new(0, srcRectangle.Height);
            Vector2 br = new(srcRectangle.Width, srcRectangle.Height);
            tl = Vector2.Transform(tl, final);
            tr = Vector2.Transform(tr, final);
            bl = Vector2.Transform(bl, final);
            br = Vector2.Transform(br, final);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;
            var blockvec4 = block.ToVector4();
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tl, depth)
                , fog, color, sky, blockvec4, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tr, depth)
                , fog, color, sky, blockvec4, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(br, depth)
                , fog, color, sky, blockvec4, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(bl, depth)
                , fog, color, sky, blockvec4, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, Vector2 scale, Color sky, Vector4 block, Color color, Color fog, SpriteEffects sprFx, float depth)
        {
            Matrix flip;
            if (sprFx == SpriteEffects.FlipHorizontally)
            {
                float flipOffset = (float)Math.Floor(srcRectangle.Width / 2f);
                float flipOffset2 = (float)Math.Ceiling(srcRectangle.Width / 2f);
                flip = Matrix.CreateTranslation(new Vector3(-flipOffset, 0, 0)) * Matrix.CreateScale(new Vector3(-1, 1, 1)) * Matrix.CreateTranslation(new Vector3(flipOffset2, 0, 0));
            }
            else flip = Matrix.CreateScale(1);
            Matrix rotate = Matrix.CreateRotationZ(rotation);
            Matrix toScreen = Matrix.CreateTranslation(new Vector3(screenPos, 0));
            Matrix toOrigin = Matrix.CreateTranslation(new Vector3(-origin, 0));
            Matrix toScale = Matrix.CreateScale(scale.X, scale.Y, 1);
            Matrix final = flip * toOrigin * toScale * rotate * toScreen;
            Vector2 tl = new(0, 0);
            Vector2 tr = new(srcRectangle.Width, 0);
            Vector2 bl = new(0, srcRectangle.Height);
            Vector2 br = new(srcRectangle.Width, srcRectangle.Height);
            tl = Vector2.Transform(tl, final);
            tr = Vector2.Transform(tr, final);
            bl = Vector2.Transform(bl, final);
            br = Vector2.Transform(br, final);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            vertices[vertexCount++] = new MyVertex(
                new Vector3(tl, depth)
                , fog, color, sky, block, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tr, depth)
                , fog, color, sky, block, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(br, depth)
                , fog, color, sky, block, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(bl, depth)
                , fog, color, sky, block, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void Draw(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float rotation, Vector2 origin, Vector2 scale, Color sky, Color block, Color material, Color tint, Color fog, SpriteEffects sprFx, float depth)
        {
            Matrix flip;
            if (sprFx == SpriteEffects.FlipHorizontally)
            {
                float flipOffset = (float)Math.Floor(srcRectangle.Width / 2f);
                float flipOffset2 = (float)Math.Ceiling(srcRectangle.Width / 2f);
                flip = Matrix.CreateTranslation(new Vector3(-flipOffset, 0, 0)) * Matrix.CreateScale(new Vector3(-1, 1, 1)) * Matrix.CreateTranslation(new Vector3(flipOffset2, 0, 0));
            }
            else flip = Matrix.CreateScale(1);
            Matrix rotate = Matrix.CreateRotationZ(rotation);
            Matrix toScreen = Matrix.CreateTranslation(new Vector3(screenPos, 0));
            Matrix toOrigin = Matrix.CreateTranslation(new Vector3(-origin, 0));
            Matrix toScale = Matrix.CreateScale(scale.X, scale.Y, 1);
            Matrix final = flip * toOrigin * toScale * rotate * toScreen;
            Vector2 tl = new Vector2(0, 0);
            Vector2 tr = new Vector2(srcRectangle.Width, 0);
            Vector2 bl = new Vector2(0, srcRectangle.Height);
            Vector2 br = new Vector2(srcRectangle.Width, srcRectangle.Height);
            tl = Vector2.Transform(tl, final);
            tr = Vector2.Transform(tr, final);
            bl = Vector2.Transform(bl, final);
            br = Vector2.Transform(br, final);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;
            var blocVec4 = block.ToVector4();
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tl, depth)
                , fog, tint, material, sky, blocVec4, Vector4.Zero, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(tr, depth)
                , fog, tint, material, sky, blocVec4, Vector4.Zero, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(br, depth)
                , fog, tint, material, sky, blocVec4, Vector4.Zero, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(bl, depth)
                , fog, tint, material, sky, blocVec4, Vector4.Zero, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void Draw(Texture2D texture, Rectangle srcRectangle, Rectangle dstRectangle, Color color)
        {
            //  if the texture changes, we flush all queued sprites.
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            Color light = Color.White;
            Vector4 blocklight = Vector4.One;
            // add the new vertices
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Left, dstRectangle.Top, 0)
                , color, light, blocklight, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Right, dstRectangle.Top, 0)
                , color, light, blocklight, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Right, dstRectangle.Bottom, 0)
                , color, light, blocklight, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Left, dstRectangle.Bottom, 0)
                , color, light, blocklight, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void DrawBlock(Texture2D texture, Vector2 screenPos, Rectangle srcRectangle, float scale, Color tint, Color sunLight, Vector4 blockLight, float depth)
        {
            Vector4 destRectangle = new Vector4(screenPos.X, screenPos.Y, screenPos.X + scale * srcRectangle.Width, screenPos.Y + scale * srcRectangle.Height);
            this.DrawBlock(texture, srcRectangle, destRectangle, tint, sunLight, blockLight, depth);
        }
        public void DrawBlock(Texture2D texture, Rectangle srcRectangle, Vector4 dstRectangle, Color tint, Color sunLight, Vector4 blockLight, float depth)
        {
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            //this.EnsureSpace(6, 4);
            if (indexCount + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + 6, indices.Length * 2));
            if (vertexCount + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + 4, vertices.Length * 2));

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.X, dstRectangle.Y, depth)
                , tint, sunLight, blockLight, GetUV(srcRectangle.Left, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Z, dstRectangle.Y, depth)
                , tint, sunLight, blockLight, GetUV(srcRectangle.Right, srcRectangle.Top));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Z, dstRectangle.W, depth)
                , tint, sunLight, blockLight, GetUV(srcRectangle.Right, srcRectangle.Bottom));
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.X, dstRectangle.W, depth)
                , tint, sunLight, blockLight, GetUV(srcRectangle.Left, srcRectangle.Bottom));
        }
        public void DrawBlock(Texture2D texture, Rectangle screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Color sunLight, Vector4 blockLight, float depth)
        {
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            //this.EnsureSpace(6, 4);
            if (indexCount + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + 6, indices.Length * 2));
            if (vertexCount + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + 4, vertices.Length * 2));

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y, depth)
                , fog, tint, sunLight, blockLight, token.TopLeftUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Width, screenBounds.Y, depth)
                , fog, tint, sunLight, blockLight, token.TopRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Width, screenBounds.Y + screenBounds.Height, depth)
                , fog, tint, sunLight, blockLight, token.BottomRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y + screenBounds.Height, depth)
                , fog, tint, sunLight, blockLight, token.BottomLeftUV);
        }
        public void DrawBlock(Texture2D texture, MapBase map, Vector3 global, IAtlasNodeToken token, Camera cam, Color fog, Color tint, Color sunLight, Vector4 blockLight)
        {
            float depth = global.GetDrawDepth(map, cam);
            Rectangle dstRectangle = cam.GetScreenBounds(global, Block.Bounds);
            this.DrawBlock(texture, dstRectangle, token, cam.Zoom, fog, tint, sunLight, blockLight, depth);
        }
        public void DrawBlock(Texture2D texture, Rectangle screenBounds, IAtlasNodeToken token, float scale, Color tint, Color sunLight, Vector4 blockLight, float depth)
        {
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            //this.EnsureSpace(6, 4);
            if (indexCount + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + 6, indices.Length * 2));
            if (vertexCount + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + 4, vertices.Length * 2));

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y, depth)
                , tint, sunLight, blockLight, token.TopLeftUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Width, screenBounds.Y, depth)
                , tint, sunLight, blockLight, token.TopRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Width, screenBounds.Y + screenBounds.Height, depth)
                , tint, sunLight, blockLight, token.BottomRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y + screenBounds.Height, depth)
                , tint, sunLight, blockLight, token.BottomLeftUV);
        }
        public void DrawBlock(Texture2D texture, Vector2 screenPos, IAtlasNodeToken token, float scale, Color tint, Color sunLight, Vector4 blockLight, float depth)
        {
            Vector4 dstRectangle = new Vector4(screenPos.X, screenPos.Y, screenPos.X + scale * token.Rectangle.Width, screenPos.Y + scale * token.Rectangle.Height);
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            //this.EnsureSpace(6, 4);
            if (indexCount + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + 6, indices.Length * 2));
            if (vertexCount + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + 4, vertices.Length * 2));

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.X, dstRectangle.Y, depth)
                , tint, sunLight, blockLight, token.TopLeftUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Z, dstRectangle.Y, depth)
                , tint, sunLight, blockLight, token.TopRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.Z, dstRectangle.W, depth)
                , tint, sunLight, blockLight, token.BottomRightUV);
            vertices[vertexCount++] = new MyVertex(
                new Vector3(dstRectangle.X, dstRectangle.W, depth)
                , tint, sunLight, blockLight, token.BottomLeftUV);
        }
        public MyVertex[] DrawBlock(Texture2D texture, Vector4 screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Color sunLight, Vector4 blockLight, float depth, Block block)
        {
            return this.DrawBlock(texture, screenBounds, token, scale, fog, tint, Color.White, sunLight, blockLight, Color.Transparent.ToVector4(), depth, block);
        }
        public MyVertex[] DrawBlock(Texture2D texture, Vector4 screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Color sunLight, Vector4 blockLight, float depth, Block block, Vector3 blockcoords)
        {
            return this.DrawBlock(texture, screenBounds, token, scale, fog, tint, Color.White, sunLight, blockLight, Color.Transparent.ToVector4(), depth, block, blockcoords);
        }
        public MyVertex[] DrawBlock(Texture2D texture, Vector4 screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Color material, Color sunLight, Vector4 blockLight, Vector4 water, float depth, Block block)
        {
            return this.DrawBlock(texture, screenBounds, token, scale, fog, tint, material.ToVector4(), sunLight, blockLight, water, depth, block, Vector3.Zero);
        }
        public MyVertex[] DrawBlock(Texture2D texture, Vector4 screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Color material, Color sunLight, Vector4 blockLight, Vector4 water, float depth, Block block, IntVec3 blockcoords)
        {
            return this.DrawBlock(texture, screenBounds, token, scale, fog, tint, material.ToVector4(), sunLight, blockLight, water, depth, block, blockcoords);
        }
        public MyVertex[] DrawBlock(Texture2D texture, Vector4 screenBounds, IAtlasNodeToken token, float scale, Color fog, Color tint, Vector4 material, Color sunLight, Vector4 blockLight, Vector4 water, float depth, Block block, IntVec3 blockCoords)
        {
            if (this.texture != null && this.texture != texture)
                this.Flush();
            this.texture = texture;

            //  ensure space for my vertices and indices.
            if (indexCount + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + 6, indices.Length * 2));
            if (vertexCount + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + 4, vertices.Length * 2));

            //  add the new indices
            indices[indexCount++] = vertexCount + 0;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 3;
            indices[indexCount++] = vertexCount + 1;
            indices[indexCount++] = vertexCount + 2;
            indices[indexCount++] = vertexCount + 3;

            var vertex1 = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y, depth)
                , fog, tint, material, sunLight, blockLight, water, token.TopLeftUV, block, blockCoords);
            var vertex2 = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Z, screenBounds.Y, depth)
                , fog, tint, material, sunLight, blockLight, water, token.TopRightUV, block, blockCoords);
            var vertex3 = new MyVertex(
                new Vector3(screenBounds.X + screenBounds.Z, screenBounds.Y + screenBounds.W, depth)
                , fog, tint, material, sunLight, blockLight, water, token.BottomRightUV, block, blockCoords);
            var vertex4 = new MyVertex(
                new Vector3(screenBounds.X, screenBounds.Y + screenBounds.W, depth)
                , fog, tint, material, sunLight, blockLight, water, token.BottomLeftUV, block, blockCoords);
            vertices[vertexCount++] = vertex1;
            vertices[vertexCount++] = vertex2; 
            vertices[vertexCount++] = vertex3; 
            vertices[vertexCount++] = vertex4;
            return new MyVertex[] { vertex1, vertex2, vertex3, vertex4 };
        }
       
        Vector2 GetUV(float x, float y)
        {
            return new Vector2(x / (float)texture.Width, y / (float)texture.Height);
        }

        void EnsureSpace(int indexSpace, int vertexSpace)
        {
            // it doubles the size
            if (indexCount + indexSpace >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + indexSpace, indices.Length * 2));
            if (vertexCount + vertexSpace >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + vertexSpace, vertices.Length * 2));
        }

        public void FlushOld()
        {
            if (this.vertexCount > 0)
            {
                Effect fx = Game1.Instance.Content.Load<Effect>("blur");
                fx.CurrentTechnique = fx.Techniques["Normal"];

                fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
                fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);
                fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
                fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
                fx.Parameters["Viewport"].SetValue(new Vector2(Device.Viewport.Width, Device.Viewport.Height));
                fx.Parameters["s"].SetValue(Block.Atlas.Texture);
                fx.Parameters["s2"].SetValue(Block.ShaderMouseMap);
                Device.DepthStencilState = DepthStencilState.Default;
                fx.CurrentTechnique.Passes["Pass1"].Apply();
                Device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                    this.indices, 0, this.indexCount / 3);

                this.vertexCount = 0;
                this.indexCount = 0;
            }
        }

        public void Flush()
        {
            if (this.vertexCount > 0)
            {
                Device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                    this.indices, 0, this.indexCount / 3);

                this.vertexCount = 0;
                this.indexCount = 0;
            }
        }
        public void Draw()
        {
            if (this.vertexCount > 0)
            {
                Device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                    this.indices, 0, this.indexCount / 3);
            }
        }
        public void Clear()
        {
            this.vertexCount = 0;
            this.indexCount = 0;
        }
    }
}
//////////////////////////////////////////////////////////////////////////////////////////////////
//   END MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////
