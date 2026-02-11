using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Core.Entities;
using Project1.Core.Helpers;

namespace Project1.Core.Attributes
{
    class AttributesComponent : EntityComp<AttributesComponent.Spec>
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Attributes;
        public override string Name { get; } = "Attributes";
        public Dictionary<AttributeDef, AttributeRuntime> Attributes = [];
        public AttributeRuntime this[AttributeDef def] => this.GetAttribute(def);
        public AttributesComponent()
        {
        }
        internal override void CopyFrom(EntityComp source)
        {
            var atts = ((AttributesComponent)source).Attributes.Values;
            this.Attributes.Clear();
            foreach (var a in atts)
                this.Attributes.Add(a.AttributeDef, new AttributeRuntime(a.AttributeDef, (int)a.Progress.Value));
            this.Randomize();
        }
        public void Add(AttributeDef def)
        {
            this.Attributes.Add(def, new AttributeRuntime(def));
        }
        public override void Tick()
        {
            //for (int i = 0; i < this.Attributes.Length; i++)
            //    this.Attributes[i].Update(this.Owner);
            foreach (var a in this.Attributes.Values)
                a.Update(this.Owner);
        }
        public AttributeRuntime GetAttribute(AttributeDef def)
        {
            return this.Attributes[def];//.FirstOrDefault(att => att.Def == def);
        }

        TableScrollableCompact<AttributeRuntime> GUITableAttributes = new TableScrollableCompact<AttributeRuntime>()
                .AddColumn("name", "", 64, a => new Label(a.AttributeDef.LabelReadable)
                {
                    TooltipFunc = (t) =>
                    {
                        t.AddControlsBottomLeft(
                            new Label(a.AttributeDef.Description),
                            a.GetProgressControl());
                    }
                })
                .AddColumn("value", "", (int) UIManager.Font.MeasureString("###").X, a => new Label(() => a.Level.ToString()));
        public override GroupBox GetGUI()
        {
            GUITableAttributes.ClearItems();
            GUITableAttributes.AddItems(this.Attributes.Values);
            return GUITableAttributes;
        }
        internal override void GetInterface(GameObject gameObject, Control box)
        {
            GUITableAttributes.ClearItems();
            GUITableAttributes.AddItems(this.Attributes.Values);
            box.AddControlsBottomLeft(GUITableAttributes);
        }
        
        internal Control GetCreationGui()
        {
            var table = new TableScrollableCompact<AttributeRuntime>()
               .AddColumn(null, "name", 80, s => new Label(s.AttributeDef.LabelReadable), 0)
               .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.Attributes.Values);
            return table;
        }

        static readonly ListBoxNoScroll GuiList = new();
        public Control GetGui()
        {
            GuiList.Clear();
            GuiList.AddItems(this.Attributes.Values);
            GuiList.Validate(true);
            return GuiList;
        }
        public AttributesComponent Randomize()
        {
            var range = 20;
            var average = range / 2;
            var snapshot = this.Attributes.Values.ToList();
            var values = RandomHelper.NextNormalsBalanced(snapshot.Count);
            for (int i = 0; i < snapshot.Count; i++)
            {
                var item = snapshot[i];
                item.Level = (int)(average * (1 + values[i]));
            }
            return this;
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.SaveDefWrappers("Attributes", this.Attributes);
        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            tag.LoadDefWrappers("Attributes", this.Attributes);
        }
        public override void Write(IDataWriter w)
        {
            w.WriteValues(this.Attributes);
        }
        public override void Read(IDataReader r)
        {
            r.ReadDefWrappers(this.Attributes);
        }

        internal void ApplyDelta(AttributeDef def, float energyConsumption)
        {
            this.GetAttribute(def).Award(this.Owner, energyConsumption);
        }
        internal void SetValue(AttributeDef def, float value)
        {
            this.GetAttribute(def).SetValue(value);
        }
        public new class Spec : Spec<AttributesComponent>
        {
            public AttributeDef[] Items;
            public Spec(params AttributeDef[] defs)
            {
                this.Items = defs;
            }
            protected override void ApplyDefaultsTo(AttributesComponent comp)
            {
                if (this.Items != null)
                {
                    foreach (var a in this.Items)
                        comp.Add(a);
                }
            }
        }
    }
}
