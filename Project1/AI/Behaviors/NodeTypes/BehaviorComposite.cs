using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.AI.Behaviors
{
    abstract class BehaviorComposite : Behavior
    {
        internal List<Behavior> Children;
        internal Behavior Find(Type bhavType)
        {
            return this.Children.Find(t => t.GetType() == bhavType);
        }
        public BehaviorComposite(params Behavior[] children)
        {
            this.Children = new List<Behavior>(children);
        }
        public override void Write(IDataWriter w)
        {
            foreach (var child in this.Children)
                child.Write(w);
        }
        public override void Read(IDataReader r)
        {
            foreach (var child in this.Children)
                child.Read(r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            var childrenTag = new SaveTag(SaveTag.Types.List, "Children", SaveTag.Types.Compound);
            foreach (var b in this.Children)
            {
                childrenTag.Add(b.Save());
            }
            tag.Add(childrenTag);
        }
        
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);

            var childrenTags = tag["Children"].Value as List<SaveTag>;
            for (int i = 0; i < childrenTags.Count; i++)
            {
                var behavtag = childrenTags[i];
                var typename = behavtag["Type"].Value as string;
                var behav = this.Children.FirstOrDefault(c => c.GetType().FullName == typename);
                if (behav != null)
                    behav.Load(behavtag);
            }
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            foreach (var c in this.Children)
                c.ObjectLoaded(parent);
        }
    }
}
