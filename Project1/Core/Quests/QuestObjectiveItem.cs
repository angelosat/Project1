using Project1.Core.World.WorldAreas;
using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Interfaces;
using Project1.Framework.Legacy;
using Start_a_Town_;
using System.Collections.Generic;
using System.IO;

namespace Project1.Core.Quests
{
    class QuestObjectiveItem : QuestObjective
    {
        public ItemMaterialAmount Objective;
        public QuestObjectiveItem(QuestDef parent) : base(parent)
        {

        }
        public QuestObjectiveItem(QuestDef parent, ItemMaterialAmount requirement):base(parent)
        {
            Objective = requirement;
        }

        public override string Text => string.Format("Gather {0}", this.Objective.ToString());

        public override int GetValue()
        {
            return this.Objective.Item.BaseValue * this.Objective.Material.Value * this.Objective.Amount ;
        }
        public override void Write(IDataWriter w)
        {
            this.Objective.Write(w);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Objective = new ItemMaterialAmount(r);
            return this;
        }
        protected override void AddSaveData(SaveTag save)
        {
            save.Add(this.Objective.Save("Objective"));
        }
        protected override void Load(SaveTag load)
        {
            this.Objective = new(load["Objective"]);
        }

        public override bool IsCompleted(Actor actor)
        {
            return actor.Inventory.Count(this.Objective.Item, this.Objective.Material) >= this.Objective.Amount;
        }
        internal override void TryComplete(Actor actor, FrontierDef area)
        {
            if (this.IsCompleted(actor))
                return;
            var item = this.Objective.Item;
            var mat = this.Objective.Material;
            if (!area.CanBeFound(item, mat, out var chance))
                return;
            actor.Loot(item.CreateFrom(mat), area);
        }
        internal override IEnumerable<ObjectAmount> GetQuestItemsInInventory(Actor actor)
        {
            var inv = actor.Inventory;
            foreach(var i in inv.Take(e=> e.Def == this.Objective.Item && e.PrimaryMaterial == this.Objective.Material, this.Objective.Amount))
                yield return i;
        }
    }
}
