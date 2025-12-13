using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    public class MaterialDef : Def, ILabeled//, IInspectable
    {
        public static readonly Random Randomizer = new();

        public override string ToString()
        {
            return this.Name;
        }
        public string DebugName => $"Material:{this.Name}";

        public bool IsTemplate { get; private set; }

        public MaterialTypeDef Type;
        public Color Color;
        public Vector4 ColorVector;
        public string Prefix;
        public float Shine;
        public int Density;
        public bool EdibleRaw, EdibleCooked;
        public MaterialState State;
        public MaterialCategory Category;
        public Fuel Fuel;
        public float BreakResistance = 1;
        public int ValueBase = 1;
        public float ValueMultiplier = 1;

        /// <summary>
        /// How many ticks it takes for the liquid to flow to nearby cells
        /// </summary>
        internal int Viscosity;

        public int Value => (int)(this.ValueBase * this.ValueMultiplier);

        public MaterialDef()
        {
            this.IsTemplate = true;
        }
        public MaterialDef(MaterialTypeDef type, string name, string prefix, int density)
            : this(type, name, prefix, Color.White, density) { }
        public MaterialDef(string name, MaterialDef template) : base(name)
        {
            this.State = template.State;
            this.Category = template.Category;
            this.Density = template.Density;
            this.SetColor(template.Color);
            this.Type = template.Type;
            this.Type.AddMaterial(this);
            this.Fuel = template.Fuel;
        }

        public MaterialDef(MaterialTypeDef type, string name, string prefix, Color color, int density) : base(name)
        {
            this.Type = type;
            type.SubTypes.Add(this);
            this.Prefix = prefix;
            this.Density = density;
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, type.Shininess);
        }
        internal void Apply(Entity entity)
        {
        }

        public static MaterialDef CreateColor(Color color)
        {
            var mat = new MaterialDef(MaterialTypeDefOf.Dye, "Color", "Color", color, 1);
            return mat;
        }

        public MaterialDef SetColor(Color color)
        {
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, this.Shine);
            return this;
        }
        public MaterialDef SetDensity(int density)
        {
            this.Density = density;
            return this;
        }
        public MaterialDef SetReflectiveness(float reflectiveness)
        {
            this.Shine = reflectiveness;
            this.ColorVector = new Vector4(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f, reflectiveness);
            return this;
        }
        public MaterialDef SetState(MaterialState state)
        {
            this.State = state;
            return this;
        }

        public MaterialDef SetName(string name)
        {
            this.Name = name;
            return this;
        }
        public MaterialDef SetPrefix(string prefix)
        {
            if (this.IsTemplate)
                throw new Exception();
            this.Prefix = prefix;
            return this;
        }
        public MaterialDef SetValue(int value)
        {
            this.ValueBase = value;
            return this;
        }

        public MaterialDef SetType(MaterialTypeDef type)
        {
            this.Type = type;
            return this;
        }
    }
}
