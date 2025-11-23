using System;

namespace Start_a_Town_
{
    class QuestRewardItem : QuestReward
    {
        public ObjectAmount Reward;

        public QuestRewardItem(QuestDef parent) : base(parent)
        {
        }
        public override string Text => $"{this.Reward.Object.Label} x{this.Reward.Amount}";
        public override int Budget => this.Reward.Object.GetValue() * this.Reward.Amount;
        public override string Label => this.Reward.Object.Label;
        public override int Count { get => this.Reward.Amount; set => this.Reward.Amount = value; }

        internal override void Award(Actor actor)
        {
            throw new NotImplementedException();
        }

        internal override bool CanAward()
        {
            throw new NotImplementedException();
        }
    }
}
