using Microsoft.Xna.Framework;
using SharpDX.Direct3D9;
using Start_a_Town_.Graphics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Start_a_Town_
{
    public class BlockRendererObservable
    {
        readonly ObservableHashSet<TargetArgs> Cells;
        readonly Dictionary<int, MySpriteBatch> Slices = [];
        readonly HashSet<int> InvalidatedSlices = [];
        readonly AtlasDepthNormals.Node.Token BlockToken;

        public BlockRendererObservable(ObservableHashSet<TargetArgs> cells)
            : this(Block.BlockBlueprint, cells)
        {
        }
        public BlockRendererObservable(AtlasDepthNormals.Node.Token texToken, ObservableHashSet<TargetArgs> cells)
        {
            this.BlockToken = texToken;
            this.Cells = cells;
            this.Cells.CollectionChanged += this.Cells_CollectionChanged;
        }
        private void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
                foreach (var z in e.OldItems.Cast<TargetArgs>().Select(cell => (int)cell.Global.Z))
                    this.InvalidatedSlices.Add(z);
            if (e.NewItems is not null)
                foreach (var z in e.NewItems.Cast<TargetArgs>().Select(cell => (int)cell.Global.Z))
                    this.InvalidatedSlices.Add(z);
        }
        void Validate(Camera camera)
        {
            if (!this.InvalidatedSlices.Any())
                return;
            var bySlice = this.Cells.ToLookup(c => c.Global.Z);
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
                            this.BlockToken,
                            cell.Global);
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
    }

    //public class BlockRendererObservable
    //{
    //    readonly ObservableCollection<IntVec3> Cells;// = new();
    //    readonly Dictionary<int, MySpriteBatch> Slices = new();
    //    readonly HashSet<int> InvalidatedSlices = new();
    //    readonly AtlasDepthNormals.Node.Token BlockToken;

    //    public BlockRendererObservable(ObservableCollection<IntVec3> cells)
    //        : this(Block.BlockBlueprint, cells)
    //    {
    //    }
    //    public BlockRendererObservable(AtlasDepthNormals.Node.Token texToken, ObservableCollection<IntVec3> cells)
    //    {
    //        this.BlockToken = texToken;
    //        this.Cells = cells;
    //        this.Cells.CollectionChanged += this.Cells_CollectionChanged;
    //    }
    //    private void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    //    {
    //        if (e.OldItems is not null)
    //            foreach (var z in e.OldItems.Cast<IntVec3>().Select(cell => cell.Z))
    //                this.InvalidatedSlices.Add(z);
    //        if (e.NewItems is not null)
    //            foreach (var z in e.NewItems.Cast<IntVec3>().Select(cell => cell.Z))
    //                this.InvalidatedSlices.Add(z);
    //    }
    //    void Validate(Camera camera)
    //    {
    //        if (!this.InvalidatedSlices.Any())
    //            return;
    //        var bySlice = this.Cells.ToLookup(c => c.Z);
    //        foreach (var z in this.InvalidatedSlices)
    //        {
    //            if (!bySlice.Contains(z))
    //                this.Slices.Remove(z);
    //            else
    //            {
    //                var cells = bySlice[z];
    //                var slice = this.Slices.GetOrAdd(z, sliceCtor);
    //                slice.Clear();
    //                foreach (var cell in cells)
    //                    camera.DrawBlockSelectionGlobal(
    //                        slice,
    //                        this.BlockToken,
    //                        cell);
    //            }
    //        }
    //        this.InvalidatedSlices.Clear();

    //        static MySpriteBatch sliceCtor()
    //        {
    //            return new(Game1.Instance.GraphicsDevice);
    //        }
    //    }

    //    public void DrawBlocks(MapBase map, Camera camera)
    //    {
    //        this.Validate(camera);
    //        camera.PrepareShader(map);
    //        Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
    //        var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
    //        camera.Effect.Parameters["World"].SetValue(world);
    //        camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
    //        foreach (var slice in this.Slices)
    //            if (slice.Key <= camera.DrawLevel)
    //                slice.Value.Draw();
    //    }
    //}

}
