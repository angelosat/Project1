using Project1.Framework.Base;
using Project1.Framework.WorldGen;
using System;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    public abstract class RoleMetaWrapper : ISaveableNewNew<RoleMetaWrapper>, ISerializableNew<RoleMetaWrapper>
    {
        public struct MetaDecision : ISaveableNewNew<MetaDecision>, ISerializableNew<MetaDecision>
        {
            ulong NextTick;
            int FailureStreak;

            internal bool CanEvaluate(ulong now) => now >= this.NextTick;
            internal void RegisterFailure()
            {
                this.FailureStreak++;
            }
            internal void RegisterSuccess()
            {
                this.FailureStreak = 0;
            }
            internal void ScheduleNext(WorldBase world)
            {
                var basedelay = (ulong)world.Random.Next(Ticks.FromHours(1), Ticks.FromHours(2));
                var damping = (ulong)(this.FailureStreak * Ticks.FromHours(1));
                this.NextTick = world.CurrentTick + basedelay + damping;
            }
            public static MetaDecision Create(SaveTag tag)
            {
                var nextTick = tag.LoadUlong("NextTick");
                var failureStreak = tag.LoadInt("FailureStreak");
                return new MetaDecision() { FailureStreak = failureStreak, NextTick = nextTick };
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                tag.Save("NextTick", this.NextTick);
                tag.Save("FailureStreak", this.FailureStreak);
                return tag;
            }

            public MetaDecision Read(IDataReader r)
            {
                this.NextTick = r.ReadUInt64();
                this.FailureStreak = r.ReadInt32();
                return this;
            }

            public void Write(IDataWriter w)
            {
                w.Write(this.NextTick);
                w.Write(this.FailureStreak);
            }

            public static MetaDecision Create(IDataReader r)
            {
                return new MetaDecision().Read(r);
            }
        }

        public Actor Actor;
        public RoleMetaDef Def;
        public FrontierDef TargetFrontier { get; private set; }
        public MetaDecision LocationDecision;
        internal virtual void AssignTo(Actor actor)
        {
            this.Actor = actor;
            actor.AI.Meta?.RemoveFrom(actor);
            actor.AI.Meta = this;
            actor.Needs.Add(this.Def.Needs);
        }

        private void RemoveFrom(Actor actor)
        {
            actor.Needs.Remove(this.Def.Needs);
        }

        internal virtual void Tick() => this.Def.Worker.Tick(this);

        internal void ReturnToTown()
        {
            this.TargetFrontier = null;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //tag.Add(this.LocationDecision.Save("LocationDecision"));
            tag.Save("LocationDecision", this.LocationDecision);
            tag.SaveDef("Def", this.Def);
            //var hasFront = this.TargetFrontier is not null;
            //tag.Save("HasTargetFrontier", hasFront);
            if (this.TargetFrontier is not null)
                tag.SaveDef("TargetFrontier", this.TargetFrontier);
            return tag;
        }

        public static RoleMetaWrapper Create(SaveTag tag)
        {
            var def = tag.LoadDef<RoleMetaDef>("Def");
            var wrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(def.WrapperType);
            wrapper.Def = def;
            wrapper.LocationDecision = tag.Load<MetaDecision>("LocationDecision");// MetaDecision.Create(tag["LocationDecision"]);
            //wrapper.TargetFrontier = tag.LoadDef<FrontierDef>("TargetFrontier");
            if (tag.TryLoadDefOut<FrontierDef>("TargetFrontier", out var frontDef)) wrapper.TargetFrontier = frontDef;
            return wrapper;
        }

        public RoleMetaWrapper Read(IDataReader r)
        {
            this.LocationDecision.Read(r);
            this.TargetFrontier = r.ReadDef<FrontierDef>();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            this.LocationDecision.Write(w);
            w.Write(this.TargetFrontier);
        }

        public static RoleMetaWrapper Create(IDataReader r)
        {
            var def = r.ReadDef<RoleMetaDef>();
            var wrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(def.WrapperType);
            wrapper.Read(r);
            return wrapper;
        }

        internal void SetTargetFrontier(FrontierDef frontier)
        {
            this.TargetFrontier = frontier;
            this.Actor.World.Events.Post(new AILocationDecisionEvent(this.Actor, frontier));
            
        }
    }
}
