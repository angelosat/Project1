using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class NeedsComponent : EntityComponent
    {
        public override string Name { get; } = "Needs";
           
        float Timer = Ticks.PerSecond;
        public List<Need> NeedsNew;
        public NeedsComponent(Actor actor)
        {
            this.Parent = actor;
            var def = actor.Def;
            var defs = def.ActorProperties.Needs;
            var size = defs.Length;
            this.NeedsNew = new List<Need>(size);

            for (int i = 0; i < size; i++)
            {
                this.NeedsNew.Add(defs[i].Create(actor));
            }
        }
        
        public NeedsComponent()
        {
        }
        
        public void AddNeed(params NeedDef[] defs)
        {
            foreach (var d in defs)
                this.NeedsNew.Add(d.Create(this.Parent as Actor));
        }

        public override void Tick()
        {
            Timer -= 1;
            if (Timer > 0)
                return;
            var parent = this.Parent;

            Timer = Ticks.PerSecond;

            for (int i = 0; i < this.NeedsNew.Count; i++)
            {
                this.NeedsNew[i].Tick(parent);
            }
        }

        public override object Clone()
        {
            return new NeedsComponent(this.Parent as Actor);
        }

        static public Need ModifyNeed(GameObject actor, string needName, float value)
        {
            var need = actor.GetNeed(needName);
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefID, need.NeedDef, value);
            return need;
        }
        static public Need ModifyNeed(GameObject actor, NeedDef type, float value)
        {
            var need = actor.GetNeed(type);
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefID, need.NeedDef, value);
            return need;
        }

        internal override void GetManagementInterface(GameObject parent, UI.Control box)
        {
            box.AddControls(new NeedsUI(parent));
        }

        public void GetUI(GameObject parent, UI.Control container)
        {
            var box = new GroupBox();

            var byCategory = this.NeedsNew.GroupBy(n => n.NeedDef.CategoryDef);
            foreach (var cat in byCategory)
            {
                var panel = new PanelLabeled(cat.Key.Label) { Location = box.BottomLeft };
                foreach (var n in cat)
                {
                    var ui = n.GetUI(parent);
                    ui.Location = panel.Controls.BottomLeft;
                    panel.AddControls(ui);
                }
                box.AddControls(panel);
            }
            container.AddControls(box);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            this.NeedsNew.WriteAbstract(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.NeedsNew.Clear();
            this.NeedsNew.InitializeAbstract(r, this.Parent);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            this.NeedsNew.SaveAbstract(tag, "Needs");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.NeedsNew.Clear();
            this.NeedsNew.TryLoadVariableTypes(tag, "Needs", this.Parent);
        }
        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(NeedsComponent);
            public NeedDef[] Needs;
            public Props(params NeedDef[] defs)
            {
                this.Needs = defs;
            }
        }
    }
}
