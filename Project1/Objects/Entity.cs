using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.Animations;
using Project1.Framework.Components;
using Project1.Framework.Entities;
using Project1.Framework.Gear;
using Project1.Framework.Materials;
using Project1.Framework.Net;
using Project1.Framework.Physics;
using Project1.Framework.Skills;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public /*abstract*/ class Entity : GameObject
    {
        bool _initialized;
        //public override GameObject Create() => new Entity();
        public SpriteComp Sprite => this.GetComponent<SpriteComp>();
        /// <summary>
        /// here or in tool class?
        /// </summary>
        public ToolComp ToolComponent => this.GetComponent<ToolComp>();

        public GearComponent Gear => this.GetComponent<GearComponent>();

        public OwnershipComponent Ownership => this.GetComponent<OwnershipComponent>();

        //public ItemVariantDef Variant { get; internal set; }
        public Entity()
        {
            this.AddComponent(new PositionComponent());
            this.AddComponent(new DefComponent());
            this.AddComponent(new PhysicsComponent());
            this.AddComponent(new SpriteComp()); // add this only through comp props
        }
        //public Entity(int stackSize): this() { this._stackSize = stackSize >= 0 ? stackSize : StackMax; }
        public Entity(ItemDef def, int amount) : base(def, amount)
        {
            //this.Def = def;
            //this.AddComponent(new SpriteComp(def));
            this.AddComponent(new PositionComponent());
            this.AddComponent(new DefComponent());
            this.AddComponent(new PhysicsComponent());
            this.AddComponent(new SpriteComp()); // add this only through comp props
        }
        //internal GameObjectSlot GetEquipmentSlot(GearTypeDef type) => this.Gear.GetSlot(type);
        //{
        //    return this.Gear.GetSlot(GearTypeDef.Dictionary[type]);
        //}
        public Entity Initialize()
        {
            if (this._initialized)
                throw new InvalidOperationException($"{this} initialized twice");
            this.Components.Initialize();
            return this;
        }

        
        internal void InitComps(ItemDef def)
        {
            this.Components.CreateAndResolve(def);
            this.EnumerateSlots();
        }
        internal bool ProvidesSkill(ToolUseDef skill)
        {
            return this.ToolComponent?.ToolUse == skill;
        }

        internal MaterialDef GetMaterial(BoneDef def)
        {
            return this.Sprite.GetMaterial(def);
        }
        internal virtual GameObject SetName(string v)
        {
            this.Name = v;
            return this;
        }

        internal Texture2D RenderIcon(int scale = 1)
        {
            return this.Body.RenderIcon(this, scale);
        }

        internal Entity SetMaterial(MaterialDef mat)
        {
            foreach (var c in this.Components.Values)
                c.SetMaterial(mat);
            this.Name = $"{mat.Prefix}";
            if (!this.Def.ReplaceName)
                this.Name += $" {this.Def.Label}";
            //this.Name = $"{mat.Prefix} {this.Def.Label}";
            mat.Apply(this);
            return this;
        }
        internal Entity SetMaterials(Dictionary<string, MaterialDef> materials)
        {
            foreach (var c in this.Components.Values)
                c.ApplyMaterials(this, materials);
            return this;
        }
        internal Entity SetQuality(Quality quality)
        {
            if (this.Def.QualityLevels)
                foreach (var c in this.Components.Values)
                    c.ApplyQuality(this, quality);
            return this;
        }

        

        public GameObject Randomize(RandomThreaded random)
        {
            if (this.Def.CraftingProperties is not null) // HACK
            {
                //var mats = ItemFactory.GetRandomMaterialsFor(this.Def);
                //this.SetMaterials(mats);
                //this.RandomizeMaterials();
                this.SetQuality(Quality.GetRandom());
            }
            foreach (var comp in this.Components.Values)
                comp.Randomize(this, random);
            //foreach (var comp in this.Components.Values)
            //    comp.Resolve();
            return this;
        }
        void RandomizeMaterials()
        {
            var random = new Random();
            var materials = Project1.Framework.Base.Def.GetDefs<MaterialDef>().ToArray();
            foreach(var bone in this.Body.GetAllBones())
                bone.Material = materials.SelectRandom(random);
        }
        internal void Select()
        {
            SelectionManager.Select(this);
        }
        /// <summary>
        /// reset name in case of errors or def changes
        /// </summary>
        internal void ResetName()
        {
            this.DefComponent.ParentName = this.Def.NameGetter?.Invoke(this) ?? this.DefComponent.ParentName; // reset name
            this.Name = this.Profile?.Label ?? this.Def.Label;
        }
        internal void Resolve()
        {
            this.Components.Resolve();
        }
        public Entity Take(int? amount)
        {
            if (!amount.HasValue)
                return this;
            if (amount.Value == this.StackSize)
                return this;
            ArgumentOutOfRangeException.ThrowIfGreaterThan(amount.Value, this.StackSize);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount.Value);
            return this.Split(amount.Value) as Entity;
        }

        internal void ApplySpecs(List<EntityComp.Spec> overrides)
        {
            this.Components.ApplySpecs(overrides);
        }
        public bool IsDead { get; private set; }
        internal void Kill()
        {
            this.IsDead = true;
            foreach (var comp in this.Components.Values)
                comp.OnKill();
            this.Map.Events.Post(new EntityKilledEvent(this));
        }

        public BoundingBox GetBoundingBoxNext()
        {
            var x = this.Global.X + this.Velocity.X;
            var y = this.Global.Y + this.Velocity.Y;
            var z = this.Global.Z + this.Velocity.Z;
            return new(
                new(x - .25f, y - .25f, z),
                new(x + .25f, y + .25f, z + this.Def.Height));
        }
    }
}
