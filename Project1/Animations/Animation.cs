using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public sealed class Animation : Inspectable
    {
        public override string Label => this.Def.Label;
        public AnimationDef Def { get; private set; }
        public GameObject Entity;
        public bool Enabled;
        public float Weight = 1;
        public float WeightChange;
        public float Speed = 1;
        public float Frame;
        public float Layer => this.Def.Layer;
        public float Fade { get { return this.FadeValue / (float)this.FadeLength; } }
        public string Name;
        private bool PreFade;
        private int FadeLength;
        private int FadeValue;
        private Func<float, float, float, float> FadeInterpolation;
        public AnimationStates State;
        public Action FinishAction = () => { };
        public Action OnFadeOut = () => { };
        public Action OnFadeIn = () => { };
        public Animation(SaveTag tag)
        {
            this.Load(tag);
        }
        [Obsolete]
        public Animation(GameObject entity, string name, bool loop = false)
            : base()
        {
            this.Entity = entity;
            this.Name = name;
        }
        public Animation(AnimationDef def)
        {
            this.Def = def;
        }

        internal Animation SetWeight(int v)
        {
            this.Weight = v;
            return this;
        }

        public override string ToString()
        {
            return $"{this.Def.Name} f: {this.Frame} w: {this.Weight}";
        }

        public void Restart()
        {
            this.Frame = 0;
            this.Weight = 1;
            this.WeightChange = 0;
            this.State = AnimationStates.Running;
        }

        internal void FadeOutAndRemove()
        {
            this.WeightChange = -0.1f;
            this.State = AnimationStates.Removed;
        }
        internal void FadeOut(float perTick)
        {
            this.WeightChange = -perTick;
            this.State = AnimationStates.Finished;
        }
        internal void FadeOut()
        {
            this.State = AnimationStates.Finished;
            this.WeightChange = -0.1f;
        }
        internal void FadeOut(int seconds)
        {
            float frames = Ticks.PerSecond * seconds;
            float dw = 1 / frames;
            this.WeightChange = -dw;
            this.State = AnimationStates.Finished;
        }
        internal void Stop()
        {
            this.State = AnimationStates.Finished;
            this.Weight = 0;
            this.WeightChange = 0;
            this.FadeValue = 0;
            this.FadeLength = 0;
        }
        internal void FadeIn(bool preFade, int fadeLength, Func<float, float, float, float> interpolation)
        {
            this.PreFade = preFade;
            this.FadeLength = fadeLength;
            this.FadeValue = 0;
            this.FadeInterpolation = interpolation;
            this.Weight = 0;
        }

        public void Add(BoneDef type, AnimationClip animation)
        {
            throw new Exception();
        }
        public bool TryGetValue(BoneDef type, out AnimationClip ani)
        {
            return this.Def.KeyFrames.TryGetValue(type, out ani);
        }
        
        public void Update(GameObject entity)
        {
            if (this.FadeValue < this.FadeLength)
            {
                this.FadeValue++;
                this.Weight = this.FadeInterpolation(0, 1, this.Fade);
                if (this.Fade >= 1)
                    this.OnFadeIn();

                if (this.PreFade)
                    return;
            }
            if (this.Weight > 0)
            {
                var prevframe = this.Frame;
                this.Frame += this.Speed;
                this.PerformActions(prevframe, this.Frame, entity);
                if (this.Frame >= this.Def.FrameCount)
                {
                    switch (this.Def.WarpMode)
                    {
                        case WarpMode.Loop:
                            this.Frame -= this.Def.FrameCount;
                            break;

                        case WarpMode.Once:
                        case WarpMode.Clamp:
                            this.Frame = this.Def.FrameCount;
                            break;

                        default:
                            break;
                    }
                }
            }
            this.Weight += this.Def.WeightChangeFunc?.Invoke(entity) ?? this.WeightChange;
            this.Weight = MathHelper.Clamp(this.Weight, 0, 1);
        }
        private void PerformActions(float prevframe, float nextframe, GameObject entity)
        {
            if (this.State == AnimationStates.Removed)
                return;
            foreach (var action in this.Def.Events)
            {
                if (prevframe < action.Key && action.Key <= nextframe)
                {
                    action.Value(entity);
                }
            }
        }

        internal void GetValue(BoneDef boneType, ref Vector2 doff, ref float dang)
        {
            if (this.Def.KeyFrames.TryGetValue(boneType, out var clip))
                clip.GetValue(this.Frame, this.Fade, out doff, out dang);
        }
        internal bool TryGetValue(BoneDef boneType, ref Vector2 doff, ref float dang)
        {
            if (this.Def.KeyFrames.TryGetValue(boneType, out var clip))
            {
                clip.GetValue(this.Frame, this.Fade, out doff, out dang);
                return true;
            }
            return false;
        }

        [Obsolete]
        [InspectorHidden]
        static public Animation Block
        {
            get
            {
                throw new Exception();
            }
        }
        [Obsolete]
        static public Animation RaiseRHand(GameObject entity)
        {
            throw new Exception();
        }
       
        public void ExportToXml()
        {
            var doc = new XDocument();
            var root = new XElement("Animation");
            doc.Add(root);
            doc.Save(this.Name + ".xml");
        }

        static public void Export()
        {
        }

        internal SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.Frame.Save("Frame"));
            tag.Add(this.FadeValue.Save("FadeValue"));
            tag.Add(this.FadeLength.Save("FadeLength"));
            tag.Add(this.Weight.Save("Weight"));
            tag.Add(this.WeightChange.Save("WeightChange"));
            tag.Add(this.Speed.Save("Speed"));
            tag.Add(((int)this.State).Save("State"));
            return tag;
        }
        internal void Save(SaveTag tag, string name)
        {
            tag.Add(this.Save(name));
        }
        public void Load(SaveTag tag)
        {
            tag.TryGetTagValue<string>("Def", t => this.Def = Start_a_Town_.Def.GetDef<AnimationDef>(t));
            tag.TryGetTagValueOrDefault("Frame", out this.Frame);
            tag.TryGetTagValueOrDefault("FadeValue", out this.FadeValue);
            tag.TryGetTagValueOrDefault("FadeLength", out this.FadeLength);
            tag.TryGetTagValueOrDefault("Weight", out this.Weight);
            tag.TryGetTagValueOrDefault("WeightChange", out this.WeightChange);
            tag.TryGetTagValueOrDefault("Speed", out this.Speed);
            tag.TryGetTagValue<int>("State", t => this.State = (AnimationStates)t);
        }

        internal void Write(IDataWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.Frame);
            w.Write(this.FadeLength);
            w.Write(this.FadeValue);
            w.Write(this.Weight);
            w.Write(this.WeightChange);
            w.Write(this.Speed);
            w.Write((int)this.State);
        }
        internal void Read(IDataReader r)
        {
            this.Def = Start_a_Town_.Def.GetDef<AnimationDef>(r.ReadString());
            this.Frame = r.ReadSingle();
            this.FadeLength = r.ReadInt32();
            this.FadeValue = r.ReadInt32();
            this.Weight = r.ReadSingle();
            this.WeightChange = r.ReadSingle();
            this.Speed = r.ReadSingle();
            this.State = (AnimationStates)r.ReadInt32();
        }
    }
}
