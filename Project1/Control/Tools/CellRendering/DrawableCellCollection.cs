using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class DrawableCellCollection : ICollection<IntVec3>
    {
        readonly ObservableCollection<IntVec3> Cells = new();
        readonly Dictionary<int, MySpriteBatch> Slices = new();
        readonly HashSet<int> InvalidatedSlices = new();
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
        void Validate(Camera camera)
        {
            if (!this.InvalidatedSlices.Any())
                return;
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
                        camera.DrawBlockSelectionGlobal(
                            slice,
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

        public void DrawBlocks(MapBase map, Camera camera)
        {
            this.Validate(camera);
            camera.PrepareShader(map);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            foreach (var slice in this.Slices)
                if (slice.Key <= camera.DrawLevel)
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
