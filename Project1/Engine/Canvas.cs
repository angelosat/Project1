using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public class Canvas
    {
        public MySpriteBatch Opaque, NonOpaque, Transparent, Designations, WallHidable;
        public readonly MySpriteBatch[] MouseOverableMeshes;
        public Canvas(GraphicsDevice gd)
        {
            this.Opaque = new MySpriteBatch(gd);
            this.NonOpaque = new MySpriteBatch(gd);
            this.Transparent = new MySpriteBatch(gd);
            this.Designations = new MySpriteBatch(gd);
            this.WallHidable = new MySpriteBatch(gd);
        }
        public Canvas(GraphicsDevice gd, int size)
        {
            this.Opaque = new MySpriteBatch(gd, size);
            this.NonOpaque = new MySpriteBatch(gd, size);
            this.Transparent = new MySpriteBatch(gd, size);
            this.Designations = new MySpriteBatch(gd, size);
            this.WallHidable = new MySpriteBatch(gd, size);
        }
        public IEnumerable<MyVertex[]> GetMouseoverableMeshes()
        {
            yield return this.Opaque.vertices;
            yield return this.NonOpaque.vertices;
            yield return this.Designations.vertices;
            yield return this.WallHidable.vertices;
        }
    }
}
