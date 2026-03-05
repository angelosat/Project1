using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;
using System.Xml.Linq;

namespace Project1.Core.Animations
{
    public sealed class Animation : Inspectable, ISerializableNew<Animation>, ISaveableNewNew<Animation>
    {
        public override string LabelReadable => this.Def.LabelReadable;
        public AnimationDef Def { get; private set; }
        public Entity Entity;
        public bool Enabled;
        float _weight = 1;
        
        public float WeightChange;
        public float Speed = 1;
        public float Frame = -1;
        public double StartTick = -1;
        public float Layer => this.Def.Layer;
        public string Name;
        private bool PreFade;
        private int FadeLength;
        private float FadeValue;
        private Func<float, float, float, float> FadeInterpolation;
        public AnimationStates State;
        public Action FinishAction = () => { };
        public Action OnFadeOut = () => { };
        public Action OnFadeIn = () => { };
        public float Fade => this.FadeValue / (float)this.FadeLength;

        public float Weight
        {
            get => this.Def.WeightGetter?.Invoke(Entity) ?? this._weight;
            set => this._weight = value;
        }

        public Animation(SaveTag tag)
        {
            this.Load(tag);
        }
        [Obsolete]
        public Animation(Entity entity, string name, bool loop = false)
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
            this.StartTick = -1;
            this.Frame = 0;
            this.Weight = 1;
            this.WeightChange = 0;
            
            this.State = AnimationStates.Running;
        }

        internal void FadeOutAndRemove()
        {
            this.WeightChange = -0.1f;
            //this.State = AnimationStates.Removed;
            this.State = AnimationStates.Finishing;
        }
        internal void FadeOutAndRemove(float rate)
        {
            this.WeightChange = rate;
            //this.State = AnimationStates.Removed;
            this.State = AnimationStates.Finishing;
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
        public void Tick(Entity entity)
        {
            if (this.StartTick == -1)
                this.StartTick = entity.Net.CurrentTick;
            var prevFrame = this.Frame;
            var elapsedServerTicks = (float)(entity.Net.CurrentTick - this.StartTick);// / Server.ClockIntervalMS;
            //var elapsedTicks = elapsedServerTicks / this.Speed;
            var elapsedTicks = elapsedServerTicks * this.Speed;

            if (this.Speed > 0)
            {

                if (this.Weight > 0)
                {
                    this.Frame = elapsedTicks;

                    // Handle looping first, so frame delta is correct for events
                    //this.Loop();
                    var maxFrames = this.Def.FrameCount;

                    if (this.State != AnimationStates.Finishing)
                        // Fire keyframe events
                        this.PerformActionsNew(prevFrame, this.Frame, entity);
                }
                // Fade logic: now deterministic per server tick
                if (this.FadeValue < this.FadeLength)
                {
                    this.FadeValue = elapsedTicks; // directly proportional to elapsed time
                    this.Weight = this.FadeInterpolation(0, 1, this.Fade);
                    if (this.Fade >= 1)
                        this.OnFadeIn();

                    if (this.PreFade)
                        return; // optionally skip main update while fading in
                }
            }

            // Weight accumulation independent of frames
            //var step = elapsedServerTicks - prevFrame;
            var step = elapsedTicks - prevFrame;
                //this.Weight += step * (this.Def.WeightChangeFunc?.Invoke(entity) ?? this.WeightChange);
            this.Weight += step * (this.Def.WeightChangeFunc?.Invoke(entity) ?? this.WeightChange);
            this.Weight = MathHelper.Clamp(this.Weight, 0f, 1f);
            if (this.Weight == 0 && this.State == AnimationStates.Finishing)
                this.State = AnimationStates.Removed;
        }

        private void Loop()
        {
            if (this.Frame >= this.Def.FrameCount)
            {
                //if (this.State == AnimationStates.Finishing)
                //{
                //    this.Frame = this.Def.FrameCount;
                //    return;
                //}
                switch (this.Def.WarpMode)
                {
                    case WarpMode.Loop:
                        // math.max because there animations (like couch) with only 1 keyframe,
                        // and currently keyframe dont store duration,
                        // so framecount would be 0
                        //this.Frame %= Math.Max(1, this.Def.FrameCount);
                        if (this.Def.FrameCount > 0)
                            this.Frame %= this.Def.FrameCount;
                        else
                            this.Frame = 1;
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

        private void PerformActionsNew(float prevFrame, float nextFrame, Entity entity)
        {
            if(this.Def.WarpMode != WarpMode.Once)
            {
                prevFrame %= this.Def.FrameCount;
                nextFrame %= this.Def.FrameCount;
            }
            if (this.State == AnimationStates.Removed)
                return;
            foreach (var action in Def.Events)
            {
                float key = action.Key;
                // handle looping correctly
                if (prevFrame < key && key <= nextFrame)
                    action.Value(entity);
                else if (this.Def.WarpMode == Animations.WarpMode.Loop && prevFrame > nextFrame && (key > prevFrame || key <= nextFrame))
                    action.Value(entity);
            }
        }
        private void PerformActions(float prevframe, float nextframe, Entity entity)
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
                clip.GetValue(this, out doff, out dang);
        }
        internal bool TryGetValue(BoneDef boneType, ref Vector2 doff, ref float dang)
        {
            if (this.Def.KeyFrames.TryGetValue(boneType, out var clip))
            {
                clip.GetValue(this, out doff, out dang);
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

        public SaveTag Save(string name)
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
            tag.TryGetTagValue<string>("Def", t => this.Def = Core.Def.GetDef<AnimationDef>(t));
            tag.TryGetTagValueOrDefault("Frame", out this.Frame);
            tag.TryGetTagValueOrDefault("FadeValue", out this.FadeValue);
            tag.TryGetTagValueOrDefault("FadeLength", out this.FadeLength);
            tag.TryGetTagValueOrDefault("Weight", out this._weight);
            tag.TryGetTagValueOrDefault("WeightChange", out this.WeightChange);
            tag.TryGetTagValueOrDefault("Speed", out this.Speed);
            tag.TryGetTagValue<int>("State", t => this.State = (AnimationStates)t);
        }
        public static Animation Create(SaveTag tag)
        {
            var animation = new Animation();
            animation.Load(tag);
            return animation;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def);//.Name);
            w.Write(this.Frame);
            w.Write(this.FadeLength);
            w.Write(this.FadeValue);
            if(this.Def.WeightGetter is null)
                w.Write(this.Weight);
            w.Write(this.WeightChange);
            w.Write(this.Speed);
            w.Write((int)this.State);
        }
        public Animation Read(IDataReader r)
        {
            this.Def = Core.Def.GetDef<AnimationDef>(r.ReadString());
            this.Frame = r.ReadSingle();
            this.FadeLength = r.ReadInt32();
            this.FadeValue = r.ReadInt32();
            if(this.Def.WeightGetter is null)
                this.Weight = r.ReadSingle();
            this.WeightChange = r.ReadSingle();
            this.Speed = r.ReadSingle();
            this.State = (AnimationStates)r.ReadInt32();
            return this;
        }
        static public Animation Create(IDataReader r)
        {
            var animation = new Animation().Read(r);
            return animation;
        }
        public Animation(Animation source)
        {
            this.Def = source.Def;
            this.Frame = source.Frame;
            this.FadeLength = source.FadeLength;
            this.FadeValue = source.FadeValue;
            this.Weight = source.Weight;
            this.WeightChange = source.WeightChange;
            this.Speed = source.Speed;
            this.State = source.State;
        }

        public Animation()
        {
        }

        internal Animation Clone()
        {
            return new Animation(this);   
        }
        internal void Sync()
        {
            Packets.SyncAnimation(this.Entity as Entity, this);
        }

        internal static class Packets
        {
            static int _packetTypeId;
            static Packets()
            {
                _packetTypeId = Registry.PacketHandlers.Register(Receive);
            }

            internal static void SyncAnimation(Entity entity, Animation anim)
            {
                var server = entity.Net as Server;
                var w = server.BeginPacket(_packetTypeId);
                w.Write(entity.RefId);
                w.Write(anim.Def);
                anim.Write(w);
            }
            private static void Receive(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                var r = packet.PacketReader;
                var actor = client.World.GetEntity<Actor>(r.ReadInt32());
                var animDef = r.ReadDef<AnimationDef>();
                var anim = actor.SpriteComp.GetAnimation(animDef);
                anim.Read(r);
            }
        }
    }
}
