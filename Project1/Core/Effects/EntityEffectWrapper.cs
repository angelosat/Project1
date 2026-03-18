using Project1.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using System;

namespace Project1.Core.Effects
{
    public record EntityEffectWrapper(EffectDef Def, Def Target, int? Budget, int TicksPerUnit) : ISaveableNewNew<EntityEffectWrapper>, ISerializableNew<EntityEffectWrapper>
    {
        bool _aborted;
        //public bool IsFinished => this.RemainingBudget.HasValue && this.RemainingBudget == 0;
        public bool IsFinished => this._aborted || (this.RemainingBudget.HasValue && this.RemainingBudget == 0);
        public float? RemainingBudget { get; private set; } = Budget;
        public float Consume(float budget)
        {
            if (!this.RemainingBudget.HasValue)
                return budget;
            var toConsume = Math.Min(budget, this.RemainingBudget.Value);
            this.RemainingBudget -= toConsume;
            return toConsume;
        }
        public bool IsInstant => this.TicksPerUnit == 0;
        internal void Tick(Actor actor)
        {
            this.Def.Worker.Tick(actor, this);
        }
        internal void Start(Actor actor) => this.Def.Worker.OnStart(actor, this);
        internal void Finish(Actor actor) => this.Def.Worker.OnFinish(actor, this);
        internal void Abort() => this._aborted = true;
        public static EntityEffectWrapper Create(IDataReader r)
        {
            var def = r.ReadDef<EffectDef>();
            var target = r.ReadDef();
            var value = r.ReadInt32();
            var rate = r.ReadInt32();
            return new(def, target, value, rate);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            this.Target.Save(tag, "Target");
            //this.Budget.Save(tag, "Value");
            this.TicksPerUnit.Save(tag, "Rate");
            return tag;
        }
        public Control GetGui()
        {
            return new Label($"Effect: {this.Def.Name}");
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            w.Write(this.Target);
            //w.Write(this.Budget);
            w.Write(this.TicksPerUnit);
        }
        public EntityEffectWrapper Read(IDataReader r) => throw new System.Exception();
        public static EntityEffectWrapper Create(SaveTag tag)
        {
            var def = tag.LoadDef<EffectDef>("Def");
            var target = tag.LoadDef<Def>("Target");
            var value = tag.LoadInt("Value");
            var rate = tag.LoadInt("Rate");
            return new EntityEffectWrapper(def, target, value, rate);
        }

       
    }
}
