using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    public class ValueModifierValue
    {
        public string Name;
        public float Value;
        public ValueModifierValue(string name, float value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
    public class ValueModifier
    {
        public Dictionary<string, ValueModifierValue> Values = new Dictionary<string, ValueModifierValue>();
        protected Func<ValueModifier, GameObject, float, float> Modifier = (parent, mod, v) => v;
        public string Name = "";
        public object Tag;
        public Func<ValueModifier, string> Description = mod => "";
        public ValueModifier(string name, Func<ValueModifier, GameObject, float, float> modifier, params ValueModifierValue[] values)
        {
            this.Name = name;
            this.Modifier = modifier;
            foreach (var val in values)
                this.Values.Add(val.Name, val);
        }
        public ValueModifier(string name, Func<ValueModifier, GameObject, float, float> modifier, object tag)
            : this(name, modifier)
        {
            this.Tag = tag;
        }
        public float Modify(GameObject parent, float value)
        {
            return this.Modifier(this, parent, value);
        }
        public float GetValue(string value)
        {
            return this.Values[value].Value;
        }
        public override string ToString()
        {
            return this.Description(this);
        }
    }
}
