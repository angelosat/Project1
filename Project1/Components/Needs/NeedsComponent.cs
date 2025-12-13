using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class NeedsComponent : EntityComp, IGui//<NeedsComponent>
    {
        public override string Name { get; } = "Needs";
           
        float Timer = Ticks.PerSecond;
        public List<Need> NeedsNew;
        public NeedsComponent(Actor actor)
        {
            this.Owner = actor;
            var def = actor.Def;
            var defs = def.ActorProperties.Needs;
            var size = defs.Length;
            this.NeedsNew = new List<Need>(size);

            for (int i = 0; i < size; i++)
            {
                //this.NeedsNew.Add(defs[i].Create(actor));
                this.NeedsNew.Add(new Need(actor, defs[i]));
            }
        }
        
        public NeedsComponent()
        {
        }
        
        public void AddNeed(params NeedDef[] defs)
        {
            foreach (var d in defs)
                //this.NeedsNew.Add(d.Create(this.Parent as Actor));
                this.NeedsNew.Add(new Need(this.Owner as Actor, d));
        }

        public override void Tick()
        {
            //Timer -= 1;
            //if (Timer > 0)
            //    return;

            //Timer = Ticks.PerSecond;

            for (int i = 0; i < this.NeedsNew.Count; i++)
                this.NeedsNew[i].Tick();// this.Parent);
        }

        public override object Clone()
        {
            return new NeedsComponent(this.Owner as Actor);
        }
        public override void Resolve()
        {
            foreach (var n in this.NeedsNew)
                n.Parent = this.Owner as Actor;
        }
        public override void OnObjectSynced(GameObject parent)
        {
            foreach (var n in this.NeedsNew)
                n.Parent = parent as Actor;
        }
        public override void OnObjectLoaded(GameObject parent)
        {
            foreach (var n in this.NeedsNew)
                n.Parent = parent as Actor;
        }
        static public Need ModifyNeed(GameObject actor, string needName, float value)
        {
            var need = actor.GetNeed(needName);
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefId, need.NeedDef, value);
            return need;
        }
        static public Need ModifyNeed(GameObject actor, NeedDef type, float value)
        {
            var need = actor.GetNeed(type);
            need.SetValue(need.Value + value, actor);
            if (actor.Net is Net.Server)
                PacketNeedModify.Send(actor.Net as Net.Server, actor.RefId, need.NeedDef, value);
            return need;
        }

        //internal override void GetManagementInterface(GameObject parent, UI.Control box)
        //{
        //    box.AddControls(new NeedsMoodsUI(parent));
        //}

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
        public void GetUI(UI.Control container)
        {
            var box = new GroupBox();

            var byCategory = this.NeedsNew.GroupBy(n => n.NeedDef.CategoryDef);
            foreach (var cat in byCategory)
            {
                var panel = new PanelLabeled(cat.Key.Label) { Location = box.BottomLeft };
                foreach (var n in cat)
                {
                    var ui = n.GetUI(this.Owner);
                    ui.Location = panel.Controls.BottomLeft;
                    panel.AddControls(ui);
                }
                box.AddControls(panel);
            }
            container.AddControls(box);
        }
        public override void Write(IDataWriter w)
        {
            this.NeedsNew.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.NeedsNew.Clear();
            this.NeedsNew.LoadFrom(r);
        }
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.NeedsNew.Save("Needs"));
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.NeedsNew.Clear();
            this.NeedsNew.LoadFrom(tag["Needs"]);
        }
        public void NewGui(GroupBox box)
        {
            var byCategory = this.NeedsNew.GroupBy(n => n.NeedDef.CategoryDef);
            foreach (var cat in byCategory)
            {
                var panel = new PanelLabeled(cat.Key.Label) { Location = box.BottomLeft };
                foreach (var n in cat)
                {
                    var ui = n.GetUI(this.Owner);
                    ui.Location = panel.Controls.BottomLeft;
                    panel.AddControls(ui);
                }
                box.AddControls(panel);
            }
        }

        public new class Props: Props<NeedsComponent>
        {
            public NeedDef[] Needs;
            public Props(params NeedDef[] defs)
            {
                this.Needs = defs;
            }
        }
    }
}
