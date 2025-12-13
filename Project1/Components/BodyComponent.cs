using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class BodyComponent : EntityComp
    {
        public override string Name { get; } = "Body";
        

        public Dictionary<string, BodyPart> BodyParts;
       
        public BodyComponent()
        {
            this.BodyParts = new Dictionary<string, BodyPart>();
        }

        public BodyComponent Initialize(BodyComponent template)
        {
            this.BodyParts = template.BodyParts;
            return this;
        }

        public override object Clone()
        {
            BodyComponent comp = new BodyComponent();
            return comp;
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            foreach (var bodypart in this.BodyParts)
            {
                data.Add(new SaveTag(SaveTag.Types.Compound, bodypart.Key, (bodypart.Value as BodyPart).Save()));
            }
            return data;
        }

        internal override void LoadExtra(SaveTag compTag)
        {
            foreach (SaveTag tag in (compTag.Value as Dictionary<string, SaveTag>).Values)
                if (tag.Value != null)
                    this.BodyParts[tag.Name] = BodyPart.Load(tag);
        }
    }
}
