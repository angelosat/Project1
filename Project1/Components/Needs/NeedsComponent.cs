using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public class NeedsComponent : EntityComp<NeedsComponent.Spec>, IGui
    {
        public override string Name { get; } = "Needs";
           
        float Timer = Ticks.PerSecond;
        public List<Need> NeedsNew;
      
        internal override void CopyFrom(EntityComp source)
        {
            var c = (NeedsComponent)source;
            var count = c.NeedsNew.Count;
            this.NeedsNew = new List<Need>(count);
            for (int i = 0; i < count; i++)
                this.NeedsNew.Add(new Need(null, c.NeedsNew[i].NeedDef));
        }
        public NeedsComponent()
        {
        }
        
        public void RegisterNeeds(params NeedDef[] defs)
        {
            this.NeedsNew = new(defs.Length);
            foreach (var d in defs)
                this.NeedsNew.Add(new Need(this.Owner as Actor, d));
        }

        public override void Tick()
        {
            for (int i = 0; i < this.NeedsNew.Count; i++)
                this.NeedsNew[i].Tick();// this.Parent);
        }

        internal override void Resolve()
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

        public new class Spec: Spec<NeedsComponent>
        {
            public NeedDef[] Needs;
            public Spec(params NeedDef[] defs)
            {
                this.Needs = defs;
            }
            protected override void ApplyDefaultsTo(NeedsComponent comp)
            {
                if (this.Needs != null)
                {
                    comp.RegisterNeeds(this.Needs);
                    //comp.Needs = this.Needs;
                }
            }
        }
    }
}
