using Start_a_Town_.UI;
using System;
using System.IO;
using System.Linq;

namespace Start_a_Town_.Components
{
    class AttributesComponent : EntityComponent
    {
        public override string Name { get; } = "Attributes";
        public AttributeStat[] Attributes;

        public AttributesComponent(ItemDef def)
        {
            var atts = def.ActorProperties.Attributes;
            var count = atts.Length;
            this.Attributes = new AttributeStat[count];
            for (int i = 0; i < count; i++)
            {
                this.Attributes[i] = new AttributeStat(atts[i]);
            }
        }
        public AttributesComponent()
        {
        }

        public AttributesComponent(params AttributeStat[] atts)
            : this()
        {
            var count = atts.Length;
            this.Attributes = new AttributeStat[count];
            for (int i = 0; i < count; i++)
            {
                this.Attributes[i] = atts[i].Clone();
            }
        }
        public AttributesComponent(params AttributeDef[] atts)
            : this()
        {
            this.Attributes = new AttributeStat[atts.Length];
            for (int i = 0; i < atts.Length; i++)
            {
                this.Attributes[i] = new AttributeStat(atts[i]);
            }
        }
        
        public override void Tick()
        {
            var parent = this.Parent;
            for (int i = 0; i < this.Attributes.Length; i++)
                this.Attributes[i].Update(parent);
        }

        public override object Clone()
        {
            return new AttributesComponent(this.Attributes);
        }

        public AttributeStat GetAttribute(AttributeDef def)
        {
            return this.Attributes.FirstOrDefault(att => att.Def == def);
        }

        static public AttributeStat GetAttribute(GameObject parent, AttributeDef type)
        {
            var comp = parent.GetComponent<AttributesComponent>();
            if (comp == null)
                return null;
            return comp.Attributes.FirstOrDefault(att => att.Def == type);
        }
        static public float GetValueOrDefault(GameObject parent, AttributeDef type, float dflt)
        {
            var comp = parent.GetComponent<AttributesComponent>();
            if (comp == null)
                return dflt;
            var found = comp.Attributes.FirstOrDefault(att => att.Def == type);
            return found != null ? found.Level : dflt;
        }
        TableScrollableCompact<AttributeStat> GUITableAttributes = new TableScrollableCompact<AttributeStat>()
                .AddColumn("name", "", 64, a => new Label(a.Def.Label)
                {
                    TooltipFunc = (t) =>
                    {
                        t.AddControlsBottomLeft(
                            new Label(a.Def.Description),
                            a.GetProgressControl());
                    }
                })
                .AddColumn("value", "", (int) UIManager.Font.MeasureString("###").X, a => new Label(() => a.Level.ToString()));
        public override GroupBox GetGUI()
        {
            GUITableAttributes.ClearItems();
            GUITableAttributes.AddItems(this.Attributes);
            return GUITableAttributes;
        }
        internal override void GetInterface(GameObject gameObject, Control box)
        {
            GUITableAttributes.ClearItems();
            GUITableAttributes.AddItems(this.Attributes);
            box.AddControlsBottomLeft(GUITableAttributes);
        }
        
        internal Control GetCreationGui()
        {
            var table = new TableScrollableCompact<AttributeStat>()
               .AddColumn(null, "name", 80, s => new Label(s.Def.Label), 0)
               .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.Attributes);
            return table;
        }

        static readonly ListBoxNoScroll GuiList = new();
        public Control GetGui()
        {
            //var win = GuiList.GetWindow();
            //if (win is null)
            //    win = GuiList.ToWindow("Skills");
            GuiList.Clear();
            GuiList.AddItems(this.Attributes);
            GuiList.Validate(true);
            return GuiList;//
            //win.Validate(true);
            //return win;
        }

        public AttributesComponent Randomize()
        {
            var range = 20;
            var average = range / 2;
            var values = RandomHelper.NextNormalsBalanced(this.Attributes.Length);
            for (int i = 0; i < this.Attributes.Length; i++)
            {
                var item = this.Attributes[i];
                item.Level = (int)(average * (1 + values[i]));
            }
            return this;
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.Attributes.SaveNewBEST(tag, "Attributes");
        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            this.Attributes.Sync(tag, "Attributes");
        }
        public override void Write(IDataWriter w)
        {
            this.Attributes.WriteNew(w);
        }
        public override void Read(IDataReader r)
        {
            this.Attributes.Read(r);
        }

        internal void Adjust(AttributeDef strength, float energyConsumption)
        {
            this.GetAttribute(AttributeDefOf.Strength).Award(this.Parent, energyConsumption);
        }

        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(AttributesComponent);
            public AttributeDef[] Items;
            public Props(params AttributeDef[] defs)
            {
                this.Items = defs;
            }
        }
    }
}
