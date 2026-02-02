using Microsoft.Xna.Framework;
using Project1.Framework.Interactions;
using Start_a_Town_;

namespace Project1.Core.Interactions
{
    internal class InteractionThrowLogid : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient) return;
            var target = i.Context.Target;
            var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;
            // TODO use this.All to throw the whole item stack vs only one
            actor.Inventory.Throw(velocity, amount: -1);
        }
    }
    class InteractionThrow : Interaction
    {
        bool All;
        public InteractionThrow():this(true)
        {

        }
        public InteractionThrow(bool all)
            : base(
            "Throw",
            0)
        {
            this.All = all;
        }
      
        //protected override void Done()
        //{
        //    var actor = this.Actor;
        //    if (actor.Net.IsClient) return;
        //    var target = this.Target;
        //    var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;
        //    // TODO use this.All to throw the whole item stack vs only one
        //    actor.Inventory.Throw(velocity, amount: -1);
        //}
        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.All ? " All" : "");
        }

        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.All);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.All = r.ReadBoolean();
        }
    }
}
