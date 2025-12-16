using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class Inspectable : ILabeled
    {
        public virtual string Label => this.ToString();
        public virtual IEnumerable<(string item, object value)> Inspect()
        {
            var t = this.GetType();
            foreach (var field in t.GetFields().Where(p => !Attribute.IsDefined(p, typeof(InspectorHidden))))
                yield return (field.Name, field.GetValue(this));
            foreach (var field in t.GetProperties().Where(p=> !Attribute.IsDefined(p, typeof(InspectorHidden))))
                yield return (field.Name, field.GetValue(this));
        }
    }
    public interface IInspectable : ILabeled
    {
        IEnumerable<(string item, object value)> Inspect();
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class InspectorHidden : Attribute
    {
    }
}