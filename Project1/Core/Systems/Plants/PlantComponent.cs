using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Graphics;
using Project1.Core.Loot;
using Project1.Core.Resources;
using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Plants
{
    public sealed class PlantComponent : EntityComp<PlantComponent.Spec>
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Plant;
        public new class Spec : Spec<PlantComponent>
        {
            public int GrowthRate = Ticks.PerGameHour; //ticks per 1 growth
            public int GrowTicks;
            public int YieldThreshold;
            public int MaxYieldCutDown;
            public int StemHealRate;
            public Sprite TextureGrowing;
            public Sprite TextureGrown;
            public Sprite TextureFruit;
            public Sprite TextureSeeds;
            public MaterialDef StemMaterial;
            public MaterialDef FruitMaterial;
            public ItemDef ProductCutDown;
            public GrowthProperties Growth;
            public ToolUseDef ToolToCut;
            protected override void ApplyDefaultsTo(PlantComponent comp)
            {
                comp.Progress = new Progress(0, comp.Length, 0);
            }
        }
        const float HarvestThreshold = .5f;
        public override string Name { get; } = "Plant";
        public Progress GrowthBody = new(0, 100, 5);
        public Progress GrowthFruit = new(0, 100, 0);
        public void SetBodyGrowth(float percentage)
        {
            this.GrowthBody.Percentage = percentage;
           
        }
        public void SetFruitGrowth(float percentage)
        {
            this.GrowthFruit.Percentage = percentage;
            if (this._fruitBone is not null)
                this._fruitBone.Sprite = this.IsHarvestable ? this._spriteFruit : null;
        }
        Bone _fruitBone;
        float GrowthRate => (this.Owner.Map.Sunlight - .5f) * 2;

        int GrowthTick, FruitGrowthTick;
        public enum GrowthStates { Growing, Ready }
        public Growth Growth = new(.05f);
        public PlantSpeciesDef Species => this.Owner.Profile as PlantSpeciesDef;
        internal override void InitializeOnce()
        {
            if (this.Species is null)
                return;

            var parent = this.Owner;
            var plant = this.Species;
            var hitpoints = parent.GetResource(ResourceDefOf.HitPoints);
            hitpoints.Max = plant.StemMaterial.Density;
            hitpoints.TicksPerRecoverOne = plant.StemHealRate;

            var body = parent.Body;
            body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
            body.Sprite = Sprite.Load(this.Species.TextureGrowing);
            if (body.TryFindBone(BoneDefOf.PlantFruit, out this._fruitBone))
                this._fruitBone.Material = this.Species.FruitMaterial;
            this.UpdateFruitTexture();
        }
        internal override void ResolveReferencesNew()
        {
            this.UpdateFruitTexture();
        }
        internal override void Resolve()
        {
            var body = this.Owner.Body;
            body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
        }
        public void UpdateFruitTexture()
        {
            if (!this.Owner.Body.TryFindBone(BoneDefOf.PlantFruit, out var fruitBone))
                return;
            if (this.IsHarvestable)
            {
                this._spriteFruit = this.Species.TexturePlantFruitOverlay is string fruitTexturePath ? Sprite.Load(fruitTexturePath) : null;
                if (_spriteFruit is not null)
                    fruitBone.SetSprite(this._spriteFruit);
            }
            else
                fruitBone.SetSprite(null);
        }
        float Length => this.Species.GrowTicks;
        Progress Progress;
        public int Level;
        public PlantComponent()
        {
            this.Progress = new Progress();
        }
        internal void SetGrowth(float growth, float fruitGrowth)
        {
            this.GrowthBody.Percentage = growth;
            this.GrowthFruit.Percentage = fruitGrowth;
        }
        public PlantComponent(PlantComponent toCopy)
        {
            this.GrowthBody = new Progress(toCopy.GrowthBody);
            this.GrowthFruit = new Progress(toCopy.GrowthFruit);
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            this.Resolve();
        }
        const int debugGrowthMod = 100;
        public override void Tick()
        {
            var parent = this.Owner;
            var growthRate = this.Species.GrowthRate / debugGrowthMod;
            this.TickWiggle();
            var sunlight = this.Owner.Map.Sunlight;
            if (sunlight <= .5f)
                return;
            float growthStep = GrowthRate;
            if (this.GrowthBody.Percentage >= HarvestThreshold)
            {
                if (this.ProducesFruit)
                    if (!this.GrowthFruit.IsFinished)
                    {
                        if (this.FruitGrowthTick++ >= growthRate)
                        {
                            this.FruitGrowthTick -= growthRate;
                            var prevPercentage = this.GrowthFruit.Percentage;
                            //this.GrowthFruit.Value++;
                            this.GrowthFruit.Value += growthStep;
                            if (this.IsHarvestable)
                            {
                                if (prevPercentage < HarvestThreshold)
                                    parent.Map.Events.Post(new PlantHarvestableEvent(this.Owner));
                                parent.Body.Sprite = Sprite.Load(this.Species.TextureGrown);
                                this.Owner.Body.FindBone(BoneDefOf.PlantFruit).Sprite = this._spriteFruit;
                            }
                        }
                    }
            }
            if (this.GrowthBody.IsFinished)
                return;
            if (this.GrowthTick++ >= growthRate)
            {
                this.GrowthTick -= growthRate;
                this.GrowthBody.Value += growthStep;
            }
            return;
        }
        public void FinishGrowing(GameObject parent)
        {
            this.Growth.Set(parent, this.Growth.Max);
            this.Progress.Value = 0;
        }
        public void Wiggle()
        {
            this.Wiggle(WiggleAngleMaxDefault, WiggleTickMaxDefault, WiggleIntensityDefault);
        }
        public void Wiggle(float angle, int ticks, int speed)
        {
            this.WiggleTick = ticks;
            this.WiggleAngleMax = angle;
            this.WiggleIntensity = speed;
            this.WiggleDirection = (new int[] { -1, 1 })[new Random().Next(2)];
        }
        private void TickWiggle()
        {
            var parent = this.Owner;
            var t = 1 - this.WiggleTick--/ (float)this.WiggleTickMax;
            if (t >= 1)
                return;
            var currentdepth = (1 - t) * this.WiggleAngleMax;
            var radians = this.WiggleIntensity * t * Math.PI * 2;
            var currentangle = this.WiggleDirection * currentdepth * (float)Math.Sin(radians);
            parent.SpriteComp._Angle = currentangle;
        }
        private int WiggleTick;
        private readonly int WiggleTickMax = 40;
        private float WiggleAngleMax;
        const float WiggleAngleMaxDefault = (float)Math.PI / 4f;
        const int WiggleTickMaxDefault = 40;
        private int WiggleIntensity;
        const int WiggleIntensityDefault = 1;
        int WiggleDirection;
        private Sprite _spriteFruit;
        public bool HarvestBy(Actor actor)
        {
            var props = this.Species;
            if (props.Growth is null)
                return false;
            var yield = (int)(this.GrowthFruit.Percentage * props.Growth.MaxYieldHarvest);
            if (yield == 0)
                return false;
            var product = props.Growth.CreateEntity();
           
            this.Owner.Map.Events.Post(new LootDropEvent([product], this.Owner.Map, this.Owner.Global, this.Owner.Velocity));

            this.ResetFruitGrowth();
            this.Owner.Map.Events.Post(new PlantHarvestedEvent(this.Owner));
            return true;
        }
        internal override void OnKill()
        {
            var owner = this.Owner;
            var plantdef = this.Species;
            var yield = (int)(this.GrowthBody.Percentage * plantdef.MaxYieldCutDown);
            if (plantdef.ChoppingProduct != null && yield > 0)
            {
                var product = RawMaterialSystem.Create(MaterialRefinementDefOf.Logs, owner.Body.Material);
                product.SetStackSize(yield);
                owner.Map.Events.Post(new LootDropEvent([product], this.Owner.Map, this.Owner.Global, this.Owner.Velocity));

                /// if the plant doesnt produce fruit, then the only seed source is by cutting the plant itself
                if (!this.ProducesFruit)
                {
                    var seeds = PlantSystem.Create(this.Species, PlantStageDefOf.Seed);
                    seeds.SetStackSize(1);
                    owner.Map.Events.Post(new LootDropEvent([seeds], this.Owner.Map, this.Owner.Global, this.Owner.Velocity));
                }
            }
        }
        
        //public override void OnSpawn(MapBase newMap)
        //{
        //    newMap.Events.ListenTo<EntityCollisionEvent>(HandleCollisionEvent);
        //}

        //private void HandleCollisionEvent(EntityCollisionEvent e)
        //{
        //    if (e.Target == this.Owner && this.Owner.Net.IsClient)
        //        this.Wiggle();
        //}

        public void ResetGrowth(GameObject parent)
        {
            this.Progress.Value = this.Progress.Max = this.Length;
        }

        internal override void CopyFrom(EntityComp source)
        {
            var plantcomp = source as PlantComponent;
            this.GrowthBody.Value = plantcomp.GrowthBody.Value;
            this.GrowthFruit.Value = plantcomp.GrowthFruit.Value;
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.Controls.Add(new Bar()
            {
                Width = 200,
                Name = "Growth: ",
                Location = tooltip.Controls.BottomLeft,
                Object = this.GrowthBody,
                TextFunc = () => this.GrowthBody.Percentage.ToString("##0%")
            });
        }
    
        internal override IEnumerable<Control> GetSelectionInfo()
        {
            var guisunlight = Label.ParseWrap("Sunlight: ", new Func<string>(() => $"{this.Owner.Map.Sunlight:##0%}"));
            var guigrowth = Label.ParseWrap("Growth rate: ", new Func<string>(() => $"{this.GrowthRate:##0%}"));
            var bargrowth = new Bar(this.GrowthBody) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthBody.Percentage.ToString("##0%") };
            var boxBars = new GroupBox().AddControls(bargrowth);

            if (this.Species.ProducesFruit)
                boxBars.AddControlsTopRight(1, new Bar(this.GrowthFruit) { Color = Color.MediumAquamarine, Name = "Fruit: ", TextFunc = () => this.GrowthFruit.Percentage.ToString("##0%") });

            //info.AddInfo(new GroupBox().AddControlsVertically(1, boxBars, guisunlight, guigrowth));
            yield return boxBars;
            yield return guisunlight;
            yield return guigrowth;
        }
     
        string GrowthTimeSpan
        {
            get
            {
                var ts = TimeSpan.FromMilliseconds(1000 * this.Progress.Value / 60f);
                string fmt = "";
                if (ts.Hours > 0)
                    fmt += "%h'h '";
                if (ts.Minutes > 0)
                    fmt += "%m'm '";
                if (ts.Seconds > 0)
                    fmt += "%s's'";
                return ts.ToString(fmt);
            }
        }
        public void ResetFruitGrowth()
        {
            this.GrowthFruit.Value = 0;
            this.FruitGrowthTick = 0;
            this.Owner.Body.Sprite = Sprite.Load(this.Species.TextureGrowing);
            this.UpdateFruitTexture();
            this.Owner.World.Events.Post(new EntityCompUpdatedEvent(this));
        }
        public override void Write(IDataWriter w)
        {
            this.GrowthFruit.Write(w);
            this.GrowthBody.Write(w);
            w.Write(this.FruitGrowthTick);
            w.Write(this.GrowthTick);
        }
        public override void Read(IDataReader r)
        {
            this.GrowthFruit = new Progress(r);
            this.GrowthBody = new Progress(r);
            this.FruitGrowthTick = r.ReadInt32();
            this.GrowthTick = r.ReadInt32();
            this.UpdateFruitTexture();
        }
        internal override void SyncWrite(IDataWriter w)
        {
            this.Progress.Write(w);
            w.Write(this.Growth.Value);
        }
        internal override void SyncRead(GameObject parent, IDataReader r)
        {
            this.Progress.Read(r);
            this.Growth.Set(parent, r.ReadInt32());
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.GrowthBody.Save("GrowthNew"));
            tag.Add(this.GrowthFruit.Save("FruitGrowth"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("GrowthNew", t => this.GrowthBody = new Progress(t));
            tag.TryGetTag("FruitGrowth", t => this.GrowthFruit = new Progress(t));
        }
        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            base.GetClientActions(parent, actions);
            actions.Add(new ContextAction("Debug: Grow", () => { return false; }));
        }
        public bool ProducesFruit => this.Species.Growth?.GrowthItemDef == ItemDefOf.Fruit;

        internal bool IsHarvestable
        {
            get
            {
                var relevantProgress = this.ProducesFruit ? this.GrowthFruit : this.GrowthBody;
                return relevantProgress.IsFinished;
            }
        }
    }
}