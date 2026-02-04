using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Net;
using Project1.Framework.WorldGen;
using System;
using System.IO;

namespace Start_a_Town_
{
    public partial class ReservationManager
    {
        class Reservation
        {
            public int Actor;
            public TargetArgs Target;
            int _Amount;
            public int Amount
            {
                get { return this._Amount; }
                set
                {
                    this._Amount = value;
                }

            }

            public int TaskID;
            public Plan Task { set { this.TaskID = value.ID; } }
            public override string ToString()
            {
                return string.Format("Actor: {0} Target: {1} Amount: {2}", this.Actor, this.Target, this.Amount);
            }
            
            public Reservation(GameObject actor, TargetArgs target, int stackcount)
            {
                if (stackcount == -1)
                    throw new Exception();
                this.Actor = actor.RefId;
                this.Target = target;
                this.Amount = stackcount;
                if (target.HasObject && stackcount > target.Object.StackSize)
                    throw new InvalidOperationException($"reservation quantity request exceeded target item's current stacksize");
            }
            public void Write(BinaryWriter w)
            {
                w.Write(this.Actor);
                this.Target.Write(w);
                w.Write(this.Amount);
                w.Write(this.TaskID);
            }
            public Reservation(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                this.Actor = r.ReadInt32();
                this.Target =  TargetArgs.Read(net, r);
                this.Amount = r.ReadInt32();
                this.TaskID = r.ReadInt32();
            }
            public SaveTag Save()
            {
                var tag = new SaveTag(SaveTag.Types.Compound);
                tag.Add(this.Actor.Save("ActorID"));
                tag.Add(this.Target.Save("Target"));
                tag.Add(this.Amount.Save("Amount"));
                return tag;
            }
            public Reservation(MapBase map, SaveTag tag)
            {
                this.Actor = tag.GetValue<int>("ActorID");
                this.Target = new TargetArgs(map.World, tag["Target"]);
                this.Amount = tag.GetValue<int>("Amount");
            }
        }
    }
}
