using Project1.Core.Attributes;
using Project1.Core.Entities.Stats.ValueGetters;
using Project1.Core.Stats;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Entities.Stats
{
    public sealed class StatDef : Def
    {
        public enum Types { Scalar, Percentile };
        public static StatDef[] ToolStatPackage { get; } = { StatDefOf.ToolEffectiveness, StatDefOf.ToolSpeed };
        public static StatDef[] NpcStatPackage { get; } = { StatDefOf.MaxHaulWeight, StatDefOf.Encumberance, StatDefOf.WalkSpeed, StatDefOf.Armor };

        public float BaseValue;
        public string Description;
        public Types Type = Types.Scalar;
        public string StringFormat = "";
        readonly Type ValueGetterType;
        StatWorker _valueGetter;
        public StatWorker Worker => this._valueGetter ??= ActivatorSafe<StatWorker>.CreateInstance(this.ValueGetterType);

        public StatDef(string name) : base(name)
        {

        }
        public StatDef(string name, Type valueGetter) : base(name)
        {
            this.ValueGetterType = valueGetter;
        }
        float ApplyModifiers(Entity parent, float value)
        {
            var mods = parent.GetStatModifiers(this);
            if (mods is not null)
                foreach (var mod in mods)
                    value = mod.Def.Mod(parent, value);
            return value;
        }
        public float CalculateFor(Entity parent)
        {
            if (this.Type == Types.Scalar)
            {
                var value = this.Worker.CalculateStat(parent);
                var modified = this.ApplyModifiers(parent, value);
                return this.BaseValue + modified;
            }
            else if (this.Type == Types.Percentile)
            {
                return this.ApplyModifiers(parent, 1);
            }
            else throw new Exception();
        }
        public Control GetControl(Entity parent)
        {
            return new Label()
            {
                TextFunc = () => $"{this.Name}: {this.CalculateFor(parent)}",
            };
        }

        public abstract class ValueBuilder : Def
        {
            public ValueBuilder(string name) : base(name)
            {
            }
            protected abstract float BaseGet(Entity parent);
            protected List<Expression> Expressions = new();

            public float Get(Entity parent)
            {
                var val = this.BaseGet(parent);
                foreach (var exp in this.Expressions)
                    val = exp.Perform(val);
                return val;
            }

            public class ExpressionDef : Def
            {
                internal Func<float, float, float> Operator;
                public ExpressionDef(string name, string label, Func<float, float, float> op) : base(name)
                {
                    this.Operator = op;
                }

                public static readonly ExpressionDef Division = new("Division", "/", (a, b) => a / b);
            }
            protected class Expression
            {
                readonly ExpressionDef Def;
                readonly float Value;

                public Expression(ExpressionDef def, float val)
                {
                    this.Def = def;
                    this.Value = val;
                }
                public float Perform(float a)
                {
                    return this.Def.Operator(a, this.Value);
                }

            }
        }
        class ValueBuilderFromAttribute : ValueBuilder
        {
            readonly AttributeDef Def;
            public ValueBuilderFromAttribute(AttributeDef def) : base("AttributeGetter")
            {
                this.Def = def;
            }
            protected override float BaseGet(Entity parent)
            {
                return parent.GetAttribute(this.Def)?.Level ?? 0;
            }

            internal ValueBuilder DivideBy(int v)
            {
                this.Expressions.Add(new Expression(ExpressionDef.Division, 2));
                return this;
            }
        }
    }
}
