using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Terraforming.Mutators;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public abstract class Terraformer : ISaveable, ISerializable
    {
        public TerraformerDef Def;
       
        public Action<RandomThreaded, WorldBase, Cell, int, int, int>
            Finalize = (r, w, c, x, y, z) => { };

        public virtual void Initialize(WorldBase w, Cell c, int x, int y, int z, double g) { }
        protected virtual void Finally(Chunk chunk) { }
        internal virtual void Finally(Chunk newChunk, Dictionary<IntVec3, double> gradients)
        {
            this.Finally(newChunk);
        }

        public override string ToString()
        {
            return this.Def.Label;
        }
        public virtual void Generate(MapBase map) { }//, Dictionary<IntVec3, double> gradients) { }

        static Terraformer() { }
        public static List<TerraformerDef> Defaults = new() { 
            TerraformerDefOf.Normal,
            TerraformerDefOf.Sea,
            TerraformerDefOf.PerlinWorms, // caves after sea because we dont want underwater caves filled with water
            TerraformerDefOf.Minerals, 
            TerraformerDefOf.Grass, 
            TerraformerDefOf.Flowers, 
            TerraformerDefOf.Trees 
        };

        protected static int GetRandom(byte[] seedArray, int x, int y, int z, int min, int max)
        {
            var seed = Generator.Perlin3D(x, y, z, 16, seedArray);
            var random = new Random(seed.GetHashCode());
            var r = random.NextDouble();
            var val = min + (int)Math.Floor((max - min) * r);
            return val;
        }
        protected static int GetRandom(int seed, int min, int max)
        {
            var random = new Random(seed);
            var r = random.NextDouble();
            var val = min + (int)Math.Floor((max - min) * r);
            return val;
        }

        public virtual IEnumerable<TerraformerProperty> GetAdjustableParameters() { yield break; }

        public static readonly Table<TerraformerProperty> GuiTable = new Table<TerraformerProperty>() { RowColor = Color.Black * .5f }// { Spacing = 0 }
            .AddColumn("name", 128, m => new Label(m.Name + ": "), 1)
            .AddColumn("slider", 196, m => new SliderNew(() => m.Value, v => m.Value = v, 200, m.Min, m.Max, m.Step, m.Format))
            .AddColumn("value", 32, m => new Label(() => m.Value.ToString(m.Format)) { Anchor = new(.5f, 0) }, .5f)
            .AddColumn("reset", 16, m => IconButton.CreateSmall(Icon.Replace, m.ResetValue).ShowOnParentFocus(true).SetHoverText("Reset to default"));

        public GroupBox GetUI()
        {
            var box = new GroupBox();
            var props = this.GetAdjustableParameters();
            GuiTable.ClearControls();
            GuiTable.AddItems(props);
           
            box.AddControlsVertically(GuiTable);
            return box;
        }

        public virtual Terraformer SetWorld(WorldBase w) { return this; }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }

        public ISaveable Load(SaveTag tag)
        {
            this.Def = tag.LoadDef<TerraformerDef>("Def");
            if (this.Def == null)
                throw new Exception();
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag tag) { }

        public void Write(IDataWriter w)
        {
            this.Def.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(IDataWriter w) { }

        public ISerializable Read(IDataReader r)
        {
            this.Def = r.ReadDef<TerraformerDef>();
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(IDataReader r) { }

    }
}
