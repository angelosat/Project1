using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework.Entities;
using Start_a_Town_;
using Start_a_Town_.UI;

namespace Project1.Framework.Needs
{
    public class NeedsComponent : EntityComp<NeedsComponent.Spec>, IGui
    {
        public override string Name { get; } = "Needs";
           
        float Timer = Ticks.PerSecond;
        //public List<Need> NeedsNew = [];
        public Dictionary<NeedDef, Need> NeedsNew = [];

        internal override void CopyFrom(EntityComp source)
        {
            var c = (NeedsComponent)source;
            //var count = c.NeedsNew.Count;
            //this.NeedsNew = new List<Need>(count);
            //for (int i = 0; i < count; i++)
            foreach(var n in c.NeedsNew.Values)
                this.NeedsNew.Add(n.NeedDef, new Need(this.Owner as Actor, n.NeedDef));
        }
        public NeedsComponent()
        {
        }
        public Need AddNeed(NeedDef def)
        {
            var need = new Need(this.Owner as Actor, def);
            this.NeedsNew.Add(def, need);
            return need;
        }
        public void Add(params NeedDef[] defs)
        {
            //this.NeedsNew = new(defs.Length);
            foreach (var d in defs)
                this.AddNeed(d);
                //this.NeedsNew.Add(d, new Need(this.Owner as Actor, d));
        }
        public void Remove(params NeedDef[] defs)
        {
            //this.NeedsNew.RemoveAll(n => defs.Contains(n.NeedDef));
            foreach (var d in defs)
                this.NeedsNew.Remove(d);
        }

        public override void Tick()
        {
            //for (int i = 0; i < this.NeedsNew.Count; i++)
            //    this.NeedsNew[i].Tick();// this.Parent);
            foreach (var n in this.NeedsNew.Values)
                n.Tick();
        }

        internal override void Resolve()
        {
            foreach (var n in this.NeedsNew.Values)
                n.Owner = this.Owner as Actor;
        }
        //public override void OnObjectSynced(GameObject parent)
        //{
        //    foreach (var n in this.NeedsNew.Values)
        //        n.Owner = parent as Actor;
        //}
        //public override void OnObjectLoaded(GameObject parent)
        //{
        //    foreach (var n in this.NeedsNew.Values)
        //        n.Owner = parent as Actor;
        //}
        //static public Need ModifyNeed(GameObject actor, string needName, float value)
        //{
        //    var need = actor.GetNeed(needName);
        //    need.SetValue(need.Value + value, actor);
        //    if (actor.Net is Net.Server)
        //        PacketNeedModify.Send(actor.Net as Net.Server, actor.RefId, need.NeedDef, value);
        //    return need;
        //}
        static public Need ModifyNeed(GameObject actor, NeedDef type, int value)
        {
            var need = actor.GetNeed(type);
            need.ApplyDelta(value);
            //need.SetValue(need.Value + value, actor);
            //if (actor.Net is Net.Server)
            //    PacketNeedModify.SendModify(actor.Net as Net.Server, actor.RefId, need.NeedDef, value);
            return need;
        }
        public void GetUI(GameObject parent, Control container)
        {
            var box = new GroupBox();

            var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
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
        public void GetUI(Control container)
        {
            var box = new GroupBox();

            var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
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
            //this.NeedsNew.Write(w);
            w.WriteValues(this.NeedsNew);
        }
        public override void Read(IDataReader r)
        {
            //this.NeedsNew.Clear();
            //this.NeedsNew.LoadFrom(r);
            //r.ReadValuesWithInferredKeys(this.NeedsNew, i => i.NeedDef);
            r.ReadDefWrappers(this.NeedsNew);
            this.Resolve();
        }
        internal override void SaveExtra(SaveTag tag)
        {
            //tag.Add(this.NeedsNew.Save("Needs"));
            tag.SaveValues(this.NeedsNew, "Needs");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            //this.NeedsNew.Clear();
            //this.NeedsNew.LoadFrom(tag["Needs"]);
            //tag["Needs"].LoadValuesWithInferredKeys(this.NeedsNew, n => n.NeedDef);
            tag.LoadDefWrappers("Needs", this.NeedsNew);
            this.Resolve();

        }
        void Rebuild()
        {
            this.NeedsNew.Clear();
            var profile = this.Owner.Profile as ActorDnaDef;
            var profileneeds = profile.Needs;
            var role = this.Owner.GetComponent<AIComponent>().Meta;
            var roleneeds = role.Def.Needs;
            var allneeds = profileneeds.Concat(roleneeds);
            foreach (var n in allneeds)
                this.Add(n);
        }
        public void NewGui(GroupBox box)
        {
            var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
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

        internal void OverridePercentage(NeedDef adventuring, float percentage)
        {
            var need = this.NeedsNew[adventuring];
            need.SetValue((int)(need.Max * percentage), this.Owner);
            this.Owner.World.Events.Post(new ActorNeedOverridenEvent(this.Owner as Actor, need.Def, need.Value));
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
                    comp.Add(this.Needs);
                    //comp.Needs = this.Needs;
                }
            }
        }
    }
}
