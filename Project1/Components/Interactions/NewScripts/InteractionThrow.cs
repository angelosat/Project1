using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

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
            var target = this.Target;
            base.InitAction();
            
            var slot = actor.Inventory.GetHauling();
            var obj = slot.Object;
            if (obj == null)
                return;
            var all = this.All;
            var newobj = all ? obj : obj.TrySplitOne();
            var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;

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
                newobj.Spawn(actor.Map, newGlobal);
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
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.All);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.All = r.ReadBoolean();
        }
    }
}
