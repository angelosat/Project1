using Project1.Framework.Serialization;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Interactions;
using System;

namespace Project1.Core.Towns
{
    class InteractionGiveItem : InteractionLogic
    {
        //bool Trade;

        //public InteractionGiveItem(bool trade) : base("GiveItem", seconds: .4f)
        //{
        //    this.Trade = trade;
        //    //this.AnimationDef = AnimationDef.TouchItem;
        //}
        //public InteractionGiveItem() : this(false)
        //{
        //}

        internal override void OnFinish(Interaction i)
        {
            var a = i.Actor;
            var t = i.Target;
            var item = a.Hauled as Entity;
            var seller = t.Object as Actor;
            var sellerCarriedItem = seller.Hauled as Entity;
            seller.Carry(item);
            throw new NotImplementedException();
            //if(this.Trade)
            //    a.Carry(sellerCarriedItem);
        }
        //protected override void WriteExtra(IDataWriter w)
        //{
        //    w.Write(this.Trade);
        //}
        //protected override void ReadExtra(IDataReader r)
        //{
        //    this.Trade = r.ReadBoolean();
        //}
    }
}
