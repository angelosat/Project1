using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
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

        internal override void InitAction()
        {
            var actor = this.Actor;
            if (actor.Net.IsClient) return;
            var target = this.Target;
            base.InitAction();
            var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;

            actor.Inventory.Throw(velocity, amount: -1);
            //PacketActorThrowHauled.Send(actor, Vector3.Zero);
            return;

            var slot = actor.Inventory.HaulSlot;
            var obj = slot.Object;
            if (obj == null)
                throw new System.Exception();

            var all = this.All;
            var newobj = all ? obj : obj.TrySplitOne();

            /// GLOBAL DOESNT GET SET HERE BECAUSE THE OBJ STILL HAVE THE ACTOR AS THE PARENT AND RETURNS HIS GLOBAL
            //newobj.Global = actor.Global + new Vector3(0, 0, actor.Physics.Height); 
            ///
            var newGlobal = actor.Global + new Vector3(0, 0, actor.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            if (newobj != obj)
            {
                if (actor.Net is Net.Server server)
                {
                    newobj.SyncInstantiate(server);
                    actor.Map.SyncSpawn(newobj, newGlobal, velocity);
                }
            }
            else
                //newobj.Spawn(actor.Map, newGlobal);
                actor.Map.Spawn(newobj as Entity, newGlobal, Vector3.Zero);
            if (obj == newobj)
                slot.Clear();
        }

        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.All ? " All" : "");
        }

        public override object Clone()
        {
            return new InteractionThrow(this.All);
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
