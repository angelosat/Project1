using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class PlantComponent : EntityComponent
    {
        const float ForageThreshold = .5f;

        public override string Name { get; } = "Plant";

        Progress GrowthBody = new(0, 100, 5);
        Progress GrowthFruit = new(0, 100, 0);

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
        float GrowthRate => (this.Parent.Map.Sunlight - .5f) * 2;

        int GrowthTick, FruitGrowthTick;
        public enum GrowthStates { Growing, Ready }
        public Growth Growth = new(.05f);
        PlantProperties _plantProps;
        public PlantProperties PlantProperties
        { 
            get => this._plantProps;
            set
            {
                this._plantProps = value;
                if (this.Parent is not null)
                    this.UpdateParent();
                //var parent = this.Parent;
                //var hitpoints = parent.GetResource(ResourceDefOf.HitPoints);
                //hitpoints.Max = value.StemMaterial.Density;
                //hitpoints.TicksPerRecoverOne = value.StemHealRate;

                //this._spriteFruit = _plantProps.TextureFruit is string fruitTexturePath ? Sprite.Load(fruitTexturePath) : null;

                //var body = parent.Body;
                //body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
                //body.Sprite = Sprite.Load(_plantProps.TextureGrowing);
                //if(body.TryFindBone(BoneDefOf.PlantFruit, out this._fruitBone))
                //    this._fruitBone.Material = _plantProps.FruitMaterial;
                //this.UpdateFruitTexture();
            }
        }
       
        public override void OnObjectCreated(GameObject parent)
        {
            if (this.PlantProperties is not null)
                this.UpdateParent();
        }
        private void UpdateParent()
        {
            var parent = this.Parent;
            var plant = this.PlantProperties;
            var hitpoints = parent.GetResource(ResourceDefOf.HitPoints);
            hitpoints.Max = plant.StemMaterial.Density;
            hitpoints.TicksPerRecoverOne = plant.StemHealRate;

            this._spriteFruit = _plantProps.TextureFruit is string fruitTexturePath ? Sprite.Load(fruitTexturePath) : null;

            var body = parent.Body;
            body.ScaleFunc = () => .25f + .75f * this.GrowthBody.Percentage;
            body.Sprite = Sprite.Load(_plantProps.TextureGrowing);
            if (body.TryFindBone(BoneDefOf.PlantFruit, out this._fruitBone))
                this._fruitBone.Material = _plantProps.FruitMaterial;
            this.UpdateFruitTexture();
        }

        void UpdateFruitTexture()
        {
            if (_spriteFruit is not null && this.Parent.Body.TryFindBone(BoneDefOf.PlantFruit, out var fruitBone) && this.IsHarvestable)
                fruitBone.Sprite = this._spriteFruit;
        }
        float Length;
        Progress Progress;
        public int Level;

        public PlantComponent()
        {
            this.Progress = new Progress();
        }
        public void SetProperties(PlantProperties props)
        {
            this.PlantProperties = props;
            this.Length = props.GrowTicks;
            this.Progress = new Progress(0, this.Length, 0);
        }
        public PlantComponent(PlantProperties props)
        {
            this.PlantProperties = props;
        }

        internal void SetGrowth(float growth, float fruitGrowth)
        {
            this.GrowthBody.Percentage = growth;
            this.GrowthFruit.Percentage = fruitGrowth;
        }

        public PlantComponent(float fruitGrowTicks)
        {
            this.Length = fruitGrowTicks;
            this.Progress = new Progress(0, this.Length, 0);
        }

        public PlantComponent(PlantComponent toCopy)
        {
            this.GrowthBody = new Progress(toCopy.GrowthBody);
            this.GrowthFruit = new Progress(toCopy.GrowthFruit);
        }

        public override void OnObjectLoaded(GameObject parent)
        {
            parent.Body.Sprite = Sprite.Load(this.IsHarvestable ? this.PlantProperties.TextureGrown : this.PlantProperties.TextureGrowing);
            this.UpdateFruitTexture();
        }
        
        public override void Tick()
        {
            var parent = this.Parent;
            var growthRate = this.PlantProperties.GrowthRate;
            this.TickWiggle();
            var sunlight = this.Parent.Map.Sunlight;
            if (sunlight <= .5f)
                return;
            float growthAmount = GrowthRate;
            if (this.GrowthBody.Percentage >= ForageThreshold)
            {
                if (this.PlantProperties.ProducesFruit)
                    if (!this.GrowthFruit.IsFinished)
                    {
                        if (this.FruitGrowthTick++ >= growthRate)
                        {
                            this.FruitGrowthTick = 0;
                            var prevPercentage = this.GrowthFruit.Percentage;
                            //this.GrowthFruit.Value++;
                            this.GrowthFruit.Value += growthAmount;
                            if (this.IsHarvestable)
                            {
                                if (prevPercentage < ForageThreshold)
                                    parent.Net.EventOccured((int)Message.Types.PlantReady, parent);
                                //parent.Body.Sprite = this.PlantProperties.TextureGrown;
                                parent.Body.Sprite = Sprite.Load(this.PlantProperties.TextureGrown);
                                this.Parent.Body.FindBone(BoneDefOf.PlantFruit).Sprite = this._spriteFruit;
                            }
                        }
                    }
            }
            if (this.GrowthBody.IsFinished)
                return;
            if (this.GrowthTick++ >= growthRate)
            {
                this.GrowthTick = 0;
                //this.GrowthBody.Value++;
                this.GrowthBody.Value += growthAmount;
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
            var parent = this.Parent;
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

        public bool Harvest(GameObject parent, GameObject actor)
        {
            var plant = parent as Plant;
            var props = plant.PlantComponent.PlantProperties;
            if (props.Growth is null)
                return false;
            var yield = (int)(this.GrowthFruit.Percentage * props.Growth.MaxYieldHarvest);
            if (yield == 0)
                return false;

            if(parent.Net is Server server)
            {
                var product = props.Growth.CreateEntity();
                var rng = server.GetRandom();
                var velocity = LootManager.RandomPopVelocity(rng);
                parent.Map.World.Register(product);
                parent.Map.Spawn(product, parent.Global, velocity);
                PacketRegisterEntity.Send(product);
                PacketSpawnEntity.Send(product, parent.Global, velocity);
            }

            this.ResetFruitGrowth(parent);
            //parent.Net.EventOccured((int)Message.Types.PlantHarvested, parent);
            return true;
        }

        public void ChopDown(GameObject plant, Actor actor)
        {
            var plantdef = this.PlantProperties;
            var yield = (int)(this.GrowthBody.Percentage * plantdef.MaxYieldCutDown);
            if (plantdef.ProductCutDown != null && yield > 0)
            {
                if (actor.Net is Server server)
                {
                    var rng = server.GetRandom();
                    var product = plantdef.ProductCutDown.CreateFrom(plant.Body.Material ?? MaterialDefOf.LightWood).SetStackSize(yield) as Entity;
                    //actor.Net.PopLoot(product, plant.Global, plant.Velocity);

                    actor.Map.World.RegisterAndSync(product);
                    actor.Map.SpawnAndSync(product, plant.Global, LootManager.RandomPopVelocity(rng));


                    /// if the plant doesnt produce fruit, then the only seed source is by cutting the plant itself
                    if (!plantdef.ProducesFruit)
                    {
                        var seeds = plantdef.CreateSeeds().SetStackSize(yield) as Entity;
                        //actor.Net.PopLoot(seeds, plant.Global, plant.Velocity);
                        actor.Map.World.RegisterAndSync(seeds);
                        actor.Map.SpawnAndSync(seeds, plant.Global, LootManager.RandomPopVelocity(rng));
                    }
                }
            }
            actor.Map.Despawn(plant);
            actor.Map.World.DisposeEntityAndSync(plant as Entity);
            
            //plant.OnDespawn();
            //actor.Net.DisposeObject(plant);
        }

        private void ResetFruitGrowth(GameObject parent)
        {
            this.GrowthFruit.Value = 0;
            this.FruitGrowthTick = 0;
            parent.Body.Sprite = Sprite.Load(this.PlantProperties.TextureGrowing);
            this.Parent.Body.FindBone(BoneDefOf.PlantFruit).Sprite = null;
        }
        public override void OnSpawn(MapBase newMap)
        {
            newMap.Events.ListenTo<EntityCollisionEvent>(HandleCollisionEvent);
        }

        private void HandleCollisionEvent(EntityCollisionEvent e)
        {
            if (e.Target == this.Parent && this.Parent.Net.IsClient)
                this.Wiggle();
        }

        public void ResetGrowth(GameObject parent)
        {
            this.Progress.Value = this.Progress.Max = this.Length;
        }

        public override object Clone()
        {
            var newcomp = new PlantComponent(this) { PlantProperties = this.PlantProperties };
            newcomp.GrowthBody.Value = this.GrowthBody.Value;
            newcomp.GrowthFruit.Value = this.GrowthFruit.Value;
            return newcomp;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
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
        //internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        //{
        //    //info.AddInfo(UI.Label.ParseWrap("Sunlight: ", new Func<string>(() => $"{parent.Map.Sunlight:##0%}"), new Func<string>(()=> parent.Map.Sunlight > .5f ? "" : "(not growing)")));
        //    info.AddInfo(UI.Label.ParseWrap("Sunlight: ", new Func<string>(() => $"{parent.Map.Sunlight:##0%}")));
        //    info.AddInfo(UI.Label.ParseWrap("Growth rate: ", new Func<string>(() => $"{this.GrowthRate:##0%}")));
        //    info.AddInfo(new Bar(this.GrowthBody) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthBody.Percentage.ToString("##0%") });
        //    if (this.PlantProperties.ProducesFruit)
        //        info.AddInfo(new Bar(this.GrowthFruit) { Color = Color.MediumAquamarine, Name = "Fruit: ", TextFunc = () => this.GrowthFruit.Percentage.ToString("##0%") });
        //}
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            var guisunlight = UI.Label.ParseWrap("Sunlight: ", new Func<string>(() => $"{parent.Map.Sunlight:##0%}"));
            var guigrowth = UI.Label.ParseWrap("Growth rate: ", new Func<string>(() => $"{this.GrowthRate:##0%}"));
            var bargrowth = new Bar(this.GrowthBody) { Color = Color.MediumAquamarine, Name = "Growth: ", TextFunc = () => this.GrowthBody.Percentage.ToString("##0%") };
            var boxBars = new GroupBox().AddControls(bargrowth);

            if (this.PlantProperties.ProducesFruit)
                boxBars.AddControlsTopRight(1, new Bar(this.GrowthFruit) { Color = Color.MediumAquamarine, Name = "Fruit: ", TextFunc = () => this.GrowthFruit.Percentage.ToString("##0%") });

            info.AddInfo(new GroupBox().AddControlsVertically(1, boxBars, guisunlight, guigrowth));
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

        public override void Write(IDataWriter writer)
        {
            this.PlantProperties.Write(writer);
            this.GrowthFruit.Write(writer);
            this.GrowthBody.Write(writer);
        }
        public override void Read(IDataReader reader)
        {
            this.PlantProperties = Def.GetDef<PlantProperties>(reader.ReadString());
            this.GrowthFruit = new Progress(reader);
            this.GrowthBody = new Progress(reader);

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
            this.PlantProperties.Save(tag, "Plant");
            tag.Add(this.GrowthBody.Save("GrowthNew"));
            tag.Add(this.GrowthFruit.Save("FruitGrowth"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.PlantProperties = tag.LoadDef<PlantProperties>("Plant");
            tag.TryGetTag("GrowthNew", t => this.GrowthBody = new Progress(t));
            tag.TryGetTag("FruitGrowth", t => this.GrowthFruit = new Progress(t));
        }
        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            base.GetClientActions(parent, actions);
            actions.Add(new ContextAction("Debug: Grow", () => { return false; }));
        }

        internal bool IsHarvestable
        {
            get
            {
                var relevantProgress = this.PlantProperties.ProducesFruit ? this.GrowthFruit : this.GrowthBody;
                return relevantProgress.IsFinished;
            }
        }
        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(PlantComponent);
        }
    }
}
