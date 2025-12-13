using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    public abstract class Entity : GameObject
    {
        private SpriteComp _sprite;
        [InspectorHidden]
        public SpriteComp Sprite => this._sprite ??= this.GetComponent<SpriteComp>();

        /// <summary>
        /// here or in tool class?
        /// </summary>
        ToolAbilityComponent _toolComponent;
        [InspectorHidden]
        public ToolAbilityComponent ToolComponent => this._toolComponent ??= this.GetComponent<ToolAbilityComponent>();

        GearComponent _gear;
        [InspectorHidden]
        public GearComponent Gear => this._gear ??= this.GetComponent<GearComponent>();

        OwnershipComponent _ownership;
        [InspectorHidden]
        public OwnershipComponent Ownership => this._ownership ??= this.GetComponent<OwnershipComponent>();

        //public override GameObject Create()
        //{
        //    return new Entity();
        //}
        public Entity()
        {
            this.AddComponent(new PositionComponent());
            this.AddComponent(new DefComponent());
            this.AddComponent(new PhysicsComponent());
            //this.AddComponent(new SpriteComp()); // add this only through comp props
        }
        public Entity(ItemDef def) : this()
        {
            this.Def = def;
            //this.AddComponent(new SpriteComp(def));
        }
        internal GameObjectSlot GetEquipmentSlot(GearType.Types type)
        {
            return this.Gear.GetSlot(GearType.Dictionary[type]);
        }


        internal void InitComps()
        {
            foreach (var prop in this.Def.CompProps)
            {
                var comp = prop.CreateComp();
                prop.Apply(comp);
                this.AddComponent(comp);
            }
            this.Components.Resolve();
        }

        internal bool ProvidesSkill(ToolUseDef skill)
        {
            return this.ToolComponent?.ToolProperties?.ToolUse == skill;
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
                c.Initialize(this, materials);
            return this;
        }
        internal Entity SetQuality(Quality quality)
        {
            if (this.Def.QualityLevels)
                foreach (var c in this.Components.Values)
                    c.Initialize(this, quality);
            return this;
        }

        

        public GameObject Randomize(RandomThreaded random)
        {
            if (this.Def.CraftingProperties is not null) // HACK
            {
                var mats = ItemFactory.GetRandomMaterialsFor(this.Def);
                this.SetMaterials(mats);
                this.SetQuality(Quality.GetRandom());
            }
            foreach (var comp in this.Components.Values)
                comp.Randomize(this, random);
            return this;
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
        }
    }
}
