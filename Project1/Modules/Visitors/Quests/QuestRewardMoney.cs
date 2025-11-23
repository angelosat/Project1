using System;
using System.Linq;

namespace Start_a_Town_
{
    class QuestRewardMoney : QuestReward
    {
        public int Amount;
        public QuestRewardMoney(QuestDef parent, int amount) : base(parent)
        {
            this.Amount = amount;
        }

        public override string Text => ItemDefAmount.GetText(ItemDefOf.Coins, this.Amount);
        public override int Count { get => this.Amount; set { this.Amount = Math.Max(0, value); this.Parent.Manager.QuestModified(this.Parent); } }

        public override int Budget => this.Count;

        public override string Label => ItemDefOf.Coins.Label;

        internal override void Award(Actor actor)
        {
            this.AwardFromTownStockpiles(actor);
        }
        void AwardFromTownStockpiles(Actor actor)
        {
            var qgiver = this.Parent.Giver;
            var moneyItems = qgiver.Town.Storage
                .FindItems(e => e.Def == ItemDefOf.Coins, this.Amount);
            if (!moneyItems.Any())
                throw new Exception();
            foreach(var i in moneyItems)
            {
                if (i.amount < i.item.StackSize)
                {
                    var split = i.item.Split(i.amount);
                    if (qgiver.Net is Net.Server server)
                    {
                        split.SyncInstantiate(server);
                        actor.Inventory.SyncInsert(split);
                    }
                }
                else
                    actor.Inventory.Insert(i.item);
            }
        }

        internal override bool CanAward()
        {
            var qgiver = this.Parent.Giver;
            return qgiver.Town.Storage
                .FindItems(e => e.Def == ItemDefOf.Coins, this.Amount)
                .Sum(i => i.amount) 
                >= 
                this.Amount;
        }
    }
}
