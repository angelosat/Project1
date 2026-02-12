using Project1.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Project1.Framework
{
    public abstract class Inspectable : ILabeled
    {
        public virtual string LabelReadable => this.ToString();
        public virtual IEnumerable<(string item, object value)> Inspect()
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var t = this.GetType();
            foreach (var field in t.GetFields(flags).Where(p => !Attribute.IsDefined(p, typeof(InspectorHidden))))
                yield return (field.Name, field.GetValue(this));
            foreach (var field in t.GetProperties(flags).Where(p=> !Attribute.IsDefined(p, typeof(InspectorHidden))))
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