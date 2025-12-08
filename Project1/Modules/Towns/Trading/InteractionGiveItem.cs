using System.IO;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionGiveItem : Interaction
    {
        bool Trade;
        public override object Clone()
        {
            return new InteractionGiveItem(this.Trade);
        }
        public InteractionGiveItem(bool trade) : base("GiveItem", seconds: .4f)
        {
            this.Trade = trade;
            this.Animation = new Animation(AnimationDef.TouchItem);
        }
        public InteractionGiveItem() : this(false)
        {
        }
       
        public override void Perform()
        {
            var a = this.Actor;
            var t = this.Target;
            var item = a.Hauled as Entity;
            var seller = t.Object as Actor;
            var sellerCarriedItem = seller.Hauled as Entity;
            seller.Carry(item);
            if(this.Trade)
                a.Carry(sellerCarriedItem);
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.Trade);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Trade = r.ReadBoolean();
        }
    }
}
