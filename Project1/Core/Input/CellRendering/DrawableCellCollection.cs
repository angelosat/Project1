using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Graphics;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Graphics;
using Project1.Framework.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Project1.Core.Input.CellRendering
{
    public sealed class DrawableCellCollection : ICollection<IntVec3>
    {
        readonly ObservableCollection<IntVec3> Cells = [];
        readonly Dictionary<int, MySpriteBatch> Slices = [];
        readonly HashSet<int> InvalidatedSlices = [];
        readonly AtlasDepthNormals.Node.Token BlockToken;
        Color _color = Color.White;
        public Color Color
        {
            get => this._color;
            set
            {
                this._color = value;
                this.Invalidate();
            }
        }
        public IntVec3 this[int index] => this.Cells[index];
        public int Count => ((ICollection<IntVec3>)this.Cells).Count;

        public bool IsReadOnly => ((ICollection<IntVec3>)this.Cells).IsReadOnly;

        public DrawableCellCollection()
            : this(Block.BlockBlueprint, Enumerable.Empty<IntVec3>())
        {
        }
        public DrawableCellCollection(AtlasDepthNormals.Node.Token texToken)
            : this(texToken, Enumerable.Empty<IntVec3>())
        {
        }
        public DrawableCellCollection(IEnumerable<IntVec3> cells)
            : this(Block.BlockBlueprint, cells)
        {
        }
        public DrawableCellCollection(AtlasDepthNormals.Node.Token texToken, IEnumerable<IntVec3> cells)
        {
            this.BlockToken = texToken;
            this.Cells.CollectionChanged += this.Cells_CollectionChanged;
            this.Add(cells);
        }
        public void Add(IEnumerable<IntVec3> cells)
        {
            foreach (var c in cells)
                this.Cells.Add(c);
        }
        private void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
                foreach (var z in e.OldItems.Cast<IntVec3>().Select(cell => cell.Z))
                    this.InvalidatedSlices.Add(z);
            if (e.NewItems is not null)
                foreach (var z in e.NewItems.Cast<IntVec3>().Select(cell => cell.Z))
                    this.InvalidatedSlices.Add(z);
        }
        void Validate(RenderContext ctx)
        {
            if (this.InvalidatedSlices.Count == 0)
                return;
            var renderer = ctx.Renderer;
            var camera = ctx.Camera;
            var view = ctx.View;
            var bySlice = this.Cells.ToLookup(c => c.Z);
            foreach (var z in this.InvalidatedSlices)
            {
                if (!bySlice.Contains(z))
                    this.Slices.Remove(z);
                else
                {
                    var cells = bySlice[z];
                    var slice = this.Slices.GetOrAdd(z, sliceCtor);
                    slice.Clear();
                    foreach (var cell in cells)
                        renderer.DrawBlockSelectionGlobal(
                            slice,
                            view,
                            cell,
                            this.BlockToken,
                            this._color
                            );
                }
            }
            this.InvalidatedSlices.Clear();

            static MySpriteBatch sliceCtor()
            {
                return new(Game1.Instance.GraphicsDevice);
            }
        }

        public void DrawBlocks(RenderContext ctx)
        {
            this.Validate(ctx);
            var renderer = ctx.Renderer;
            var view = ctx.View;
            renderer.PrepareShader(view);
            view.Rotate(0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
            Renderer.Effect.Parameters["World"].SetValue(world);
            Renderer.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            foreach (var slice in this.Slices)
                if (slice.Key <= ctx.View.Settings.DrawLevel)
                    slice.Value.Draw();
        }
        internal void Invalidate()
        {
            foreach (var z in this.Slices.Keys)
                this.InvalidatedSlices.Add(z);
        }
        public void Add(IntVec3 item)
        {
            ((ICollection<IntVec3>)this.Cells).Add(item);
        }

        public void Clear()
        {
            ((ICollection<IntVec3>)this.Cells).Clear();
        }

        public bool Contains(IntVec3 item)
        {
            return ((ICollection<IntVec3>)this.Cells).Contains(item);
        }

        public void CopyTo(IntVec3[] array, int arrayIndex)
        {
            ((ICollection<IntVec3>)this.Cells).CopyTo(array, arrayIndex);
        }

        public bool Remove(IntVec3 item)
        {
            return ((ICollection<IntVec3>)this.Cells).Remove(item);
        }

        public IEnumerator<IntVec3> GetEnumerator()
        {
            return ((IEnumerable<IntVec3>)this.Cells).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.Cells).GetEnumerator();
        }
    }

}
