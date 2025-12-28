using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components
{
    class AttributesComponent : EntityComp<AttributesComponent.Spec>
    {
        public override string Name { get; } = "Attributes";
        //public AttributeRuntime[] Attributes;
        public Dictionary<AttributeDef, AttributeRuntime> Attributes = [];
        public AttributesComponent()
        {
        }
        internal override void CopyFrom(EntityComp source)
        {
            var atts = ((AttributesComponent)source).Attributes.Values;
            //var count = atts.Length;
            //this.Attributes = new AttributeRuntime[count];
            //for (int i = 0; i < count; i++)
            //    this.Attributes[i] = atts[i].Clone();
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
                .AddColumn("name", "", 64, a => new Label(a.AttributeDef.Label)
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
               .AddColumn(null, "name", 80, s => new Label(s.AttributeDef.Label), 0)
               .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.Attributes.Values);
            return table;
        }

        static readonly ListBoxNoScroll GuiList = new();
        public Control GetGui()
        {
            //var win = GuiList.GetWindow();
            //if (win is null)
            //    win = GuiList.ToWindow("Skills");
            GuiList.Clear();
            GuiList.AddItems(this.Attributes.Values);
            GuiList.Validate(true);
            return GuiList;//
            //win.Validate(true);
            //return win;
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
            //this.Attributes.SaveNewBEST(tag, "Attributes");
            //tag.SaveValues(this.Attributes, "Attributes");
            tag.SaveDefWrappers("Attributes", this.Attributes);
        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            //this.Attributes.Sync(tag, "Attributes");
            //tag["Attributes"].LoadValuesWithInferredKeys(this.Attributes, a => a.AttributeDef);
            tag.LoadDefWrappers("Attributes", this.Attributes);
        }
        public override void Write(IDataWriter w)
        {
            //this.Attributes.Write(w);
            w.WriteValues(this.Attributes);
        }
        public override void Read(IDataReader r)
        {
            //this.Attributes.Read(r);
            r.ReadDefWrappers(this.Attributes);
        }

        internal void Adjust(AttributeDef strength, float energyConsumption)
        {
            this.GetAttribute(AttributeDefOf.Strength).Award(this.Owner, energyConsumption);
        }
        internal void AdjustAndSync(AttributeDef def, float v)
        {
            this.Adjust(def, v);
            AttributeRuntime.Packets.SendAdjust(this.Owner as Actor, def, v);
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
                    //comp.Attributes = new AttributeRuntime[this.Items.Length];
                    //for (int i = 0; i < this.Items.Length; i++)
                    //    comp.Attributes[i] = new AttributeRuntime(this.Items[i]);
                    foreach (var a in this.Items)
                        comp.Add(a);
                }
            }
        }
    }
}
